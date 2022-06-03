//===---------------------------------------------------===//
//                  ThreadWorker.cs
//
// this file is under the MIT License
// See https://opensource.org/licenses/MIT for license information.
// Copyright(c) 2020-2022 moe-org All rights reserved.
//
//===---------------------------------------------------===//

using EightLeggedEssay.ThreadWorker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;

namespace EightLeggedEssay.Cmdlet
{

    /// <summary>
    /// 新建一个任务管理器
    /// </summary>
    [Cmdlet(VerbsCommon.New, "ThreadJobManager")]
    [OutputType(typeof(WorkerManager))]
    public class NewThreadJobManager : PSCmdlet
    {
        public const string CallName = "New-ThreadJobManager";

        /// <summary>
        /// 线程数量，如果设置为0则使用操作系统的所有处理器
        /// </summary>
        [Parameter(ValueFromPipeline = false, Mandatory = true, Position = 1)]
        public long Count { get; set; } = 0;

        /// <summary>
        /// 任务管理器名称，如果为null则设置为default
        /// </summary>
        [Parameter(ValueFromPipeline = true, Mandatory = false, Position = 2)]
        public string? Name { get; set; } = "default";


        protected override void ProcessRecord()
        {
            if (Count == 0)
            {
                Count = Environment.ProcessorCount;
            }

            Name ??= "defailt";

            WriteObject(new WorkerManager(Count, Name));
        }
    }


    /// <summary>
    /// 添加多线程任务
    /// </summary>
    [Cmdlet(VerbsLifecycle.Start, "PriThreadJob")]
    [OutputType(typeof(void))]
    public class StartThreadJob : PSCmdlet
    {
        public const string CallName = "Start-PriThreadJob";

        /// <summary>
        /// 线程管理器
        /// </summary>
        [Parameter(ValueFromPipeline = true, Mandatory = true, Position = 1)]
        public WorkerManager? Manager { get; set; } = null;

        /// <summary>
        /// 脚本块
        /// </summary>
        [Parameter(ValueFromPipeline = true, Mandatory = true, Position = 2)]
        public ScriptBlock? ScriptBlock { get; set; } = null;

        /// <summary>
        /// 调用堆栈
        /// </summary>
        [Parameter(ValueFromPipeline = false, Mandatory = true, Position = 3)]
        public string? CallStack { get; set; } = string.Empty;

        /// <summary>
        /// 要传递的对象
        /// </summary>
        [Parameter(ValueFromPipeline = false, Mandatory = false, Position = 4)]
        public PSObject? PassedVariable { get; set; } = null;

        protected override void ProcessRecord()
        {
            if (ScriptBlock == null)
            {
                WriteError(new ErrorRecord(
                    new ArgumentNullException(nameof(ScriptBlock)),
                    string.Empty,
                    ErrorCategory.InvalidArgument, null));
                return;
            }
            if (Manager == null)
            {
                WriteError(new ErrorRecord(
                    new ArgumentNullException(nameof(Manager)),
                    string.Empty,
                    ErrorCategory.InvalidArgument, null));
                return;
            }

            var script = ScriptBlock;

            var location = CallStack ?? string.Empty;

            Manager.Start(
                () =>
                {
                    var engine = ScriptEngine.GetEngine(Thread.CurrentThread.Name!, location);

                    engine.Open();

                    engine.Shell.Runspace.SessionStateProxy.SetVariable("PassedVariable", PassedVariable?.BaseObject);

                    var result = engine.Shell.AddScript(script.ToString()).Invoke();

                    engine.Close();

                    if (result.Count > 1)
                    {
                        return result.ToArray();
                    }
                    else if (result.Count == 0)
                    {
                        return null;
                    }
                    else
                    {
                        return result[0];
                    }
                });
        }
    }

    /// <summary>
    /// 等待线程工作完成。同时获取所有执行结果
    /// </summary>
    [Cmdlet(VerbsLifecycle.Wait, "ThreadJob")]
    [OutputType(typeof(object[]))]
    public class WaitThreadJob : PSCmdlet
    {
        public const string CallName = "Wait-ThreadJob";

        /// <summary>
        /// 线程管理器
        /// </summary>
        [Parameter(ValueFromPipeline = true, Mandatory = true, Position = 1)]
        public WorkerManager? Manager { get; set; } = null;

        protected override void ProcessRecord()
        {
            if (Manager == null)
            {
                WriteError(new ErrorRecord(
                    new ArgumentNullException(nameof(Manager)),
                    string.Empty,
                    ErrorCategory.InvalidArgument, null));
                return;
            }

            Manager.Wait();

            while (Manager.Results.TryTake(out object? result))
            {
                WriteObject(result);
            }
            while (Manager.Errors.TryTake(out ThreadJobException? err))
            {
                WriteError(new
                    ErrorRecord(
                    err,
                    null,
                    ErrorCategory.InvalidOperation,
                    null));
            }
        }
    }
}
