//===---------------------------------------------------===//
//                  Sitemap.cs
//
// this file is under the MIT License
// See https://opensource.org/licenses/MIT for license information.
// Copyright(c) 2020-2022 moe-org All rights reserved.
//
//===---------------------------------------------------===//

using System.Text;
using System.Xml;

namespace EightLeggedEssay
{
    /// <summary>
    /// Sitemap 0.9的的SitemapIndex实现
    /// </summary>
    public class SitemapMapIndex
    {
        /// <summary>
        /// sitemap文件的索引
        /// </summary>
        public class Index
        {
            public string Loc { get; set; }
            public DateTime? LastMod { get; set; } = null;

            public Index(string loc)
            {
                ArgumentNullException.ThrowIfNull(loc);
                Loc = loc;
            }
        }

        public List<Index> Indexs { get; } = new();

        /// <summary>
        /// 写sitemap作xml字符串
        /// </summary>
        /// <returns>xml字符串</returns>
        public string Write()
        {
            using MemoryStream stream = new();

            XmlWriterSettings settings = new()
            {
                NewLineChars = "\n",
                ConformanceLevel = ConformanceLevel.Document,
                Indent = true
            };

            using var writer = XmlWriter.Create(stream, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("urlset");
            writer.WriteAttributeString("sitemapindex", "http://www.sitemaps.org/schemas/sitemap/0.9");
            {
                foreach(var index in Indexs)
                {
                    writer.WriteStartElement("sitemap");

                    writer.WriteElementString("loc", index.Loc);

                    if(index.LastMod is not null)
                    {
                        writer.WriteElementString("lastmod", 
                            index.LastMod.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz"));
                    }

                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();

            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }

    /// <summary>
    /// Sitemap 0.9的Sitemap实现
    /// </summary>
    public class Sitemap
    {
        /// <summary>
        /// 修改频率
        /// </summary>
        public enum ChangeFreq
        {
            always,
            hourly,
            daily,
            weekly,
            monthly,
            yearly,
            never
        }

        /// <summary>
        /// url of sitemap under the urlset
        /// </summary>
        public class Url
        {
            public string Loc { get; set; }

            public DateTime? LastMod { get; set; } = null;

            public ChangeFreq? ChangeFreq { get; set; } = null;

            private decimal? _priority = null;

            /// <summary>
            /// 值需要在1到0之间。
            /// 只保留一位小数，多余的位数将被忽略
            /// </summary>
            public decimal? Priority
            {
                get
                {
                    return _priority;
                }
                set
                {
                    if(value != null && (value > 1 || value < 0))
                    {
                        throw new InvalidDataException("Priority of sitemap should less than 1 and more than 0");
                    }
                    _priority = value;
                }
            }

            public Url(string loc)
            {
                ArgumentNullException.ThrowIfNull(loc);
                Loc = loc;
            }
        }

        /// <summary>
        /// url集
        /// </summary>
        public List<Url> Urls { get; } = new();

        /// <summary>
        /// 写sitemap作xml字符串。注意，这不会对sitemap大小进行检查
        /// </summary>
        /// <returns>xml字符串</returns>
        public string Write()
        {
            using MemoryStream stream = new();

            XmlWriterSettings settings = new()
            {
                NewLineChars = "\n",
                ConformanceLevel = ConformanceLevel.Document,
                Indent = true
            };

            using var writer = XmlWriter.Create(stream, settings);

            writer.WriteStartElement("urlset");
            writer.WriteAttributeString("xmlns","http://www.sitemaps.org/schemas/sitemap/0.9");

            {
                foreach(var url in Urls)
                {
                    writer.WriteStartElement("url");

                    writer.WriteElementString("loc",url.Loc);

                    if(url.ChangeFreq is not null)
                    {
                        writer.WriteElementString("changefreq", url.ChangeFreq.ToString());
                    }
                    if(url.LastMod is not null)
                    {
                        writer.WriteElementString("lastmod", url.LastMod.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz"));
                    }
                    if(url.Priority is not null)
                    {
                        writer.WriteElementString("priority",url.Priority.Value.ToString("0.0"));
                    }

                    writer.WriteEndElement();
                }
            }

            writer.WriteEndElement();
            writer.Close();

            return Encoding.UTF8.GetString(stream.ToArray());
        }

    }
}
