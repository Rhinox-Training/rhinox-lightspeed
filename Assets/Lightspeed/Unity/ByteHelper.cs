using System;
using System.Linq;

namespace Rhinox.Utilities
{
    public static class ByteHelper
    {
        public static byte[] Merge(params byte[][] byteArrs)
        {
            int totalLength = byteArrs.Sum(x => x.Length);
            int offset = 0;
            byte[] resultArr = new byte[totalLength];
            foreach (var byteArr in byteArrs)
            {
                Array.Copy(byteArr, 0, resultArr,
                    offset, byteArr.Length);
                offset += byteArr.Length;
            }

            return resultArr;
        }
    }
}