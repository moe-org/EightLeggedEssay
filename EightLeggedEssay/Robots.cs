//===---------------------------------------------------===//
//                  Robots.cs
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
using System.Threading.Tasks;

namespace EightLeggedEssay
{
    /// <summary>
    /// Robots.txt的实现
    /// </summary>
    public class Robots
    {
        /// <summary>
        /// single robot rule
        /// </summary>
        public class RobotRule
        {

            public string UserAgent { get; set; }

            public List<string> Allow { get; } = new();

            public List<string> Disallow { get; } = new();

            public RobotRule(string userAgent)
            {
                ArgumentNullException.ThrowIfNull(userAgent);
                UserAgent = userAgent;
            }
        }

        public List<string> Sitemaps = new();

        public List<RobotRule> Rules = new();


        /// <summary>
        /// 输出robots.txt的文本
        /// </summary>
        public string Write()
        {
            StringBuilder builder = new();

            foreach(var sitemap in Sitemaps) {
                builder.Append(string.Format("Sitemap: {0}",sitemap)).AppendLine();
            }

            if(Sitemaps.Count != 0)
            {
                // spertate sitemaps and rules
                builder.AppendLine();
            }

            foreach(var rule in Rules)
            {
                builder.Append(string.Format("User-agent: {0}", rule.UserAgent)).AppendLine();

                foreach(var dis in rule.Disallow)
                {
                    builder.Append(string.Format("Disallow {0}", dis)).AppendLine();
                }
                foreach (var allow in rule.Allow)
                {
                    builder.Append(string.Format("Allow {0}", allow)).AppendLine();
                }
            }

            builder.AppendLine();

            return builder.ToString();
        }

    }
}
