using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse
{
    public static class Extensions
    {
        public static int GetInt(this byte[] arr, int offset)
        {
            return BitConverter.ToInt32(arr.Skip(offset).Take(4).Reverse().ToArray(), 0);
        }

        public static uint GetUInt(this byte[] arr, int offset)
        {
            return BitConverter.ToUInt32(arr.Skip(offset).Take(4).Reverse().ToArray(), 0);
        }

        public static string GetString(this byte[] arr, int offset, int size)
        {
            return Encoding.ASCII.GetString(arr.Skip(offset).Take(size).ToArray());
        }

        public static byte[] TrimLeadingZeros(this byte[] arr, int offset)
        {
            return Trim(arr, 0x0, offset);
        }

        public static byte[] Trim(this byte[] arr, byte toTrim, int offset)
        {
            return arr.Skip(offset).SkipWhile(x => x == toTrim).ToArray();
        }

        public static byte[] Merge(this byte[] arr, params byte[][] arrays)
        {
            byte[] rv = new byte[arr.Length + arrays.Sum(a => a.Length)];
            int offset = 0;

            System.Buffer.BlockCopy(arr, 0, rv, offset, arr.Length);
            offset += arr.Length;

            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
    }
}
