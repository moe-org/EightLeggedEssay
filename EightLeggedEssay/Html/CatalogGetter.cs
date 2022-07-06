//===---------------------------------------------------===//
//                  CatalogGetter.cs
//
// this file is under the MIT License
// See https://opensource.org/licenses/MIT for license information.
// Copyright(c) 2020-2022 moe-org All rights reserved.
//
//===---------------------------------------------------===//

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EightLeggedEssay.Html
{

    /// <summary>
    /// 目录项。最多7层嵌套（得益于我们的傻逼用户会使用多个h1元素）
    /// </summary>
    public class CatalogItem
    {
        /// <summary>
        /// 默认初始化层级，如果为root则需要为1
        /// </summary>
        public int FormatLevel { get; set; } = 1;

        /// <summary>
        /// 当前目录项名称
        /// </summary>
        public string? Name { get; set; } = null;

        /// <summary>
        /// 子目录项
        /// </summary>
        public List<CatalogItem>? SubItems { get; set; } = null;

        public CatalogItem(string? name, List<CatalogItem>? sub = null)
        {
            Name = name;
            SubItems = sub;
        }

        public override string ToString()
        {
            StringBuilder builder = new();

            builder.AppendFormat("*-> {0}\n", Name ?? "null");
            if (SubItems != null)
            {
                foreach (var item in SubItems)
                {
                    item.FormatLevel = FormatLevel + 1;
                    builder.AppendFormat("{0}{1}", new string('\t', FormatLevel),item.ToString());
                }
            }
            return builder.ToString();
        }
    }

    /// <summary>
    /// 获取html的目录
    /// </summary>
    public class CatalogGetter
    {
        /// <summary>
        /// 逻辑顺序
        /// </summary>
        private List<(HtmlNode, int)> LogicLevelOrder = new();

        /// <summary>
        /// 遍历并获取所有html目录项，然后扁平展开
        /// </summary>
        /// <param name="node">目录项</param>
        private void VivitNode(HtmlNode node)
        {
            var regex = Regex.Match(node.Name, "h(?<Level>[1-6]{1})");

            if (regex.Success)
            {
                regex.Groups.TryGetValue("Level", out Group? value);

                int level = int.Parse(value!.Value);

                LogicLevelOrder.Add(new(node, level));
            }

            foreach (var sub in node.ChildNodes)
            {
                VivitNode(sub);
            }
        }

        /// <summary>
        /// 生成目录树
        /// </summary>
        private void Generate(ref int index, CatalogItem parent, ref int level)
        {
            // 父节点的子节点
            CatalogItem? lastChildren = null;

            while (index != LogicLevelOrder.Count)
            {
                var item = LogicLevelOrder[index];

                if (item.Item2 > level)
                {
                    // 子节点
                    // 继续处理
                    level++;

                    // 如果父节点没有子节点，则将孙节点附加到父节点上
                    // 否则将孙节点附加到子节点上
                    Generate(ref index, lastChildren ?? parent, ref level);
                }
                else if (level == item.Item2)
                {
                    // 添加父节点的子节点
                    lastChildren = new(item.Item1.InnerText, new());
                    parent.SubItems!.Add(lastChildren);
                    index++;
                }
                else
                {
                    // 返回父节点
                    // 让父节点来处理
                    level--;
                    return;
                }
            }
        }

        /// <summary>
        /// 获取文章目录结构
        /// </summary>
        /// <param name="html">html文章</param>
        /// <returns>目录结构</returns>
        public CatalogItem GetCatalog(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return GetCatalog(doc);
        }

        /// <summary>
        /// 获取文章目录结构
        /// </summary>
        /// <param name="html">html文章</param>
        /// <returns>目录结构</returns>
        public CatalogItem GetCatalog(HtmlDocument html)
        {
            LogicLevelOrder.Clear();

            foreach (var sub in html.DocumentNode.ChildNodes)
            {
                VivitNode(sub);
            }

            int i = 0;
            int l = 1;
            CatalogItem item = new("root", new());
            Generate(ref i, item, ref l);

            return item;
        }

    }
}
