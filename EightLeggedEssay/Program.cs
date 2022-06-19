//===---------------------------------------------------===//
//                  Program.cs
//
// this file is under the MIT License
// See https://opensource.org/licenses/MIT for license information.
// Copyright(c) 2020-2022 moe-org All rights reserved.
//
//===---------------------------------------------------===//

using EightLeggedEssay.Cmdlet.Html;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EightLeggedEssay
{
    /// <summary>
    /// entry class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 打印帮助信息
        /// </summary>
        static void PrintHelp()
        {
            Console.WriteLine("usage:EightLeggedEssay [--options] -- [command options]");
            Console.WriteLine("options:");
            Console.WriteLine("\t--server path  :start a http server in path,default in output path");
            Console.WriteLine("\t--config path  :set the path to load config file");
            Console.WriteLine("\t--system path  :set the path of EightLeggedEssay system module");
            Console.WriteLine("\t--repl         :entry the repl mode");
            Console.WriteLine("\t--help         :print help then exit with success");
            Console.WriteLine("\t--debug        :entry debug mode");
            Console.WriteLine("\t--new    path  :create a new site in path then exit");
            Console.WriteLine("\t--run  command :execute a command that defined by configuration file");
            Console.WriteLine("\tthe arguments after `--` will send to the `--run command`");
        }

        /// <summary>
        /// 是否处在debug模式。这个值在程序正式开始运行后应该不变
        /// </summary>
        public static bool DebugMode { get; private set; } = false;

        /// <summary>
        /// 服务器地址，null代表不启动服务器
        /// </summary>
        public static string? ServerPath { get; set; } = null;

        /// <summary>
        /// 配置文件地址
        /// </summary>
        public static string ConfigurationPath { get; set; } = "./EightLeggedEssay.json";

        /// <summary>
        /// 执行的命令。如果不是处在命令执行模式则为null
        /// </summary>
        public static string? ExecuteCommand { get; private set; } = null;

        /// <summary>
        /// 执行命令的参数
        /// </summary>
        public static List<string> CommandArguments { get; private set; } = new();

        /// <summary>
        /// entry function
        /// </summary>
        /// <param name="args">command line arguments</param>
        public static int Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            bool repl = false;

            // 解析参数
            Thread.CurrentThread.Name = "main";

            int index = 0;

            while (index != args.Length)
            {
                string arg = args[index];

                if (arg == "--help")
                {
                    PrintHelp();
                    return 0;
                }
                else if (arg == "--repl")
                {
                    repl = true;
                }
                else if (arg == "--")
                {
                    index++;
                    if (index != args.Length)
                    {
                        CommandArguments.AddRange(args[index..]);
                    }
                    break;
                }
                else if (arg == "--debug")
                {
                    DebugMode = true;
                }
                else if (arg == "--server")
                {
                    index++;

                    if (index == args.Length)
                    {
                        Printer.ErrLine("Miss option for --server");
                    }
                    else
                    {
                        ServerPath = args[index];
                    }
                }
                else if (arg == "--config")
                {
                    index++;

                    if (index == args.Length)
                    {
                        Printer.ErrLine("Miss option for --config");
                    }
                    else
                    {
                        ConfigurationPath = args[index];
                    }
                }
                else if (arg == "--system")
                {
                    index++;

                    if (index == args.Length)
                    {
                        Printer.ErrLine("Miss option for --config");
                    }
                    else
                    {
                        ScriptEngineManager.SystemModulePath = args[index];
                    }
                }
                else if (arg == "--run")
                {
                    index++;

                    if (index == args.Length)
                    {
                        Printer.ErrLine("Miss option for --command");
                    }
                    else
                    {
                        ExecuteCommand = args[index];
                    }
                }
                else if (arg == "--new")
                {
                    index++;

                    if (index == args.Length)
                    {
                        Printer.ErrLine("Miss option for --new");
                    }
                    else
                    {
                        var createPath = args[index];

                        if (!Directory.Exists(createPath))
                        {
                            Directory.CreateDirectory(createPath);
                        }

                        Configuration.GlobalConfiguration.Commands.Add("new", "new.ps1");

                        Configuration.SaveTo(Path.Join(createPath, ConfigurationPath));

                        Directory.CreateDirectory(Path.Join(createPath, Configuration.GlobalConfiguration.OutputDirectory));
                        Directory.CreateDirectory(Path.Join(createPath, Configuration.GlobalConfiguration.SourceDirectory));
                        Directory.CreateDirectory(Path.Join(createPath, Configuration.GlobalConfiguration.ContentDirectory));
                        Directory.CreateDirectory(Path.Join(createPath, Configuration.GlobalConfiguration.ThemeDirectory));

                        File.Create(Path.Join(createPath, Configuration.GlobalConfiguration.BuildScript)).Close();

                        File.WriteAllText(Path.Join(createPath, "new.ps1"),
                            @"
# create a new poster
if($args.Length -ne 1){
    Write-Error ""input a argument as poster relative path to create a new poster""
    return
}

$config = Get-EleVariable Configuration | ConvertFrom-Json

$File = [System.IO.Path]::GetFullPath(($config.ContentDirectory) + ""/"" + ($args[0]))

$NewPosterHeader = @{ }

$NewPosterHeader[""Title""] = $File.Name
$NewPosterHeader[""CreateTime""] = Get-Date

New-Item -Path $File -ItemType file

(""<!--INFOS--`n{0}`n--INFOS-->`n`n#Hello World!`n"" -f ($NewPosterHeader | ConvertTo-Json)) | Out-File -FilePath $File -Encoding ""UTF-8""

                        ".Trim());

                        return 0;
                    }
                }
                else
                {
                    Printer.ErrLine($"unknown options:{arg}");
                    return 1;
                }
                index++;
            }
            // 检查参数
            if (repl && ExecuteCommand != null)
            {
                Printer.ErrLine($"entry repl mode and execute command at same time!");
                return 1;
            }
            if (ExecuteCommand == null && CommandArguments.Count != 0)
            {
                Printer.ErrLine($"not execute any command but added command arguments");
                return 1;
            }

            // 加载配置文件
            Configuration.ReadFrom(ConfigurationPath);
            var config = Configuration.GlobalConfiguration;

            // 构建
            var clock = new Stopwatch();

            clock.Start();
            {
                var engine = ScriptEngine.GetEngine("main");

                engine.Open();
                {
                    Printer.OkLine("powered by PowerShell v{0}", engine.Shell.Runspace.Version);

                    // 构建脚本
                    if (!repl && ExecuteCommand == null)
                    {
                        var command = Path.GetFullPath(config.BuildScript);
                        Printer.OkLine("execute:{0}", command);

                        engine.Shell.AddCommand(command).Invoke();
                    }
                    // 执行命令
                    else if (ExecuteCommand != null)
                    {
                        if (config.Commands.TryGetValue(ExecuteCommand, out string? script))
                        {
                            var command = Path.GetFullPath(script);
                            Printer.OkLine("execute:{0}", command);

                            engine.Shell.AddCommand(command)
                                .AddParameters(CommandArguments)
                                .Invoke();
                        }
                        else
                        {
                            Printer.ErrLine("unknown command:{0}", ExecuteCommand);
                            return -1;
                        }
                    }
                    // 进入repl循环
                    else
                    {
                        Printer.OkLine("entry repl mode");

                        string? cmd;
                        Printer.Ok("> ");
                        while (!string.IsNullOrEmpty(cmd = Console.ReadLine()))
                        {
                            try
                            {
                                var results = engine.Shell.AddScript(cmd).Invoke();

                                engine.Shell.Commands.Clear();

                                foreach (var result in results)
                                {
                                    Console.WriteLine("{0}", result);
                                }
                            }
                            catch (Exception err) {
                                Printer.ErrLine("{0}", err.ToString());
                            }

                            Printer.Ok("> ");
                        }
                    }
                }
                engine.Close();
            }
            clock.Stop();

            Printer.OkLine("cost {0}s", clock.Elapsed.TotalSeconds);

            return 0;
        }

    }
}
