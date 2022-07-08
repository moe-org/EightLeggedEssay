//===---------------------------------------------------===//
//                  Tools.cs
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
    /// IO工具
    /// </summary>
    public static class IOUtility
    {

        /// <summary>
        /// 创建一个文件的父目录（如果不存在的话）
        /// </summary>
        /// <param name="file">文件的路径</param>
        public static void CreateParents(string file)
        {
            var path = new FileInfo(file);
            if (path?.Directory?.FullName is not null)
            {
                if (!Directory.Exists(path.Directory.FullName))
                {
                    Directory.CreateDirectory(path.Directory.FullName);
                }
            }
        }

        /// <summary>
        /// 从流中读取一个long作为长度，然后读取内容
        /// </summary>
        /// <param name="input">输入流</param>
        /// <returns>读取到的内容</returns>
        public static byte[] ReadContentWithLongLngeht(Stream input)
        {
            var lengthBuf = new byte[8];

            input.Read(lengthBuf);

            var length = BitConverter.ToInt64(lengthBuf);

            var buf = new byte[length];

            input.Read(buf);

            return buf;
        }



    }







}
