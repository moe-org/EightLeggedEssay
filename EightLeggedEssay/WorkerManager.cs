//===---------------------------------------------------===//
//                  WorkerManager.cs
//
// this file is under the MIT License
// See https://opensource.org/licenses/MIT for license information.
// Copyright(c) 2020-2022 moe-org All rights reserved.
//
//===---------------------------------------------------===//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EightLeggedEssay
{
    /// <summary>
    /// 线程工作异常
    /// </summary>
    public class ThreadJobException : Exception
    {
        /// <summary>
        /// 正在执行的任务，如果没有，则为null
        /// </summary>
        public Func<object?>? Task { get; set; }

        /// <summary>
        /// 出事的线程
        /// </summary>
        public Thread Thread { get; set; } = Thread.CurrentThread;

        public ThreadJobException(string msg, Exception err) : base(msg, err) { }
    }

    /// <summary>
    /// 线程工作实现器
    /// </summary>
    public class WorkerManager
    {
        /// <summary>
        /// 执行错误
        /// </summary>
        public readonly ConcurrentBag<ThreadJobException> Errors = new();

        /// <summary>
        /// 执行结果
        /// </summary>
        public readonly ConcurrentBag<object?> Results = new();

        private readonly string Name;
        private readonly long ThreadCount;

        /// <summary>
        /// 线程列表
        /// </summary>
        private readonly List<Thread> Threads = new();

        /// <summary>
        /// 锁
        /// </summary>
        private readonly object Locker = new();

        /// <summary>
        /// 初始化一个线程工作器
        /// </summary>
        /// <param name="count">线程数量</param>
        /// <param name="name">人类可读的表示符</param>
        public WorkerManager(long count, string name)
        {
            if (count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "count must not be zero");
            }

            ThreadCount = count;
            Name = name;
        }

        /// <summary>
        /// 开始任务
        /// </summary>
        /// <param name="executable">要执行的任务</param>
        public void Start(Func<object?> executable)
        {
            lock (Locker)
            {
                for (long index = 0; index < ThreadCount; index++)
                {
                    Threads.Add(
                        new Thread(() =>
                        {
                            try
                            {
                                var obj = executable.Invoke();

                                Results.Add(obj);
                            }
                            catch (Exception err)
                            {
                                Errors.Add(new ThreadJobException("execute failed down", err)
                                {
                                    Task = executable
                                });
                            }

                        })
                        {
                            Name = $"Job-{Name}-{index}"
                        }
                    );
                }

                Threads.ForEach(t => t.Start());
            }
        }

        /// <summary>
        /// 等待所有线程执行完毕
        /// </summary>
        public void Wait()
        {
            while (true)
            {
                Thread t;
                lock (Locker)
                {
                    if (Threads.Count != 0)
                    {
                        t = Threads[0];
                        Threads.RemoveAt(0);
                    }
                    else
                    {
                        return;
                    }
                }
                t.Join();
            }
        }
    }
}
