//===---------------------------------------------------===//
//                  FileSystemWatcher.cs
//
// this file is under the MIT License
// See https://opensource.org/licenses/MIT for license information.
// Copyright(c) 2020-2022 moe-org All rights reserved.
//
//===---------------------------------------------------===//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace EightLeggedEssay.Cmdlet
{

    /// <summary>
    /// 创建一个新的文件系统监视器
    /// </summary>
    [Cmdlet(VerbsCommon.New, "FileSystemWatcher")]
    [OutputType(typeof(FileSystemWatcher))]
    public class CreateFileWatcher : PSCmdlet
    {
        public const string CallName = "New-FileSystemWatcher";

        [Parameter(Position = 0,Mandatory = true,ValueFromPipeline = true)]
        public string? Path { get; set; }

        protected override void ProcessRecord()
        {
            FileSystemWatcher fileSystem = new(Path ?? throw new ArgumentNullException(nameof(Path)));
            fileSystem.IncludeSubdirectories = true;
            fileSystem.NotifyFilter = 
                                   NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;
            WriteObject(fileSystem);
        }
    }

    /// <summary>
    /// 设置文件系统管理器信息
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "FileSystemWatcherSettings")]
    public class AddFileWatcherHandle : PSCmdlet
    {
        public const string CallName = "Set-FileSystemWatcherSettings";

        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public FileSystemWatcher FileWatcher { get; set; } = null!;

        [Parameter(Mandatory = false)]
        public SwitchParameter OnChanged { get; set; } = new(false);

        [Parameter(Mandatory = false)]
        public SwitchParameter OnDeleted { get; set; } = new(false);

        [Parameter(Mandatory = false)]
        public SwitchParameter OnCreated { get; set; } = new(false);

        [Parameter(Mandatory = false)]
        public SwitchParameter OnError { get; set; } = new(false);

        [Parameter(Mandatory = false)]
        public SwitchParameter OnRenamed { get; set; } = new(false);

        [Parameter(Mandatory = false)]
        public SwitchParameter NoIncludeSubdirectories { get; set; } = new(false);

        [Parameter(Mandatory = false)]
        public string? Fillter { get; set; } = null;

        protected override void ProcessRecord()
        {
            if(OnChanged.IsPresent)
            {
                FileWatcher.Changed += (obj,param) =>
                {
                    var eve = Events.GenerateEvent("FileSystemWatcher.OnChanged",obj,null,new PSObject(param),true,true);
                    Printer.PutLine("{0}:file system event received:{1}",Thread.CurrentThread.Name,param.FullPath);
                };
            }
            if (OnCreated.IsPresent)
            {
                FileWatcher.Created += (obj, param) =>
                {
                    var eve = Events.GenerateEvent("FileSystemWatcher.OnCreated", obj, null, new PSObject(param), true, true);
                    Printer.PutLine("{0}:file system event received:{1}", Thread.CurrentThread.Name, param.FullPath);

                };
            }
            if (OnDeleted.IsPresent)
            {
                FileWatcher.Deleted += (obj, param) =>
                {
                    var eve = Events.GenerateEvent("FileSystemWatcher.OnDeleted", obj, null, new PSObject(param), true, true);
                    Printer.PutLine("{0}:file system event received:{1}", Thread.CurrentThread.Name, param.FullPath);
                };
            }
            if (OnRenamed.IsPresent)
            {
                FileWatcher.Renamed += (obj, param) =>
                {
                    var eve = Events.GenerateEvent("FileSystemWatcher.OnRenamed", obj, null, new PSObject(param), true, true);
                    Printer.PutLine("{0}:file system event received:{1}", Thread.CurrentThread.Name, param.FullPath);
                };
            }
            if (OnError.IsPresent)
            {
                FileWatcher.Error += (obj, param) =>
                {
                    var eve = Events.GenerateEvent("FileSystemWatcher.OnError", obj, null, new PSObject(param), true, true);
                    Printer.PutLine("{0}:file system event received:{1}", Thread.CurrentThread.Name, param.ToString());
                };
            }
            if (NoIncludeSubdirectories.IsPresent)
            {
                FileWatcher.IncludeSubdirectories = false;
            }
            if (Fillter != null)
            {
                FileWatcher.Filters.Add(Fillter);
            }
            WriteObject(FileWatcher);
        }
    }

    /// <summary>
    /// 创建一个新的文件系统监视器
    /// </summary>
    [Cmdlet(VerbsLifecycle.Start, "FileSystemWatcher")]
    [OutputType(typeof(FileSystemWatcher))]
    public class BeginFileWatcher : PSCmdlet
    {
        public const string CallName = "Start-FileSystemWatcher";

        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public FileSystemWatcher FileWatcher { get; set; } = null!;

        protected override void ProcessRecord()
        {
            FileWatcher.EnableRaisingEvents = true;
            WriteObject(FileWatcher);
        }
    }

    /// <summary>
    /// 创建一个新的文件系统监视器
    /// </summary>
    [Cmdlet(VerbsLifecycle.Stop, "FileSystemWatcher")]
    public class EndFileWatcher : PSCmdlet
    {
        public const string CallName = "Stop-FileSystemWatcher";

        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public FileSystemWatcher FileWatcher { get; set; } = null!;

        protected override void ProcessRecord()
        {
            FileWatcher.EnableRaisingEvents = false;
            FileWatcher.Dispose();
        }
    }
}
