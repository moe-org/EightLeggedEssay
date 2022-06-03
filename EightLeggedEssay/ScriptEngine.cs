//===---------------------------------------------------===//
//                  Engine.cs
//
// this file is under the MIT License
// See https://opensource.org/licenses/MIT for license information.
// Copyright(c) 2020-2022 moe-org All rights reserved.
//
//===---------------------------------------------------===//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.ComponentModel;
using System.Reflection;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using System.Reflection.Metadata;
using System.Xml.Linq;
using System.Drawing.Drawing2D;

namespace EightLeggedEssay
{
    /// <summary>
    /// 脚本引擎管理器。使用ThreadLocal来管理脚本缓存
    /// </summary>
    public static class ScriptEngineManager
    {
        /// <summary>
        /// 系统模块路径
        /// </summary>
        public static string SystemModulePath =
            Path.Join(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? "."), "EightLeggedEssayModule/EightLeggedEssay.psd1");

        private static readonly ThreadLocal<InitialSessionState> SessionStates = new();

        private static readonly ThreadLocal<Runspace> Runspaces = new();

        private static readonly ThreadLocal<PowerShell> PowerShells = new();

        private static void InitSessionState(InitialSessionState session)
        {
            // 初始化
            session.ThreadOptions = PSThreadOptions.UseCurrentThread;
            session.ThrowOnRunspaceOpenError = true;

            // 添加cmdlet
            void AddCmdlet(string name, Type typed)
            {
                SessionStateCmdletEntry cmdlet = new(name, typed, null);

                session.Commands.Add(cmdlet);
            }

            AddCmdlet(Cmdlet.GetVariable.CallName, typeof(Cmdlet.GetVariable));

            AddCmdlet(Cmdlet.NewThreadJobManager.CallName, typeof(Cmdlet.NewThreadJobManager));
            AddCmdlet(Cmdlet.StartThreadJob.CallName, typeof(Cmdlet.StartThreadJob));
            AddCmdlet(Cmdlet.WaitThreadJob.CallName, typeof(Cmdlet.WaitThreadJob));

            AddCmdlet(Cmdlet.TemplateEngine.CompileScriban.CallName, typeof(Cmdlet.TemplateEngine.CompileScriban));
            AddCmdlet(Cmdlet.TemplateEngine.GetScribanTable.CallName, typeof(Cmdlet.TemplateEngine.GetScribanTable));

            AddCmdlet(Cmdlet.Markdown.CompileMarkdown.CallName, typeof(Cmdlet.Markdown.CompileMarkdown));
            AddCmdlet(Cmdlet.Markdown.CompileMarkdownPoster.CallName, typeof(Cmdlet.Markdown.CompileMarkdownPoster));

            // 添加系统模块
            session.ImportPSModule(SystemModulePath);

            // 设置执行策略
            session.ExecutionPolicy = Microsoft.PowerShell.ExecutionPolicy.Unrestricted;
        }


        /// <summary>
        /// 获取初始化状态
        /// </summary>
        /// <returns></returns>
        private static InitialSessionState GetSessionState()
        {
            if (SessionStates.Value == null)
            {
                var session = InitialSessionState.CreateDefault();
                SessionStates.Value = session;

                InitSessionState(session);

                return session;
            }
            else
            {
                return SessionStates.Value;
            }
        }

        /// <summary>
        /// 获取运行空间
        /// </summary>
        /// <returns>运行空间</returns>
        private static Runspace GetRunspace()
        {
            if (Runspaces.Value == null)
            {
                var space = RunspaceFactory.CreateRunspace(GetSessionState());

                Runspaces.Value = space;

                space.Open();

                space.SessionStateProxy.LanguageMode = PSLanguageMode.FullLanguage;

                return space;
            }
            else
            {
                return Runspaces.Value;
            }
        }

        /// <summary>
        /// 获取脚本引擎
        /// </summary>
        /// <param name="pwsh">获取到的脚本引擎</param>
        /// <returns>如果获取到的脚本引擎是新的（需要初始化），则返回true</returns>
        public static bool GetEngine(out PowerShell pwsh)
        {
            if (PowerShells.Value != null)
            {
                pwsh = PowerShells.Value;

                return false;
            }
            else
            {
                pwsh = PowerShell.Create(GetRunspace());
                PowerShells.Value = pwsh;

                return true;
            }
        }
    }

    /// <summary>
    /// 脚本引擎
    /// </summary>
    public class ScriptEngine
    {
        private static readonly ThreadLocal<ScriptEngine> ThreadLocalEngine = new();

        /// <summary>
        /// 是否已经开启
        /// </summary>
        private bool open = false;

        private string name;

        private string? createLocation;

        private PowerShell pwsh;

        /// <summary>
        /// 使用EngineName作为name的访问器,只有在open成功的时候此才会被赋值。
        /// 避免直接访问name，在调用多次构造函数的时候修改输出信息。
        /// </summary>
        private string EngineName { get; set; }

        /// <summary>
        /// 使用EngineCreateLocation作为createLocation的访问器,只有在open成功的时候此才会被赋值。
        /// 避免直接访问createLocation，在调用多次构造函数的时候修改输出信息。
        /// </summary>
        private string? EngineCreateLocation { get; set; }

        public PowerShell Shell
        {
            get
            {
                if (open)
                {
                    return pwsh;
                }
                else
                {
                    throw new InvalidOperationException("ScriptEngine not open");
                }
            }
        }

        private ScriptEngine(string name, string? createLocation, PowerShell pwsh)
        {
            this.name = name;
            this.createLocation = createLocation;
            this.pwsh = pwsh;
            EngineName = name;
        }

        /// <summary>
        /// 创建一个新脚本引擎
        /// </summary>
        /// <param name="name">脚本引擎名称</param>
        /// <param name="createLocation">脚本引擎创建源代码地址，如果为null则没有</param>
        public static ScriptEngine GetEngine(string name, string? createLocation = null)
        {
            ScriptEngine engine;

            if (ThreadLocalEngine.Value == null)
            {
                var initNeeded = ScriptEngineManager.GetEngine(out PowerShell pwsh);

                ThreadLocalEngine.Value = new ScriptEngine(name, createLocation, pwsh);

                engine = ThreadLocalEngine.Value;

                if (initNeeded)
                {
                    pwsh.Streams.Error.DataAdded += (sender, args) =>
                    {
                        ErrorRecord err = ((PSDataCollection<ErrorRecord>)sender!)[args.Index];

                        Printer.ErrLine("{0}:{1}\n{2}{3}",
                            engine.EngineName,
                            err.Exception.ToString(),
                            err.ScriptStackTrace,
                             !string.IsNullOrEmpty(engine.EngineCreateLocation)
                             ? "\ncreated:" + engine.EngineCreateLocation : string.Empty);
                    };
                    pwsh.Streams.Warning.DataAdded += (sender, args) =>
                    {
                        WarningRecord warning = ((PSDataCollection<WarningRecord>)sender!)[args.Index];

                        Printer.WarnLine("{0}:{1}",
                            engine.EngineName,
                            warning.Message);
                    };
                    pwsh.Streams.Progress.DataAdded += (sender, args) =>
                    {
                        ProgressRecord progress = ((PSDataCollection<ProgressRecord>)sender!)[args.Index];
                        Printer.PutLine("{0}:{1}", engine.EngineName, progress.ToString());
                    };
                    pwsh.Streams.Information.DataAdded += (sender, args) =>
                    {
                        InformationRecord information = ((PSDataCollection<InformationRecord>)sender!)[args.Index];
                        Printer.PutLine("{0}:{1}", engine.EngineName, information.ToString());
                    };
                    pwsh.Streams.Verbose.DataAdded += (sender, args) =>
                    {
                        VerboseRecord verbose = ((PSDataCollection<VerboseRecord>)sender!)[args.Index];
                        Printer.PutLine("{0}:{1}", engine.EngineName, verbose.ToString());
                    };
                }
            }
            else
            {
                engine = ThreadLocalEngine.Value;
            }

            engine.name = name;
            engine.createLocation = createLocation;

            return engine;
        }

        /// <summary>
        /// 开启脚本引擎
        /// </summary>
        public void Open()
        {
            if (open)
            {
                throw new InvalidOperationException("open ScriptEngin when it was open");
            }
            open = true;

            pwsh.Stop();

            // 设置engine输出信息
            EngineName = name;
            EngineCreateLocation = createLocation ?? string.Empty;
        }

        /// <summary>
        /// 关闭脚本引擎
        /// </summary>
        public void Close()
        {
            if (!open)
            {
                throw new InvalidOperationException("close ScriptEngin when it is closed");
            }
            open = false;

            pwsh.Stop();
            // 重置状态以供下次使用
            pwsh.Runspace.ResetRunspaceState();
            pwsh.Commands.Clear();
        }

        /// <summary>
        /// 当前脚本引擎是否处在open状态
        /// </summary>
        public bool IsOpen()
        {
            return open;
        }
    }

}
