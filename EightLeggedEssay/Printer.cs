//===---------------------------------------------------===//
//                  Printer.cs
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
    /// 打印机
    /// </summary>
    public static class Printer
    {

        public static readonly object locker = new();

        public static void PutLine(string fmt, params object?[] args)
        {
            lock (locker)
            {
                Console.Out.WriteLine("{0}", string.Format(fmt, args));
            }
        }

        public static void WarnLine(string fmt, params object?[] args)
        {
            lock (locker)
            {
                var colored = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Out.WriteLine("{0}", string.Format(fmt, args));
                Console.ForegroundColor = colored;
            }
        }

        public static void ErrLine(string fmt, params object?[] args)
        {
            lock (locker)
            {
                var colored = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("{0}", string.Format(fmt, args));
                Console.ForegroundColor = colored;
            }
        }

        public static void OkLine(string fmt, params object?[] args)
        {
            lock (locker)
            {
                var colored = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine("{0}", string.Format(fmt, args));
                Console.ForegroundColor = colored;
            }
        }

        public static void Put(string fmt, params object?[] args)
        {
            lock (locker)
            {
                Console.Out.Write("{0}", string.Format(fmt, args));
            }
        }

        public static void Warn(string fmt, params object?[] args)
        {
            lock (locker)
            {
                var colored = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Out.Write("{0}", string.Format(fmt, args));
                Console.ForegroundColor = colored;
            }
        }

        public static void Err(string fmt, params object?[] args)
        {
            lock (locker)
            {
                var colored = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.Write("{0}", string.Format(fmt, args));
                Console.ForegroundColor = colored;
            }
        }

        public static void Ok(string fmt, params object?[] args)
        {
            lock (locker)
            {
                var colored = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.Write("{0}", string.Format(fmt, args));
                Console.ForegroundColor = colored;
            }
        }

        /// <summary>
        /// 不格式化的输出
        /// </summary>
        /// <param name="str"></param>
        public static void NoFormatErr(string str)
        {
            Err("{0}", str);
        }

        /// <summary>
        /// 不格式化的输出
        /// </summary>
        /// <param name="str"></param>
        public static void NoFormatErrLine(string str)
        {
            ErrLine("{0}", str);
        }

        /// <summary>
        /// 不格式化的输出
        /// </summary>
        /// <param name="str"></param>
        public static void NoFormatPut(string str)
        {
            Put("{0}", str);
        }

        /// <summary>
        /// 不格式化的输出
        /// </summary>
        /// <param name="str"></param>
        public static void NoFormatPutLine(string str)
        {
            PutLine("{0}", str);
        }

        /// <summary>
        /// 不格式化的输出
        /// </summary>
        /// <param name="str"></param>
        public static void NoFormatWarn(string str)
        {
            Warn("{0}", str);
        }

        /// <summary>
        /// 不格式化的输出
        /// </summary>
        /// <param name="str"></param>
        public static void NoFormatWarnLine(string str)
        {
            WarnLine("{0}", str);
        }

        /// <summary>
        /// 不格式化的输出
        /// </summary>
        /// <param name="str"></param>
        public static void NoFormatOk(string str)
        {
            Ok("{0}", str);
        }

        /// <summary>
        /// 不格式化的输出
        /// </summary>
        /// <param name="str"></param>
        public static void NoFormatOkLine(string str)
        {
            OkLine("{0}", str);
        }

    }
}
