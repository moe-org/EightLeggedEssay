//===---------------------------------------------------===//
//                  Rss.cs
//
// this file is under the MIT License
// See https://opensource.org/licenses/MIT for license information.
// Copyright(c) 2020-2022 moe-org All rights reserved.
//
//===---------------------------------------------------===//

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EightLeggedEssay
{
    /// <summary>
    /// 代表每一篇文章
    /// </summary>
    public class RssItem
    {
        public string? Title { get; set; } = null;
        public string? Description { get; set; } = null;
        public string? Link { get; set; } = null;

        public string? Author { get; set; } = null;

        public DateTime? PublishTime { get; set; } = null;

        public (string, string?)? Category { get; set; } = null;

        /// <summary>
        /// 写入xml
        /// </summary>
        /// <param name="writer">xml写入器</param>
        public void Write(XmlWriter writer)
        {
            writer.WriteStartElement("item");
            {
                if (Title is not null)
                {
                    writer.WriteElementString("title", Title);
                }
                if (Description is not null)
                {
                    writer.WriteElementString("description", Description);
                }
                if (Link is not null)
                {
                    writer.WriteElementString("link", Link);
                }
                if (Author is not null)
                {
                    writer.WriteElementString("author", Author);
                }
                if (PublishTime is not null)
                {
                    writer.WriteElementString("pubDate", PublishTime.Value.ToString("R"));
                }
                if (Category is not null)
                {
                    writer.WriteStartElement("category");
                    if (Category.Value.Item2 is not null)
                    {
                        writer.WriteAttributeString("domain", Category.Value.Item2);
                    }
                    writer.WriteString(Category.Value.Item1);
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }
    }

    /// <summary>
    /// Rss类。注意：我们不会使用SyndicationFeed。nullable的是可选选项
    /// </summary>
    public class Rss
    {
        public string Title { get; set; }
        public Uri Link { get; set; }
        public string Description { get; set; }
        public string? Language { get; set; } = null;
        public string? CopyRight { get; set; } = null;
        public DateTime? PublishTime { get; set; } = null;
        public DateTime? LastBuildDate { get; set; } = null;
        public string? Generator { get; set; } = "EightLeggedEssay";
        public int? Ttl { get; set; } = null;
        public (string,string?)? Category { get; set; } = null;
        public string? WebMaster { get; set; } = null;
        public string? ManagingEditor { get; set; } = null;
        public List<RssItem> Items { get; init; } = new();

        /// <summary>
        /// 构造一个Rss文档
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="url">网站地址</param>
        /// <param name="description">描述</param>
        public Rss(string title,Uri link,string description)
        {
            ArgumentNullException.ThrowIfNull(title);
            ArgumentNullException.ThrowIfNull(link);
            ArgumentNullException.ThrowIfNull(description);

            Title = title;
            Link = link;
            Description = description;
        }

        /// <summary>
        /// 写入RSS 2.0
        /// </summary>
        /// <returns>xml字符串</returns>
        public string GetString()
        {
            using MemoryStream stream = new();

            XmlWriterSettings settings = new()
            {
                NewLineChars = "\n",
                ConformanceLevel = ConformanceLevel.Fragment,
                Indent = true
            };

            using var write = XmlWriter.Create(stream,settings);

            // base
            write.WriteStartElement("rss");
            write.WriteAttributeString("version","2.0");
            write.WriteStartElement("channel");

            // 开始写入内容
            {
                // 写入一些必选项
                // call ToString instead of checking null
                write.WriteElementString("title", Title.ToString());
                write.WriteElementString("link", Link.ToString());
                write.WriteElementString("description", Description.ToString());

                if(Language is not null)
                {
                    write.WriteElementString("language", Language);
                }
                if(CopyRight is not null)
                {
                    write.WriteElementString("copyright", CopyRight);
                }
                if (PublishTime is not null)
                {
                    write.WriteElementString("pubDate", PublishTime.Value.ToString("R"));
                }
                if (LastBuildDate is not null)
                {
                    write.WriteElementString("lastBuildDate", LastBuildDate.Value.ToString("R"));
                }
                if(Generator is not null)
                {
                    write.WriteElementString("generator", Generator);
                }
                if(Ttl is not null){
                    write.WriteElementString("ttl", Ttl.ToString());
                }
                if (Category is not null)
                {
                    write.WriteStartElement("category");
                    if (Category.Value.Item2 is not null)
                    {
                        write.WriteAttributeString("domain", Category.Value.Item2);
                    }
                    write.WriteString(Category.Value.Item1);
                    write.WriteEndElement();
                }
                if (ManagingEditor is not null)
                {
                    write.WriteElementString("managingEditor", ManagingEditor);
                }
                if (WebMaster is not null)
                {
                    write.WriteElementString("webMaster", WebMaster);
                }

                // 写入items
                foreach(var item in Items)
                {
                    item.Write(write);
                }
            }

            write.WriteEndElement();
            write.WriteEndElement();
            write.Flush();
            write.Close();

            return Encoding.UTF8.GetString(stream.GetBuffer());
        }
    }
}
