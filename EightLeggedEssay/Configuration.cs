//===---------------------------------------------------===//
//                  Configuration.cs
//
// this file is under the MIT License
// See https://opensource.org/licenses/MIT for license information.
// Copyright(c) 2020-2022 moe-org All rights reserved.
//
//===---------------------------------------------------===//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections;
using System.Text.Json.Nodes;

namespace EightLeggedEssay
{
    /// <summary>
    /// 站点配置文件
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// 全局配置文件
        /// </summary>
        public static Configuration GlobalConfiguration { get; private set; } = new Configuration();

        /// <summary>
        /// 全局配置文件的源文本
        /// </summary>
        public static string GlobalConfigurationText { get; private set; } = string.Empty;

        /// <summary>
        /// 站点URL
        /// </summary>
        public string RootUrl { get; set; } = "";

        /// <summary>
        /// 站点输出目录
        /// </summary>
        public string OutputDirectory { get; set; } = "site";

        /// <summary>
        /// 用于构建的脚本
        /// </summary>
        public string BuildScript { get; set; } = "build-EightLeggedEssay.ps1";

        /// <summary>
        /// 内容目录
        /// </summary>
        public string ContentDirectory { get; set; } = "content";

        /// <summary>
        /// 资源目录
        /// </summary>
        public string SourceDirectory { get; set; } = "source";

        /// <summary>
        /// 主题目录
        /// </summary>
        public string ThemeDirectory { get; set; } = "theme";

        /// <summary>
        /// 用户自定义设置
        /// </summary>
        public Dictionary<string, JsonNode> UserConfiguration { get; set; } = new();

        /// <summary>
        /// 映射command到脚本文件
        /// </summary>
        public Dictionary<string, string> Commands { get; set; } = new();

        /// <summary>
        /// 保存配置文件到路径
        /// </summary>
        /// <param name="path">指定路径</param>
        public static void SaveTo(string path)
        {
            var config = GlobalConfiguration;

            JsonSerializerOptions opt = new()
            {
                WriteIndented = true
            };

            GlobalConfigurationText = JsonSerializer.Serialize(config, opt);

            if (!File.Exists(path))
            {
                using var f = File.Create(path);
                f.Write(Encoding.UTF8.GetBytes(GlobalConfigurationText));
            }
            else
            {
                File.WriteAllBytes(path, Encoding.UTF8.GetBytes(GlobalConfigurationText));
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>获取的配置文件</returns>
        public static void ReadFrom(string path)
        {
            GlobalConfigurationText = File.ReadAllText(path, Encoding.UTF8);
            GlobalConfiguration = JsonSerializer.Deserialize<Configuration>(GlobalConfigurationText)
                ?? throw new JsonException("empty json");
        }
    }
}
