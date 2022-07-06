//===---------------------------------------------------===//
//                  HtmlChecker.cs
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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EightLeggedEssay.Html
{
    /// <summary>
    /// 检查出来的html错误
    /// </summary>
    public class HtmlError
    {
        public string Reason { get; }

        public long Line { get; }

        public long Column { get; }

        public string Part { get; }

        public HtmlError(string reason, HtmlNode? node = null)
        {
            Reason = reason;

            if (node is not null)
            {
                Column = node.LinePosition;
                Line = node.Line;
                Part = node.OuterHtml;
            }
            else
            {
                Line = -1;
                Column = -1;
                Part = "unknow html part";
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new();

            builder.Append("html check error:").Append(Reason).AppendLine();

            var line = Line != -1 ? Line.ToString() : "unknown";
            var column = Column != -1 ? Column.ToString() : "unknown";

            // 计算需要的字符数量
            int count = 0;

            Part.Split('\n').ToList().ForEach((str) => {
                if(str.Length > count)
                {
                    count = str.Length;
                }
            });

            builder
                .Append(string.Format("at line {0} at column {1}", line, column))
                .AppendLine()
                .Append(Part)
                .AppendLine()
                .Append(new string('^', count));

            return builder.ToString();
        }
    }

    /// <summary>
    /// 检查单个错误的html错误检查器
    /// </summary>
    public interface IHtmlCheckerItem
    {
        /// <summary>
        /// 检查html文档
        /// </summary>
        /// <param name="html"html文档></param>
        /// <param name="errors">html错误</param>
        /// <returns>如果检查到错误，返回true</returns>
        bool TryGetError(HtmlDocument html, List<HtmlError> errors);

        /// <summary>
        /// 初始化状态，通常会在每次检查进行之前被调用
        /// </summary>
        void StartNewCheck();
    }

    /// <summary>
    /// html检查器
    /// </summary>
    public interface IHtmlChecker {
        /// <summary>
        /// 检查html字符串
        /// </summary>
        /// <param name="html">要检查的html字符串</param>
        /// <param name="errors">产生的错误</param>
        /// <returns>如果发现错误则返回true，否则返回false</returns>
        bool TryGetError(string html, out List<HtmlError> errors)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return TryGetError(doc, out errors);
        }

        /// <summary>
        /// 检查html文档
        /// </summary>
        /// <param name="html">要检查的html文档</param>
        /// <param name="errors">产生的错误</param>
        /// <returns>如果发现错误则返回true，否则返回false</returns>
        bool TryGetError(HtmlDocument htmlDocument, out List<HtmlError> errors);
    }


    /// <summary>
    /// Html检查器。默认添加所有检查
    /// </summary>
    public class HtmlChecker : IHtmlChecker
    {
        /// <summary>
        /// 默认构造函数。默认添加所有检查
        /// </summary>
        public HtmlChecker()
        {
            Checkers.Add(new HtmlTitleCheck());
        }
        
        /// <summary>
        /// 使用checker初始化的构造函数
        /// </summary>
        /// <param name="checkers">要添加的初始checkers</param>
        public HtmlChecker(IEnumerable<IHtmlCheckerItem> checkers)
        {
            Checkers.AddRange(checkers);
        }

        /// <summary>
        /// 是否在碰到第一个错误的时候返回
        /// </summary>
        public bool ReturnOnFirstError { get; set; } = false;

        /// <summary>
        /// 决定要执行的html检查。所有检查器将会在使用前调用Reset函数
        /// </summary>
        public List<IHtmlCheckerItem> Checkers { get; } = new();

        /// <summary>
        /// 检查html
        /// </summary>
        /// <param name="htmlDocument">对html进行检查</param>
        /// <param name="firstError">所有html错误</param>
        /// <returns>如果返回true则代表检查到错误</returns>
        public bool TryGetError(HtmlDocument htmlDocument, out List<HtmlError> errors)
        {
            errors = new();

            foreach (var item in Checkers)
            {
                item.StartNewCheck();
                if (item.TryGetError(htmlDocument, errors) && ReturnOnFirstError)
                {
                    return true;
                }
            }

            return errors.Count != 0;
        }
    }

    /// <summary>
    /// 检查html的标题。这个会检查h1到h6的使用情况。会被认为是错误的：
    /// 没有h1标题。
    /// 没有使用h(n)就使用了h(n+1)，如没有使用h4就使用了h5。
    /// 
    /// </summary>
    public class HtmlTitleCheck : IHtmlCheckerItem
    {
        /// <summary>
        /// 已经使用过的标题等级
        /// </summary>
        private int UsedLevel = 0;

        /// <summary>
        /// 是否拥有h1
        /// </summary>
        private bool HasH1 = false;

        /// <summary>
        /// 这个函数将会在每次检查之前被调用
        /// </summary>
        public void StartNewCheck()
        {
            UsedLevel = 0;
            HasH1 = false;
        }

        public void CheckItem(HtmlNode node, List<HtmlError> errors)
        {
            var regex = Regex.Match(node.Name, "h(?<Level>[1-6]{1})");

            if (regex.Success)
            {
                // 检查
                if (regex.Groups.TryGetValue("Level", out Group? value))
                {
                    var level = int.Parse(value.Value);

                    // 检查H1
                    if (level == 1 && HasH1)
                    {
                        errors.Add(new("too many <h1> element(one is good)", node));
                    }
                    else if (level == 1)
                    {
                        HasH1 = true;

                        if (UsedLevel != 0)
                        {
                            errors.Add(new("<h1> isn't the first heading element", node));
                        }
                    }
                    // 检查其他标题
                    // 如果当前标题等级不是UsedLevel + 1或者小于等于UsedLevel
                    // 则说明我们跨过了某些标题等级!
                    else if ((UsedLevel + 1) < level)
                    {
                        errors.Add(new(string.Format("please using <h{0}> before using <h{1}>!", UsedLevel + 1, level), node));
                    }
                    UsedLevel = level;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            // 迭代子元素
            foreach (var item in node.ChildNodes)
            {
                CheckItem(item, errors);
            }
        }

        public bool TryGetError(HtmlDocument html, List<HtmlError> errors)
        {
            int originError = errors.Count;

            // 开查
            foreach (var item in html.DocumentNode.ChildNodes)
            {
                CheckItem(item, errors);
            }

            if (!HasH1)
            {
                // 没有h1标题
                // 报错
                errors.Add(new("not found <h1> element(one is good)"));
            }

            return originError != errors.Count;
        }
    }

}
