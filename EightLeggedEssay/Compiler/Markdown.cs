//===---------------------------------------------------===//
//                  Markdown.cs
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
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Text.Json;
using Markdig;
using System.Runtime.InteropServices;
using static EightLeggedEssay.Compiler.Md4c;

namespace EightLeggedEssay.Compiler
{

    /// <summary>
    /// 一个特别的markdown转换器
    /// </summary>
    public static class Md4c
    {
        /// <summary>
        /// 这个注解代表md4c转换器支持的markdown转换选项。注意，选项可能和markdig的实现不完全一致。
        /// </summary>
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        public class Md4cSupported : Attribute
        {
        }

        /// <summary>
        /// md4c的解析选项
        /// </summary>
        public enum ParseOption
        {
            MD_FLAG_TABLES = 0x0100,
            MD_FLAG_TASKLISTS = 0x0800,
            MD_FLAG_LATEXMATHSPANS = 0x1000,
            MD_FLAG_STRIKETHROUGH = 0x0200,
        }

        private delegate void getText(IntPtr text, uint size);

        [DllImport("MD4CSharp", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int md4c_csharp_to_html(byte[] utf8, uint size, int parse_flags, int render_flags, getText callback);

        /// <summary>
        /// 0 stand for not inited.
        /// 1 stand for found.
        /// 2 stand for not found.
        /// </summary>
        private static volatile byte _md4c = 0;

        /// <summary>
        /// Md4c库是否可用状态
        /// </summary>
        public static bool HasMd4c
        {
            get
            {
                if (_md4c == 0)
                {
                    try
                    {
                        Marshal.PrelinkAll(typeof(Md4c));
                        _md4c = 1;
                    }
                    catch (DllNotFoundException)
                    {
                        _md4c = 2;
                    }
                }
                return _md4c == 1;
            }
        }

        /// <summary>
        /// 将markdown转换为html
        /// </summary>
        /// <param name="markdown">markdown字符串</param>
        /// <returns>html</returns>
        /// <exception cref="MarkdownParseException">markdown解析错误</exception>
        public static string Compile(string markdown,int parseFlags)
        {
            var utf8 = Encoding.UTF8.GetBytes(markdown);

            StringBuilder builder = new();

            var error = md4c_csharp_to_html(utf8, (uint)(utf8.Length), parseFlags, 0, (t, s) =>
            {
                Marshal.PtrToStringUTF8(t, (int)s);
            });

            if (error != 0)
            {
                throw new MarkdownParseException($"failed to convert markdown with code {error}(from md4c)");
            }

            return builder.ToString();
        }
    }

    /// <summary>
    /// markdown编译器
    /// </summary>
    public static class MarkdownCompiler
    {
        /// <summary>
        /// 渲染选项
        /// </summary>
        public class MarkdownRenderOptions
        {
            /// <summary>
            /// 是否开启markdig高级扩展（本质上为自动启用一些扩展，具体列表见markdig README）
            /// </summary>
            public bool EnableAdvancedExtend { get; set; } = false;

            /// <summary>
            /// https://github.com/xoofx/markdig/blob/master/src/Markdig.Tests/Specs/TaskListSpecs.md
            /// or 
            /// MD_FLAG_TASKLISTS
            /// </summary>
            [Md4cSupported]
            public bool TaskLists { get; set; } = false;

            /// <summary>
            /// https://github.com/xoofx/markdig/blob/master/src/Markdig.Tests/Specs/EmphasisExtraSpecs.md
            /// or
            /// MD_FLAG_STRIKETHROUGH
            /// </summary>
            [Md4cSupported]
            public bool EmphasisExtra { get; set; } = false;

            /// <summary>
            /// https://github.com/xoofx/markdig/blob/master/src/Markdig.Tests/Specs/EmojiSpecs.md
            /// </summary>
            public bool EmojiSupport { get; set; } = false;

            /// <summary>
            /// https://github.com/xoofx/markdig/blob/master/src/Markdig.Tests/Specs/PipeTableSpecs.md
            /// or
            /// MD_FLAG_TABLES
            /// </summary>
            [Md4cSupported]
            public bool PipeTable { get; set; } = false;

            /// <summary>
            /// https://github.com/xoofx/markdig/blob/master/src/Markdig.Tests/Specs/GridTableSpecs.md
            /// </summary>
            public bool GridTable { get; set; } = false;

            /// <summary>
            /// https://github.com/xoofx/markdig/blob/master/src/Markdig.Tests/Specs/HardlineBreakSpecs.md
            /// </summary>
            public bool HardLineBreak { get; set; } = false;

            /// <summary>
            /// https://github.com/xoofx/markdig/blob/master/src/Markdig.Tests/Specs/MathSpecs.md
            /// or
            /// MD_FLAG_LATEXMATHSPANS
            /// </summary>
            [Md4cSupported]
            public bool LatexInline { get; set; } = false;


            /// <summary>
            /// 尝试使用Md4C作为markdown转换器。这将忽略其他选项。
            /// 如果md4c的动态库未找到，则继续使用其他选项和Markdig进行编译。
            /// </summary>
            public bool TryToUseMd4c { get; set; } = false;
        }

        /// <summary>
        /// 编译
        /// </summary>
        public static string Compile(string markdown, MarkdownRenderOptions options)
        {
            if (options.TryToUseMd4c && Md4c.HasMd4c)
            {
                int parseFlags = 0;
                if (options.TaskLists)
                {
                    parseFlags |= (int)ParseOption.MD_FLAG_TASKLISTS;
                }
                if (options.EmphasisExtra)
                {
                    parseFlags |= (int)ParseOption.MD_FLAG_STRIKETHROUGH;
                }
                if (options.PipeTable)
                {
                    parseFlags |= (int)ParseOption.MD_FLAG_TABLES;
                }
                if (options.LatexInline)
                {
                    parseFlags |= (int)ParseOption.MD_FLAG_LATEXMATHSPANS;
                }

                return Md4c.Compile(markdown, parseFlags);
            }

            var pipelineBuilder = new MarkdownPipelineBuilder();

            if (options.EnableAdvancedExtend)
            {
                pipelineBuilder.UseAdvancedExtensions();
            }
            if (options.TaskLists)
            {
                pipelineBuilder.UseTaskLists();
            }
            if (options.HardLineBreak)
            {
                pipelineBuilder.UseSoftlineBreakAsHardlineBreak();
            }
            if (options.EmphasisExtra)
            {
                pipelineBuilder.UseEmphasisExtras();
            }
            if (options.LatexInline)
            {
                pipelineBuilder.UseMathematics();
            }
            if (options.EmojiSupport)
            {
                pipelineBuilder.UseEmojiAndSmiley();
            }
            if (options.PipeTable)
            {
                pipelineBuilder.UsePipeTables();
            }
            if (options.GridTable)
            {
                pipelineBuilder.UseGridTables();
            }

            var pipeline = pipelineBuilder.Build();

            return Markdig.Markdown.ToHtml(markdown, pipeline);
        }
    }

    /// <summary>
    /// Markdown头部信息，支持json序列化
    /// </summary>
    public class MarkdownHead
    {
        /// <summary>
        /// 文章创建时间
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 文章的标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 文章的属性
        /// </summary>
        public Dictionary<string, JsonNode> Attributes { get; set; } = new();

        /// <summary>
        /// 严格模式设置
        /// </summary>
        public bool? Strict { get; set; } = null;
    }

    /// <summary>
    /// markdown解析异常
    /// </summary>
    public class MarkdownParseException : Exception
    {
        /// <summary>
        /// 构造一个markdown解析异常
        /// </summary>
        /// <param name="msg"></param>
        public MarkdownParseException(string msg) : base(msg) { }

        /// <summary>
        /// 构造一个markdown解析异常
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="inner"></param>
        public MarkdownParseException(string msg, Exception inner) : base(msg, inner) { }
    }

    /// <summary>
    /// 编译过后的markdown文章
    /// </summary>
    public class MarkdownPoster
    {
        public MarkdownHead Head { get; set; }

        /// <summary>
        /// not html!
        /// </summary>
        public string Markdown { get; set; }

        public MarkdownPoster(MarkdownHead head, string markdown)
        {
            Head = head;
            Markdown = markdown;
        }
    }

    /// <summary>
    /// 这个类负责和编译Markdown有关的事情
    /// </summary>
    public static class Markdown
    {
        /// <summary>
        /// 用于标识文章信息开头
        /// </summary>
        public const string PosterInfoHeader = "<!--INFOS--";

        /// <summary>
        /// 用于表示文章信息结尾
        /// </summary>
        public const string PosterInfoEnd = "--INFOS-->";

        /// <summary>
        /// 检查markdown头部
        /// </summary>
        private static void CheckHead(ref MarkdownHead head)
        {
            if (string.IsNullOrEmpty(head.Title))
            {
                throw new MarkdownParseException("head with no title");
            }
        }

        /// <summary>
        /// 获取文章头部
        /// </summary>
        /// <param name="inputString">文章字符串，需要包括文章头部。</param>
        /// <param name="head">获取的文章的头部</param>
        /// <param name="options">文章渲染选项</param>
        /// <returns>待渲染的markdown</returns>
        public static MarkdownPoster ParseMarkdownPoster(string inputString)
        {
            // 扫描头部
            if (inputString[..PosterInfoHeader.Length] != PosterInfoHeader)
            {
                throw new MarkdownParseException($"poster not start with {PosterInfoHeader}");
            }

            inputString = inputString.Remove(0, PosterInfoHeader.Length);

            // 扫描尾部
            // 添加一个换行符来确保尾部处于一个新的行
            int index = inputString.IndexOf("\n" + PosterInfoEnd);

            // 截取json
            if (index >= 0)
            {
                string json = inputString[..index];
                var markdown = inputString[(index + PosterInfoEnd.Length)..];

                // 序列化
                var h = JsonSerializer.Deserialize<MarkdownHead>(json) ??
                    throw new MarkdownParseException("failed to parse head",
                    new JsonException("empty json"));

                CheckHead(ref h);

                return new MarkdownPoster(h, markdown);
            }

            // 没有尾部，报错
            throw new MarkdownParseException($"poster has no {PosterInfoEnd} in new line with \\n");
        }
    }
}
