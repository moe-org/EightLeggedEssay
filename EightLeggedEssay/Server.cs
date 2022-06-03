//===---------------------------------------------------===//
//                  Server.cs
//
// this file is under the MIT License
// See https://opensource.org/licenses/MIT for license information.
// Copyright(c) 2020-2022 moe-org All rights reserved.
//
//===---------------------------------------------------===//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EightLeggedEssay
{
    /// <summary>
    /// 服务器配置
    /// </summary>
    public class ServerConfiguration
    {
        //============= User Options =============//
        /// <summary>
        /// work path
        /// </summary>
        public string Workpath { get; set; } = Path.GetFullPath("./");

        /// <summary>
        /// if not found page,return this
        /// </summary>
        public string NotFoundPage { get; set; } = "404.html";

        /// <summary>
        /// if no url,entry this
        /// </summary>
        public string IndexPage { get; set; } = "index.html";

        /// <summary>
        /// default ports
        /// </summary>
        public int Port { get; set; } = 8848;
    }


    /// <summary>
    /// 这个类提供了一个简单的HTTP服务器
    /// </summary>
    public class Server
    {

        private ServerConfiguration opt = new ServerConfiguration();

        //============= System Options =============//
        /// <summary>
        /// 锁
        /// </summary>
        private readonly object locker = new();

        /// <summary>
        /// 运行标志
        /// </summary>
        private bool running = false;

        /// <summary>
        /// 顶级url，在启动后设置
        /// </summary>
        private string RootUrl { get; set; } = "";

        /// <summary>
        /// 404 url，在启动后设置
        /// </summary>
        private string NotFoundUrl { get; set; } = "";

        /// <summary>
        /// 入口Url，在启动后设置
        /// </summary>
        private string IndexUrl { get; set; } = "";

        /// <summary>
        /// 初始化设置
        /// </summary>
        private HttpListener Configure()
        {
            RootUrl = "http://localhost:" + opt.Port + "/";
            NotFoundUrl = RootUrl + opt.NotFoundPage;
            IndexUrl = RootUrl + opt.IndexPage;

            HttpListener http = new();
            http.Prefixes.Add(RootUrl);

            return http;
        }

        /// <summary>
        /// run in server thread
        /// </summary>
        private void server()
        {
            var http = Configure();

            http.Start();

            // 检查是否中断运行
            void check()
            {
                lock (locker)
                {
                    if (running == false)
                    {
                        http.Abort();
                        throw new ThreadInterruptedException();
                    }
                }
            }

            // 开始监听循环
            try
            {
                while (true)
                {
                    // 等待接受请求
                    var contextTask = http.GetContextAsync();

                    while (!contextTask.Wait(50)) { check(); }

                    // 处理请求
                    var context = contextTask.Result;
                    var request = context.Request;
                    var response = context.Response;

                    response.KeepAlive = false;

                    // 处理连接请求
                    void processRequest(string? aim = null, bool hasNotFound = false)
                    {
                        // 处理路径
                        string absPath = "";

                        string fullPath = request.Url?.ToString() ?? "/";

                        if (aim != null)
                        {
                            // 我们已经有了目标路径
                            absPath = aim;
                        }
                        else
                        {
                            // 没有目标路径，我们自己检测
                            if (request.Url == null || request.Url.AbsolutePath == string.Empty || request.Url.AbsolutePath == "/")
                            {
                                // 用户没有访问任何URL或者访问了Root
                                // 千里转进index
                                response.Redirect(IndexUrl);
                                absPath = opt.IndexPage;
                            }
                            else
                            {
                                // 用户访问了某个URL
                                absPath = request.Url.AbsolutePath;
                            }
                        }

                        // 读取文件并写入
                        try
                        {
                            var bytes = File.ReadAllBytes(Path.Join(opt.Workpath, absPath));
                            response.OutputStream.Write(bytes);
                        }
                        // 找不到文件
                        catch(FileNotFoundException)
                        {
                            if (hasNotFound)
                            {
                                // NotFound.html也找不到
                                // 直接返回错误代码
                                response.StatusCode = ((int)HttpStatusCode.NotFound);
                                response.StatusDescription = "Not Found";
                                response.Close();
                            }
                            else
                            {
                                // 查找NotFound.html文件
                                processRequest(opt.NotFoundPage, true);
                            }
                        }


                        response.Close();
                    }

                    // 接受请求
                    try
                    {
                        processRequest();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                return;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
            }
        }

        /// <summary>
        /// start server in new thread
        /// </summary>
        public void Start(ServerConfiguration opt)
        {
            this.opt = opt;

            lock (locker)
            {
            }
            running = true;

            Thread t = new(server)
            {
                Name = "Http Server Thread"
            };
            t.Start();
        }

        /// <summary>
        /// stop the server
        /// </summary>
        public void Stop()
        {
            lock (locker)
            {
                running = false;
            }
        }
    }
}
