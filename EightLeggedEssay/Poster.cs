﻿//===---------------------------------------------------===//
//                  Poster.cs
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
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace EightLeggedEssay
{
    /// <summary>
    /// 用于存储文章的头部
    /// </summary>
    public class PoasterHeader
    {
        public string Title { get; set; } = string.Empty;

        public DateTime CreateTime { get; set; } = DateTime.Now;

        public bool? Strict { get; set; } = null;

        public Dictionary<string, JsonNode> Attributes { get; set; } = new();

        public string? SourcePath { get; set; } = null;

        public string? CompiledPath { get; set; } = null;
    }

    /// <summary>
    /// 这个类表示一篇文章，包括文章的其他信息。这篇文章通常已编译，这个类不能直接序列化或者反序列化。
    /// </summary>
    public class Poster
    {
        /// <summary>
        /// 文章的标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 文章创建日期
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 是否启动严格模式。如果启用则会对文章进行一些额外的检查，如果设置为null则遵循系统设置。
        /// </summary>
        public bool? Strict { get; set; } = null;

        /// <summary>
        /// 文章的自定义属性，给用户自行使用
        /// </summary>
        public Dictionary<string, JsonNode> Attributes { get; set; } = new();

        /// <summary>
        /// 文章的源路径。如果为null则说明文章并非来源文件系统。
        /// </summary>
        public string? SourcePath { get; set; } = null;

        /// <summary>
        /// 文章的输出路径，注意，输出的是半成品，而非文章同模板渲染后的成品。如果为null则说明文章没有输出到文件系统，而是使用内存存储。
        /// </summary>
        public string? CompiledPath { get; set; } = null;

        /// <summary>
        /// Text的弱引用版本。如果文章输出到文件系统则会启用这个字段，在引用失效的时候将会从文件系统读取。
        /// </summary>
        private WeakReference<string>? WeakText = null!;

        /// <summary>
        /// Text的强引用版本。这个字段用于不输出到文件系统的文章使用。
        /// 
        /// 对于不输出到文件系统的文章来说，引用一旦失效将无法再找回，所以使用强引用来储存。
        /// </summary>
        private string? StrongText = null;

        /// <summary>
        /// 文章经过编译后的数据。
        /// 如果文章储存在内存中则使用强引用来管理。否则使用弱引用并在引用失效时从文件系统读取。
        /// </summary>
        public string Text
        {
            get
            {
                if (StrongText != null)
                {
                    return StrongText;
                }
                else
                {
                    if (WeakText != null && WeakText.TryGetTarget(out string? target))
                    {
                        return target;
                    }
                    else
                    {
                        // 重新加载
                        target = File.ReadAllText(CompiledPath
                            ?? throw new InvalidOperationException("try to load memory poster"));

                        if (WeakText != null)
                        {
                            WeakText?.SetTarget(target);
                        }
                        else
                        {
                            WeakText = new(target);
                        }

                        return target;
                    }
                }
            }
        }

        /// <summary>
        /// 创建一篇经过编译文章，并用特殊的格式输出到文件系统。
        /// 
        /// 要读取这种特殊格式，使用Poster.Parse(bytes[]);
        /// </summary>
        /// <param name="text">经过编译的文章内容</param>
        /// <param name="title">文章标题</param>
        /// <param name="utcCreateTime">文章创建时间</param>
        /// <param name="strict">严格模式</param>
        /// <param name="attributes">文章属性</param>
        /// <param name="sourcePath">文章源路径</param>
        /// <param name="compiledPath">
        /// 要输出的文件系统的路径。
        /// 如果为null，则文章储存在内存中，而非文件系统中。
        /// </param>
        /// <returns></returns>
        public static Poster Create(
            string text,
            string title,
            DateTime createTime,
            bool? strict,
            Dictionary<string, JsonNode> attributes,
            string? sourcePath,
            string? compiledPath)
        {
            ArgumentNullException.ThrowIfNull(text);
            ArgumentNullException.ThrowIfNull(title);
            ArgumentNullException.ThrowIfNull(createTime);
            ArgumentNullException.ThrowIfNull(attributes);

            Poster poster = new()
            {
                Title = title,
                CreateTime = createTime,
                Strict = strict,
                Attributes = attributes,
                SourcePath = sourcePath,
                CompiledPath = compiledPath
            };

            // 文章放在内存当中
            if (compiledPath == null)
            {
                poster.StrongText = text;
            }
            // 文章放在文件系统当中
            else
            {
                poster.WeakText = new(text);

                // 写入文件系统
                PoasterHeader head = new()
                {
                    Title = poster.Title,
                    CreateTime = poster.CreateTime,
                    Strict = poster.Strict,
                    Attributes = poster.Attributes,
                    SourcePath = poster.SourcePath,
                    CompiledPath = poster.CompiledPath
                };
                // 文件布局
                // [long:头部的长度][bytes:头部的json字符串utf8编码]
                // [long:文章的长度][bytes:文章的字符串utf8编码]

                var headJson = JsonSerializer.Serialize(head);

                using MemoryStream outputStream = new();

                var headJsonBytes = Encoding.UTF8.GetBytes(headJson);
                outputStream.Write(BitConverter.GetBytes(headJsonBytes.LongLength));
                outputStream.Write(headJsonBytes);

                var textBytes = Encoding.UTF8.GetBytes(text);
                outputStream.Write(BitConverter.GetBytes(textBytes.LongLength));
                outputStream.Write(textBytes);

                File.WriteAllBytes(compiledPath, outputStream.ToArray());
            }

            return poster;
        }

        /// <summary>
        /// 从二进制数据解析一篇文章。二进制数据应该由Create()函数输出到文件系统当中。
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public static Poster Parse(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data);

            using var inputStream = new MemoryStream(data);

            var longBuf = new byte[8];

            // 读取头部
            inputStream.Read(longBuf);

            var length = BitConverter.ToInt64(longBuf);

            var buf = new byte[length];

            inputStream.Read(buf);

            string head = Encoding.UTF8.GetString(buf);
            // 读取文本
            inputStream.Read(longBuf);

            length = BitConverter.ToInt64(longBuf);

            buf = new byte[length];

            inputStream.Read(buf);

            string text = Encoding.UTF8.GetString(buf);
            // 解析
            var h = JsonSerializer.Deserialize<PoasterHeader>(text) ?? throw new JsonException("empty json");

            var p = new Poster()
            {
                Title = h.Title,
                CreateTime = h.CreateTime,
                Attributes = h.Attributes,
                Strict = h.Strict,
                SourcePath = h.SourcePath,
                CompiledPath = h.CompiledPath,
            };

            p.WeakText.SetTarget(text);

            return p;
        }
    }
}
