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
using System.Drawing;
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
                                response.Close();
                                return;
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
                            var buffer = File.ReadAllBytes(Path.Join(opt.Workpath, absPath));

                            // text
                            if (absPath.EndsWith(".html") || absPath.EndsWith(".htm"))
                            {
                                response.ContentType = "text/html";
                            }
                            else if (absPath.EndsWith(".mjs") || absPath.EndsWith(".js"))
                            {
                                response.ContentType = "application/javascript";
                            }
                            else if (absPath.EndsWith(".csv"))
                            {
                                response.ContentType = "text/csv";
                            }
                            else if (absPath.EndsWith(".css"))
                            {
                                response.ContentType = "text/css";
                            }
                            else if (absPath.EndsWith(".xml"))
                            {
                                response.ContentType = "text/xml";
                            }
                            else if (absPath.EndsWith(".xhtml") || absPath.EndsWith(".xht"))
                            {
                                response.ContentType = "application/xhtml+xml";
                            }
                            // image
                            else if (absPath.EndsWith(".gif"))
                            {
                                response.ContentType = "image/gif";
                            }
                            else if (absPath.EndsWith(".jpeg") || absPath.EndsWith(".jpg") || absPath.EndsWith(".jpe"))
                            {
                                response.ContentType = "image/jpeg";
                            }
                            else if (absPath.EndsWith(".png"))
                            {
                                response.ContentType = "image/png";
                            }
                            else if (absPath.EndsWith(".webp"))
                            {
                                response.ContentType = "image/webp";
                            }
                            else if (absPath.EndsWith(".svg") || absPath.EndsWith(".svgz"))
                            {
                                response.ContentType = "image/svg+xml";
                            }
                            else if (absPath.EndsWith(".tiff") || absPath.EndsWith(".tif"))
                            {
                                response.ContentType = "image/tiff";
                            }
                            else if (absPath.EndsWith(".ico") || absPath.EndsWith(".icon"))
                            {
                                response.ContentType = "image/x-icon";
                            }
                            // font
                            else if (absPath.EndsWith(".woff"))
                            {
                                response.ContentType = "font/woff";
                            }
                            else if (absPath.EndsWith(".woff2"))
                            {
                                response.ContentType = "font/woff2";
                            }
                            else if (absPath.EndsWith(".ttf") || absPath.EndsWith(".ttc")
                                || absPath.EndsWith(".otf") || absPath.EndsWith(".otc"))
                            {
                                response.ContentType = "font/otf";
                            }
                            // audio && video
                            else if (absPath.EndsWith(".mp3"))
                            {
                                response.ContentType = "audio/mpeg";
                            }
                            else if (absPath.EndsWith(".flac"))
                            {
                                response.ContentType = "audio/x-flac";
                            }
                            else if (absPath.EndsWith(".ogg") || absPath.EndsWith(".ogv") || absPath.EndsWith(".oga"))
                            {
                                response.ContentType = "application/ogg";
                            }
                            else if (absPath.EndsWith(".m4a") || absPath.EndsWith(".mp4"))
                            {
                                response.ContentType = "application/mp4";
                            }
                            // rss
                            else if (absPath.EndsWith(".rss"))
                            {
                                response.ContentType = "application/rss+xml";
                            }
                            // others
                            else
                            {
                                Printer.WarnLine("{0}:{1}{2}", "Http Server", "unknown response content type:", absPath);
                            }

                            response.ContentLength64 = buffer.Length;
                            Stream output = response.OutputStream;
                            response.StatusCode = ((int)HttpStatusCode.OK);
                            output.Write(buffer, 0, buffer.Length);
                            response.Close();
                        }
                        // 找不到文件
                        catch (FileNotFoundException)
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
                    finally
                    {
                        response.Close();
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
