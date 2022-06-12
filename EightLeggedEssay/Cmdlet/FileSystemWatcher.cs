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
            FileSystemWatcher fileSystem = new();
            fileSystem.BeginInit();
            fileSystem.Path = Path ?? throw new ArgumentNullException(nameof(Path));
            WriteObject(fileSystem);
        }
    }

    /// <summary>
    /// 设置文件系统管理器信息
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "FileSystemWatcherSettings")]
    public class AddFileWatcherHandle : PSCmdlet
    {
        public const string CallName = "Add-FileSystemWatcherSettings";

        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public FileSystemWatcher FileWatcher { get; set; } = null!;

        [Parameter(Mandatory = false)]
        public ScriptBlock? OnChanged { get; set; } = null;

        [Parameter(Mandatory = false)]
        public ScriptBlock? OnDeleted { get; set; } = null;

        [Parameter(Mandatory = false)]
        public ScriptBlock? OnCreated { get; set; } = null;

        [Parameter(Mandatory = false)]
        public ScriptBlock? OnError { get; set; } = null;

        [Parameter(Mandatory = false)]
        public ScriptBlock? OnRenamed { get; set; } = null;

        [Parameter(Mandatory = false)]
        public string? Fillter { get; set; } = null;

        protected override void ProcessRecord()
        {
            if(OnChanged != null)
            {
                FileWatcher.Changed += (obj,param) =>
                {
                    OnChanged.Invoke(obj, param);
                };
            }
            if (OnCreated != null)
            {
                FileWatcher.Created += (obj, param) =>
                {
                    OnCreated.Invoke(obj, param);
                };
            }
            if (OnDeleted != null)
            {
                FileWatcher.Deleted += (obj, param) =>
                {
                    OnDeleted.Invoke(obj, param);
                };
            }
            if (OnError != null)
            {
                FileWatcher.Error += (obj, param) =>
                {
                    OnError.Invoke(obj, param);
                };
            }
            if (OnRenamed != null)
            {
                FileWatcher.Renamed += (obj, param) =>
                {
                    OnRenamed.Invoke(obj, param);
                };
            }
            if(Fillter != null)
            {
                FileWatcher.Filters.Add(Fillter);
            }
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
            FileWatcher.EndInit();
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
            FileWatcher.Dispose();
        }
    }
}
