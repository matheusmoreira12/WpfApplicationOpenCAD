using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace OpenCAD.OpenCADFormat.DataConversion
{
    using Utils;

    public static class ObjectConversion
    {
        public static bool TryConvertTo<T>(object value, out T result) where T : class
        {
            try
            {
                result = (T)(dynamic)value;
                return true;
            }
            catch { }

            result = null;
            return false;
        }

        public static bool TryConvertTo<T>(object value, out T? result) where T : struct
        {
            try
            {
                result = (T)(dynamic)value;
                return true;
            }
            catch { }

            result = null;
            return false;
        }
    }

    public static class BinaryConversion
    {
        public const ushort BIT_MASK = 0x1;

        public static void SetRange(this BitArray bitArray, ushort value, int startIndex, int count)
        {
            for (int i = 0; i < count; i++)
                bitArray.Set(i + startIndex, Convert.ToBoolean((value >> i) & BIT_MASK));
        }

        public static ushort GetRange(this BitArray bitArray, int startIndex, int count)
        {
            ushort result = 0;

            for (int i = 0; i < count; i++)
            {
                ushort bitValue = (ushort)(Convert.ToUInt16(bitArray.Get(i + startIndex)) & BIT_MASK);
                result = (ushort)(result | bitValue << i);
            }

            return result;
        }

        public static byte[] AsByteArray(this BitArray bits)
        {
            byte[] result = new byte[(bits.Length + 7) / 8];

            bits.CopyTo(result, 0);

            return result;
        }

        public static string ToRepresentation(BitArray value, int fromBase)
        {
            int bitsPerChar;

            if (fromBase == 2)
                bitsPerChar = 1;
            else if (fromBase == 8)
                bitsPerChar = 3;
            else if (fromBase == 16)
                bitsPerChar = 4;
            else
                throw new InvalidOperationException(@"Cannot convert to representation. Base needs to be 2 - binary, 
8 - octal or 16 - hexadecimal.");

            string result = "";

            for (int i = 0; i < value.Length / bitsPerChar; i++)
                result += Convert.ToString(value.GetRange(i * bitsPerChar, bitsPerChar), fromBase);

            return result;
        }

        public static BitArray FromRepresentation(string rep, int fromBase)
        {
            int bitsPerChar;

            if (fromBase == 2)
                bitsPerChar = 1;
            else if (fromBase == 8)
                bitsPerChar = 3;
            else if (fromBase == 16)
                bitsPerChar = 4;
            else
                throw new InvalidOperationException(@"Cannot convert from representation. Base needs to be 2 - binary, 
8 - octal or 16 - hexadecimal.");

            BitArray result = new BitArray(rep.Length * bitsPerChar);

            for (int i = 0; i < rep.Length; i++)
                result.SetRange(Convert.ToUInt16(rep.Substring(i, 1), fromBase), i * bitsPerChar, bitsPerChar);

            return result;
        }

        private static BitArray fromBinaryRepresentation(string rep)
        {
            BitArray result = new BitArray(rep.Length);

            for (int i = 0; i < rep.Length; i++)
                switch (rep[i])
                {
                    case '0':
                        result.Set(i, false);
                        break;
                    case '1':
                        result.Set(i, true);
                        break;
                    default:
                        throw new InvalidOperationException("Invalid binary digit found during conversion.");
                }

            return result;
        }
    }
}