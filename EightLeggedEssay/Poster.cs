//===---------------------------------------------------===//
//                  Poster.cs
//
// this file is under the MIT License
// See https://opensource.org/licenses/MIT for license information.
// Copyright(c) 2020-2022 moe-org All rights reserved.
//
//===---------------------------------------------------===//

using Scriban.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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

        public bool HasHtmlErrors { get; set; } = false;

        public PoasterHeader() { }

        public PoasterHeader(Poster poster)
        {
            this.Title = poster.Title;
            this.CreateTime = poster.CreateTime;
            this.Strict = poster.Strict;
            this.Attributes = poster.Attributes;
            this.SourcePath = poster.SourcePath;
            this.CompiledPath = poster.CompiledPath;
            this.HasHtmlErrors = poster.HasHtmlErrors;
        }
    }

    /// <summary>
    /// 这个类表示一篇文章，包括文章的其他信息。这篇文章通常已编译，这个类不能直接序列化或者反序列化。
    /// 线程不安全!
    /// 设置field操作会将文章保存。
    /// </summary>
    public class Poster
    {
        public Poster()
        {

        }

        public Poster(PoasterHeader header)
        {
            this._title = header.Title;
            this._strict = header.Strict;
            this._sourcePath = header.SourcePath;
            this._compiledPath = header.CompiledPath;
            this._hasHtmlError = header.HasHtmlErrors;
            this._createTime = header.CreateTime;
            this._attributes = header.Attributes;
        }

        private string _title = string.Empty;

        /// <summary>
        /// 文章的标题
        /// </summary>
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                Save();
            }
        }

        private DateTime _createTime = DateTime.Now;

        /// <summary>
        /// 文章创建日期
        /// </summary>
        public DateTime CreateTime
        {
            get
            {
                return _createTime;
            }
            set
            {
                _createTime = value;
                Save();
            }
        }

        private bool? _strict = null;

        /// <summary>
        /// 是否启动严格模式。如果启用则会对文章进行一些额外的检查，如果设置为null则遵循系统设置。
        /// </summary>
        public bool? Strict
        {
            get
            {
                return _strict;
            }
            set
            {
                _strict = value;
                Save();
            }
        }

        private Dictionary<string, JsonNode> _attributes = new();

        /// <summary>
        /// 文章的自定义属性，给用户自行使用。通常保存在文件中。
        /// </summary>
        public Dictionary<string, JsonNode> Attributes
        {
            get
            {
                return _attributes;
            }
            set
            {
                _attributes = value;
                Save();
            }
        }

        private bool _hasHtmlError = true;

        /// <summary>
        /// 文章是否有Html错误
        /// </summary>
        public bool HasHtmlErrors
        {
            get
            {
                return _hasHtmlError;
            }
            set
            {
                _hasHtmlError = value;
                Save();
            }
        }

        /// <summary>
        /// 文章的运行时数据，不会被增量编译保存，由运行时使用。一旦重启程序将修改将丢失。
        /// </summary>
        public Dictionary<object, object> ExtendedData { get; set; } = new();

        private string? _sourcePath = null;

        /// <summary>
        /// 文章的源路径。如果为null则说明文章并非来源文件系统。
        /// </summary>
        public string? SourcePath
        {
            get
            {
                return _sourcePath;
            }
            set
            {
                _sourcePath = value;
                Save();
            }
        }

        private string? _compiledPath = null;

        /// <summary>
        /// 文章的输出路径，注意，输出的是半成品，而非文章同模板渲染后的成品。如果为null则说明文章没有输出到文件系统，而是使用内存存储。
        /// </summary>
        public string? CompiledPath
        {
            get
            {
                return _compiledPath;
            }
            set
            {
                _compiledPath = value;
                Save();
            }
        }

        /// <summary>
        /// Text的弱引用版本。如果文章输出到文件系统则会启用这个字段，在引用失效的时候将会从文件系统读取。
        /// </summary>
        private readonly WeakReference<string?> _weakText = new(null);

        /// <summary>
        /// Text的强引用版本。这个字段用于不输出到文件系统的文章使用。
        /// 
        /// 对于不输出到文件系统的文章来说，引用一旦失效将无法再找回，所以使用强引用来储存。
        /// </summary>
        private string? _strongText = null;

        /// <summary>
        /// 文章经过编译后的数据。
        /// 如果文章储存在内存中则使用强引用来管理。否则使用弱引用并在引用失效时从文件系统读取。
        /// </summary>
        public string Text
        {
            get
            {
                if (_strongText != null)
                {
                    return _strongText;
                }
                else
                {
                    if (_weakText.TryGetTarget(out string? target))
                    {
                        return target;
                    }
                    else
                    {
                        // 重新加载
                        target = ParseText(File.ReadAllBytes(CompiledPath
                            ?? throw new InvalidOperationException("try to load memory poster")));

                        _weakText.SetTarget(target);

                        return target;
                    }
                }
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new();
            builder.AppendLine(string.Format("Poster Title:{0}", Title));
            builder.AppendLine(string.Format("Poster Create Time:{0}", CreateTime));
            builder.AppendLine(string.Format("Poster Strict:{0}", Strict));
            builder.AppendLine(string.Format("Poster Attributes:{0}", Attributes));
            builder.AppendLine(string.Format("Poster Source Path:{0}", SourcePath));
            builder.AppendLine(string.Format("Poster Compiled Path:{0}", CompiledPath));
            builder.AppendLine(string.Format("Poster Has Html Errors:{0}", HasHtmlErrors));
            builder.AppendLine(string.Format("Poster Text:{0}", Text));
            return builder.ToString();
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
                _title = title,
                _createTime = createTime,
                _strict = strict,
                _attributes = attributes,
                _sourcePath = sourcePath,
                _compiledPath = compiledPath
            };

            // 文章放在内存当中
            if (compiledPath == null)
            {
                poster._strongText = text;
            }
            // 文章放在文件系统当中
            else
            {
                // 缓存文本内容
                poster._weakText.SetTarget(text);

                // 写入文件系统
                poster.Save();
            }

            return poster;
        }

        /// <summary>
        /// 解析编译过的源文件的文本而不解析头部
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static string ParseText(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data);

            using var inputStream = new MemoryStream(data);

            // 跳过头部
            var longBuf = new byte[8];

            inputStream.Read(longBuf);

            var length = BitConverter.ToInt64(longBuf);

            inputStream.Seek(length, SeekOrigin.Current);

            // 读取文本
            string text = Encoding.UTF8.GetString(IOUtility.ReadContentWithLongLngeht(inputStream));

            return text;
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

            // 读取
            string head = Encoding.UTF8.GetString(IOUtility.ReadContentWithLongLngeht(inputStream));

            string text = Encoding.UTF8.GetString(IOUtility.ReadContentWithLongLngeht(inputStream));

            // 解析
            var h = JsonSerializer.Deserialize<PoasterHeader>(head) ?? throw new JsonException("empty json");

            var p = new Poster(h);

            p._weakText.SetTarget(text);

            return p;
        }

        /// <summary>
        /// 将当前文章信息写入CompiledFile。等价于保存当前文章信息。
        /// </summary>
        public void Save()
        {
            if (CompiledPath == null)
            {
                return;
            }
            PoasterHeader head = new(this);
            // 文件布局
            // [long:头部的长度][bytes:头部的json字符串utf8编码]
            // [long:文章的长度][bytes:文章的字符串utf8编码]

            var headJson = JsonSerializer.Serialize(head);

            using MemoryStream outputStream = new();

            var headJsonBytes = Encoding.UTF8.GetBytes(headJson);
            outputStream.Write(BitConverter.GetBytes(headJsonBytes.LongLength));
            outputStream.Write(headJsonBytes);

            var textBytes = Encoding.UTF8.GetBytes(Text);
            outputStream.Write(BitConverter.GetBytes(textBytes.LongLength));
            outputStream.Write(textBytes);

            // create parent directories by the way
            IOUtility.CreateParents(CompiledPath);
            File.WriteAllBytes(CompiledPath, outputStream.ToArray());
        }
    }
}
