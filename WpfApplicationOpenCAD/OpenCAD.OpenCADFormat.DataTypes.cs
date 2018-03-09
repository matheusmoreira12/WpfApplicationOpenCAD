using OpenCAD.Utils;
using System;
using System.Numerics;

namespace OpenCAD.OpenCADFormat.DataTypes
{
    public struct BigFloat
    {
        #region String Conversion
        private static BigInteger parseMantissa(string decimalStr, int decimalEnd)
        {
            string mantissaStr = decimalStr.Substring(0, decimalEnd - 0).Replace(StringUtils.DECIMAL_SEPARATOR.ToString(), "");

            BigInteger mantissa;
            if (!BigInteger.TryParse(mantissaStr, out mantissa))
                throw new InvalidOperationException("Cannot parse BigFloat from the specified string. Invalid mantissa.");

            return mantissa;
        }

        private static int parseExponent(string decimalStr, int exponentStart)
        {
            string exponentStr = decimalStr.Substring(exponentStart, decimalStr.Length - 1 - exponentStart);

            int exponent = 0;
            if (!int.TryParse(decimalStr, out exponent))
                throw new InvalidOperationException("Cannot parse BigFloat from the specified string. Invalid exponent.");

            return exponent;
        }

        private static int parseExponentShift(string decimalStr)
        {
            int floatingPointIndex = decimalStr.IndexOf(StringUtils.DECIMAL_SEPARATOR);

            return floatingPointIndex - 1;
        }

        public static BigFloat Parse(string value)
        {
            StringScanner scanner = new StringScanner(value);
            bool isFloatingPoint,
                hasExponent;
            string decimalStr;

            if (StringUtils.ReadDecimalString(scanner, out decimalStr, out isFloatingPoint, out hasExponent))
            {
                int decimalEnd = hasExponent ? decimalStr.IndexOf(StringUtils.DECIMAL_EXPONENT_CHARACTER) : (decimalStr.Length - 1),
                    exponentStart = decimalEnd + 2;

                int exponent = 0;
                if (hasExponent)
                    exponent = parseExponent(decimalStr, exponentStart);

                if (isFloatingPoint)
                    exponent += parseExponentShift(decimalStr);

                BigInteger mantissa = parseMantissa(decimalStr, decimalEnd);

                return new BigFloat(mantissa, exponent);
            }
            else
                throw new InvalidOperationException("Cannot parse BigFloat from the specified string.");
        }

        public override string ToString()
        {
            return null;
        }
        #endregion

        public static explicit operator double(BigFloat me)
        {
            return (int)me.mantissa * Math.Pow(10, me.exponent);
        }

        public static explicit operator float(BigFloat me)
        {
            return (float)(double)me;
        }

        public static explicit operator BigFloat(double value)
        {
            return new BigFloat();
        }

        public static explicit operator BigFloat(float value)
        {
            return new BigFloat();
        }

        public static BigFloat Parse(string s)
        {
            return new BigFloat();
        }

        private BigInteger mantissa;
        private int exponent;

        private BigFloat(BigInteger mantissa, int exponent)
        {
            this.mantissa = mantissa;
            this.exponent = exponent;
        }
    }
}