//===---------------------------------------------------===//
//                  GetVariable.cs
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

namespace EightLeggedEssay.Cmdlet
{
    /// <summary>
    /// 系统变量条目
    /// </summary>
    public enum SystemVariableEnums
    {
        /// <summary>
        /// 是否处在debug模式
        /// </summary>
        IsDebugMode,
        /// <summary>
        /// http服务器地址
        /// </summary>
        ServerPath,
        /// <summary>
        /// 配置文件的文本
        /// </summary>
        Configuration,
    }

    /// <summary>
    /// 获取系统变量
    /// </summary>
    [Cmdlet(VerbsCommon.Get,"EleVariable")]
    [OutputType(typeof(object))]
    public class GetVariable : PSCmdlet
    {
        /// <summary>
        /// 调用名称
        /// </summary>
        public const string CallName = "Get-EleVariable";


        /// <summary>
        /// 要获取的变量的名称
        /// </summary>
        [Parameter(ValueFromPipeline = true,Position = 0,Mandatory = true)]
        public string Name { get; set; } = "";

        protected override void ProcessRecord()
        {
            if (Enum.TryParse(Name,out SystemVariableEnums variable))
            {
                switch (variable)
                {
                    case SystemVariableEnums.ServerPath:
                        WriteObject(Program.ServerPath);
                        return;
                    case SystemVariableEnums.IsDebugMode:
                        WriteObject(Program.DebugMode);
                        return;
                    case SystemVariableEnums.Configuration:
                        WriteObject(Configuration.GlobalConfigurationText);
                        return;
                }
            }

            WriteError(new ErrorRecord(
                new ArgumentException("unknown system variable name"),
                string.Empty,
                ErrorCategory.InvalidArgument,
                Name));
            WriteObject(null);
        }

    }
}
