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
        ContentDir,
        ThemeDir,
        OutputDir,
        SourceDir,
        RootUrl,
        BuildScript,
    }

    /// <summary>
    /// 获取系统变量
    /// </summary>
    [Cmdlet(VerbsCommon.Get,"EleVariable")]
    [OutputType(typeof(string))]
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
                    case SystemVariableEnums.ContentDir:
                        WriteObject(Configuration.GlobalConfiguration.ContentDirectory);
                        return;
                    case SystemVariableEnums.OutputDir:
                        WriteObject(Configuration.GlobalConfiguration.OutputDirectory);
                        return;
                    case SystemVariableEnums.SourceDir:
                        WriteObject(Configuration.GlobalConfiguration.SourceDirectory);
                        return;
                    case SystemVariableEnums.ThemeDir:
                        WriteObject(Configuration.GlobalConfiguration.ThemeDirectory);
                        return;
                    case SystemVariableEnums.RootUrl:
                        WriteObject(Configuration.GlobalConfiguration.RootUrl);
                        return;
                    case SystemVariableEnums.BuildScript:
                        WriteObject(Configuration.GlobalConfiguration.BuildScript);
                        return;
                }
            }
            
            WriteError(new ErrorRecord(new ArgumentException("unknown system variable name"),String.Empty,ErrorCategory.InvalidArgument, null));
            WriteObject(null);
        }

    }
}
