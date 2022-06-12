//===---------------------------------------------------===//
//                  HttpServer.cs
//
// this file is under the MIT License
// See https://opensource.org/licenses/MIT for license information.
// Copyright(c) 2020-2022 moe-org All rights reserved.
//
//===---------------------------------------------------===//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace EightLeggedEssay.Cmdlet
{
    /// <summary>
    /// 启动http服务器
    /// </summary>
    [Cmdlet(VerbsLifecycle.Start, "HttpServer")]
    [OutputType(typeof(Server))]
    public class StartServer : PSCmdlet
    {
        public const string CallName = "Start-HttpServer";

        [Parameter(Mandatory = false, Position = 0, ValueFromPipeline = true)]
        public string Path { get; set; } = ".";

        protected override void ProcessRecord()
        {
            if (Path == null)
            {
                WriteError(
                    new ErrorRecord(
                        new ArgumentNullException(nameof(Path)), null, ErrorCategory.InvalidArgument,
                        null));
                return;
            }

            ServerConfiguration configuration = new()
            {
                Workpath = System.IO.Path.GetFullPath(Path)
            };

            Server server = new();
            server.Start(configuration);

            WriteObject(server);
        }
    }
}
