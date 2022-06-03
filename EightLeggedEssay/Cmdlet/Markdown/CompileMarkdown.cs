//===---------------------------------------------------===//
//                  CompileMarkdown.cs
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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EightLeggedEssay.Cmdlet.Markdown
{
    /// <summary>
    /// 编译Markdown的Cmdlet
    /// </summary>
    [Cmdlet(VerbsData.Convert, "Markdown")]
    [OutputType(typeof(object[]))]
    public class CompileMarkdown : PSCmdlet
    {
        public const string CallName = "Convert-Markdown";

        /// <summary>
        /// 是否使用高级扩展
        /// </summary>
        [Parameter()]
        public SwitchParameter EnableAdvancedExpansion { get; set; } = new(false);

        /// <summary>
        /// 输入的markdown的字符串
        /// </summary>
        [Parameter(ValueFromPipeline = true, Position = 0, Mandatory = true)]
        public string? Source { get; set; } = null;

        protected override void ProcessRecord()
        {
            if (Source == null)
            {
                WriteError(new ErrorRecord(
                    new ArgumentNullException(nameof(Source)),
                    null,
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            var opt = new MarkdownCompiler.MarkdownRenderOptions();

            if (EnableAdvancedExpansion.IsPresent)
            {
                opt.EnableAdvancedExtend = true;
            }

            var output = MarkdownCompiler.Compile(Source, opt);

            WriteObject(output);
        }
    }

    /// <summary>
    /// 编译Markdown文章。和Compile-Markdown不同，文章包括头部信息，并且来自文件系统。
    /// 自带增量编译功能
    /// </summary>
    [Cmdlet(VerbsData.Convert, "PriMarkdownPoster")]
    [OutputType(typeof(MarkdownPoster))]
    public class CompileMarkdownPoster : PSCmdlet
    {
        public const string CallName = "Convert-PriMarkdownPoster";

        /// <summary>
        /// 要输入的markdown文章文本
        /// </summary>
        [Parameter(ValueFromPipeline = true, Position = 0, Mandatory = true)]
        public string? Source { get; set; } = null;

        protected override void ProcessRecord()
        {
            if (Source == null)
            {
                WriteError(new ErrorRecord(
                    new ArgumentNullException(nameof(Source)),
                    null,
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            var output = Markdown.Render(Source);

            WriteObject(output);
        }
    }

}
