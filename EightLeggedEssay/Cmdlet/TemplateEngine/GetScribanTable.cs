//===---------------------------------------------------===//
//                  GetScribianTable.cs
//
// this file is under the MIT License
// See https://opensource.org/licenses/MIT for license information.
// Copyright(c) 2020-2022 moe-org All rights reserved.
//
//===---------------------------------------------------===//

using Scriban.Runtime;
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
    /// 从Hashtable获取用于编译Scriban的属性
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "ScribanTable")]
    [OutputType(typeof(ScriptObject))]
    public class GetScribanTable : PSCmdlet
    {
        public const string CallName = "Get-ScribanTable";

        /// <summary>
        /// 输入一个IDictionary
        /// </summary>
        [Parameter(ValueFromPipeline = true, Mandatory = true, Position = 0)]
        public PSObject? Source { get; set; } = null;

        /// <summary>
        /// 遍历
        /// </summary>
        /// <param name="unknown">要遍历的对象</param>
        /// <returns>返回string或者ScriptObject或者ScriptArray或者一个function callable。
        /// function callable的返回值同上。</returns>
        private static object? Visit(object? unknown)
        {
            // 对于null，直接返回
            if (unknown == null)
            {
                return null;
            }
            // 对于集合，遍历每个子单元，返回数组
            else if (unknown.GetType().IsAssignableTo(typeof(ICollection)))
            {
                var list = (ICollection)unknown;

                ScriptArray array = new();

                foreach (var item in list)
                {
                    array.Add(Visit(item));
                }

                return array;
            }
            // 对于数组，遍历每一对key，返回字典
            else if (unknown.GetType().IsAssignableTo(typeof(IDictionary)))
            {
                var dict = (IDictionary)unknown;
                ScriptObject obj = new();

                foreach (var key in dict.Keys)
                {
                    obj.Add(key.ToString(), Visit(dict[key]));
                }

                return obj;
            }
            // 对于函数块，使用惰性求值
            // 即在使用时才求值
            else if (unknown.GetType() == typeof(ScriptBlock))
            {
                var block = (ScriptBlock)unknown;

                Func<object?> func = () =>
                {
                    return Visit(block.InvokeReturnAsIs());
                };

                return func;
            }
            else
            {
                return unknown.ToString();
            }
        }

        protected override void ProcessRecord()
        {
            if (Source == null || !Source.BaseObject.GetType().IsAssignableTo(typeof(IDictionary)))
            {
                WriteError(new ErrorRecord(
                    new ArgumentNullException(
                        $"Source is null or isn't IDictionary. {Source?.BaseObject.GetType()}", nameof(Source)),
                    string.Empty,
                    ErrorCategory.InvalidArgument, Source));
                return;
            }

            ScriptObject result = new();

            IDictionary table = (IDictionary)Source.BaseObject;

            foreach (var key in table.Keys)
            {
                result.Add(key.ToString(), Visit(table[key]));
            }

            WriteObject(result);
        }
    }
}
