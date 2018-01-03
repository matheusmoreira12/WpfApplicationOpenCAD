using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace OpenCAD
{
    namespace OpenCADFormat
    {
        namespace DataConversion
        {
            public struct ArbitraryFloat
            {
                static public explicit operator double(ArbitraryFloat me)
                {   
                    return 0;
                }

                static public explicit operator float(ArbitraryFloat me)
                {
                    return 0;
                }

                static public explicit operator ArbitraryFloat(double value)
                {
                    return new ArbitraryFloat();
                }

                static public explicit operator ArbitraryFloat(float value)
                {
                    return new ArbitraryFloat();
                }

                static public ArbitraryFloat Parse(string s)
                {
                    return new ArbitraryFloat();
                }

                private BigInteger mantissa;
                private int exponent;

                private ArbitraryFloat(BigInteger mantissa, int exponent)
                {
                    this.mantissa = mantissa;
                    this.exponent = exponent;
                }
            }

            public static class BinaryConversion
            {
                static public byte[] AsByteArray(this BitArray bits)
                {
                    byte[] result = new byte[(bits.Length + 7) / 8];

                    bits.CopyTo(result, 0);

                    return result;
                }

                static public BitArray FromString(string s)
                {
                    BitArray result = new BitArray(s.Length);

                    for (int i = 0; i < s.Length; i++)
                    {
                        switch (s[i])
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
                    }

                    return result;
                }
            }
        }
    }
}