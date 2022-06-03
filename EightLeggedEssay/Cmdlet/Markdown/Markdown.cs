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

namespace EightLeggedEssay.Cmdlet.Markdown
{

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
            /// 是否开启高级扩展
            /// </summary>
            public bool EnableAdvancedExtend { get; set; } = false;
        }

        /// <summary>
        /// 编译
        /// </summary>
        public static string Compile(string markdown, MarkdownRenderOptions options)
        {
            var pipelineBuilder = new MarkdownPipelineBuilder();

            if (options.EnableAdvancedExtend)
            {
                pipelineBuilder.UseAdvancedExtensions();
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
    public class MarkdownParseException : System.Exception
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
        public static MarkdownPoster Render(string inputString)
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
