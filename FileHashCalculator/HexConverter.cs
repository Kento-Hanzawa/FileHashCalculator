using System;
using System.Buffers;

namespace FileHashCalculator
{
    public static class HexConverter
    {
        // Hexadecimal Chars
        private static readonly char[] _HEX = "0123456789ABCDEF".ToCharArray();
        private static readonly char[] _hex = "0123456789abcdef".ToCharArray();

        public static string ToHexString(in ReadOnlySpan<byte> value, bool isUpperCase = true)
        {
            if (value.Length == 0)
            {
                return string.Empty;
            }

            int length = value.Length * 2; // byte -> "00" ~ "FF"
            char[] hex = isUpperCase ? _HEX : _hex;
            char[] buffer = ArrayPool<char>.Shared.Rent(length);
            try
            {
                int index = 0;
                foreach (byte b in value)
                {
                    buffer[index++] = hex[unchecked(b >> 4)];
                    buffer[index++] = hex[unchecked(b & 0x0F)];
                }
                return new string(buffer.AsSpan(0, length));
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }
    }
}
