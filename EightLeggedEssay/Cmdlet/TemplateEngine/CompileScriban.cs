//===---------------------------------------------------===//
//                  CompileScriban.cs
//
// this file is under the MIT License
// See https://opensource.org/licenses/MIT for license information.
// Copyright(c) 2020-2022 moe-org All rights reserved.
//
//===---------------------------------------------------===//

using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace EightLeggedEssay.Cmdlet.TemplateEngine
{
    /// <summary>
    /// Scriban的编译器
    /// </summary>
    public static class ScribanCompiler
    {
        /// <summary>
        /// 用于Scriban include使用的类
        /// </summary>
        private class ScribanIncluder : ITemplateLoader
        {
            /// <summary>
            /// Include搜索文件的路径
            /// </summary>
            public string IncludePath { get; set; } = Environment.CurrentDirectory;

            public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
            {
                return Path.GetFullPath(Path.Join(IncludePath, templateName));
            }
            public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
            {
                return File.ReadAllText(templatePath);
            }
            public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
            {
                return new ValueTask<string>(File.ReadAllTextAsync(templatePath));
            }
        }

        private static readonly ThreadLocal<ScribanIncluder> Includer = new();
        private static readonly ThreadLocal<TemplateContext> Context = new();
        private static readonly ThreadLocal<ScriptObject> EmptyObject = new();

        /// <summary>
        /// 渲染
        /// </summary>
        /// <param name="includePath">include文件的路径，如果为null则设置为当前工作目录</param>
        /// <param name="logs">如果发生错误，则将此值设为非null</param>
        /// <returns>渲染结果，如果发现错误则返回string.Empty</returns>
        public static string Render(
            string template,
            string? includePath,
            ScriptObject? variable,
            out LogMessageBag? logs)
        {
            // 初始化值
            if (Includer.Value == null)
            {
                Includer.Value = new();
            }
            var includer = Includer.Value;
            includer.IncludePath = includePath ?? Environment.CurrentDirectory;


            if (Context.Value == null)
            {
                Context.Value = new();
            }
            var context = Context.Value;


            // 清除缓存
            context.CachedTemplates.Clear();

            // 渲染
            if (variable != null)
            {
                context.PushGlobal(variable);
            }
            try
            {
                var parsed = Template.Parse(template);

                if (parsed.HasErrors)
                {
                    logs = parsed.Messages;
                    return string.Empty;
                }
                else
                {
                    logs = null;
                }

                return parsed.Render(context);
            }
            finally
            {
                // 控制变量作用域
                if (variable != null)
                {
                    context.PopGlobal();
                }
            }
        }

    }

    /// <summary>
    /// 用于渲染Scriban模板引擎的Cmdlet
    /// </summary>
    [Cmdlet(VerbsData.Convert, "Scriban")]
    [OutputType(typeof(string))]
    public class CompileScriban : PSCmdlet
    {
        public const string CallName = "Convert-Scriban";

        /// <summary>
        /// 模板源字符串
        /// </summary>
        [Parameter(ValueFromPipeline = true, Mandatory = true, Position = 0)]
        public PSObject? Source { get; set; } = null;

        /// <summary>
        /// 用于编译文章的属性。可选
        /// </summary>
        [Parameter(ValueFromPipeline = true, Mandatory = false, Position = 1)]
        public PSObject? Property { get; set; } = null;

        /// <summary>
        /// include路径。默认为当前工作目录
        /// </summary>
        [Parameter(ValueFromPipeline = true, Mandatory = false, Position = 2)]
        public PSObject? IncludePath { get; set; } = null;

        protected override void ProcessRecord()
        {
            // 检查类型
            if (Source == null || Source.BaseObject.GetType() != typeof(string))
            {
                WriteError(new ErrorRecord(
                    new ArgumentException(
                        $"Source is null or isn't string. {Source?.BaseObject.GetType()}", nameof(Source)),
                    string.Empty,
                    ErrorCategory.InvalidArgument, Source));
                return;
            }
            if (Property != null && Property.BaseObject.GetType() != typeof(ScriptObject))
            {
                WriteError(new ErrorRecord(
                    new ArgumentException(
                        $"Property isn't ScriptObject. {Property.BaseObject.GetType()}", nameof(Property)),
                    string.Empty,
                    ErrorCategory.InvalidArgument, Property));
                return;
            }

            // 准备参数
            var source = (string)Source.BaseObject;

            ScriptObject? attr = null;

            if (Property?.BaseObject != null)
            {
                attr = (ScriptObject)Property.BaseObject;
            }

            string? includePath = null;

            if (IncludePath?.BaseObject != null)
            {
                includePath = IncludePath.BaseObject.ToString() ?? includePath;
            }

            // 渲染
            var result = ScribanCompiler.Render(source, includePath, attr, out LogMessageBag? errors);

            if (errors != null)
            {
                foreach (var err in errors)
                {
                    WriteError(
                        new ErrorRecord(
                            new Scriban.Syntax.ScriptRuntimeException(err.Span, err.Message),
                            null,
                            ErrorCategory.InvalidData,
                            source));
                }
                return;
            }

            WriteObject(result);
        }
    }
}
