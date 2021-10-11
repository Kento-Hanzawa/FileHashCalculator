using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHashCalculator
{
    public static class HexConverter
    {
        public static string ToHexString(in ReadOnlySpan<byte> value, bool isUpperCase = true)
        {
            if (value.Length == 0)
            {
                return string.Empty;
            }

            ReadOnlySpan<char> hex = isUpperCase
                ? stackalloc char[16] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' }
                : stackalloc char[16] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
            int hexLength = value.Length * 2;
            char[] buffer = ArrayPool<char>.Shared.Rent(hexLength);
            try
            {
                int index = 0;
                foreach (byte b in value)
                {
                    buffer[index++] = hex[unchecked(b >> 4)];
                    buffer[index++] = hex[unchecked(b & 0x0F)];
                }
                return new string(buffer.AsSpan(0, hexLength));
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }
    }
}
