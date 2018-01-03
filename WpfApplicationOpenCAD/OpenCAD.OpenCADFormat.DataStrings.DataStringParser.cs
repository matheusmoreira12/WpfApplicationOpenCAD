using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenCAD.OpenCADFormat.DataConversion;
using System.Numerics;

namespace OpenCAD.OpenCADFormat.DataStrings
{
    public class DataStringParser
    {
        static private char[] getCharRange(char start, char end)
        {
            return Enumerable.Range(start, end - start + 1).Select(i => (char)i).ToArray();
        }

        static char[] LOWER_CASE_LETTER_CHARS { get; } = getCharRange('a', 'z');
        static char[] UPPER_CASE_LETTER_CHARS { get; } = getCharRange('A', 'Z');
        static char[] LETTER_CHARSET { get; } = LOWER_CASE_LETTER_CHARS.Concat(UPPER_CASE_LETTER_CHARS).ToArray();
        static char[] NUMERIC_CHARSET { get; } = getCharRange('0', '9');
        static char[] HEXADECIMAL_CHARSET { get; } = NUMERIC_CHARSET.Concat(getCharRange('a', 'f')).ToArray();
        static char[] ALPHANUMERIC_CHARSET { get; } =
            LOWER_CASE_LETTER_CHARS.Concat(UPPER_CASE_LETTER_CHARS).Concat(NUMERIC_CHARSET).ToArray();
        static char[] IDENTIFIER_CHARSET { get; } = ALPHANUMERIC_CHARSET.Concat(new char[] { '_' }).ToArray();
        static char[] LITERAL_CHARSET { get; } = NUMERIC_CHARSET.Concat(IDENTIFIER_CHARSET).Concat(new char[] { '#' }).ToArray();
        static char[] WHITESPACE_CHARACTERS = new char[] { ' ', '\n' };

        const char SEPARATOR_CHARACTER = ';';
        const char FUNC_PARAMS_OPENING_CHAR = '(';
        const char FUNC_PARAMS_CLOSING_CHAR = ')';
        const char PARAM_NAME_SEPARATOR_CHAR = ':';
        const char STRING_ENCLOSING_CHAR = '\'';

        public string TextContent { get; private set; }

        private char currentChar
        {
            get
            {
                if (scanIndex >= TextContent.Length)
                    return char.MinValue;

                return TextContent[scanIndex];
            }
        }

        private int scanIndex;

        public DataStringMainContext Parse()
        {
            DataStringMainContext result = new DataStringMainContext();

            scanIndex = 0;

            readAllItems(result);

            return result;
        }

        private void readAllItems(DataStringParameter result)
        {
            DataStringParameter generatedItem;

            while (readDataStringParameter(out generatedItem) || readDataStringSeparator())
            {
                if (generatedItem != null)
                    result.Parameters.Add(generatedItem);
            }
        }

        private bool readDataStringSeparator()
        {
            if (currentChar == SEPARATOR_CHARACTER)
            {
                scanIndex++;

                while (WHITESPACE_CHARACTERS.Contains(currentChar))
                    scanIndex++;

                return true;
            }

            return false;
        }

        private bool readDataStringParameter(out DataStringParameter parameter)
        {
            return readDataStringFunction(out parameter) || readDataStringSymbol(out parameter) || readDataStringLiteral(out parameter);
        }

        private bool readDataStringLiteral(out DataStringParameter parameter)
        {
            return readDataStringLiteralString(out parameter) || readDataStringBinary(out parameter) || readDataStringFloat(out parameter);
        }

        private bool readDataStringBinary(out DataStringParameter parameter)
        {
            int initialIndex = scanIndex;

            if (currentChar == '0')
            {
                scanIndex++;

                bool literalIsBinary = currentChar == 'b',
                    literalIsHexadecimal = currentChar == 'x';

                if (literalIsBinary || literalIsHexadecimal)
                {
                    scanIndex++;

                    while (literalIsBinary && "01".Contains(currentChar) ||
                        literalIsHexadecimal && HEXADECIMAL_CHARSET.Contains(currentChar))
                        scanIndex++;

                    int valueStart = initialIndex + 2;
                    string valueStr = TextContent.Substring(valueStart, scanIndex - valueStart);

                    if (literalIsBinary)
                    {
                        parameter = new DataStringLiteralBinary(BinaryConversion.FromString(valueStr).AsByteArray());
                        return true;
                    }
                    else if (literalIsHexadecimal)
                    {
                        parameter = new DataStringLiteralHexadecimal(valueStr);
                        return true;
                    }
                }
            }

            scanIndex = initialIndex;
            parameter = null;
            return false;
        }

        private bool readDataStringFloat(out DataStringParameter parameter)
        {
            int initialIndex = scanIndex;

            bool isFloatingPoint = false;

            if (NUMERIC_CHARSET.Contains(currentChar))
            {
                if (currentChar == '-' || currentChar == '+')
                    scanIndex++;

                while (NUMERIC_CHARSET.Contains(currentChar))
                    scanIndex++;

                if (currentChar == '.')
                {
                    scanIndex++;

                    while (NUMERIC_CHARSET.Contains(currentChar))
                        scanIndex++;

                    isFloatingPoint = true;
                }

                string valueStr = TextContent.Substring(initialIndex, scanIndex - initialIndex);

                if (isFloatingPoint)
                    parameter = new DataStringLiteralFloatingPoint(ArbitraryFloat.Parse(valueStr));
                else
                    parameter = new DataStringLiteralInteger(BigInteger.Parse(valueStr));

                return true;
            }

            scanIndex = initialIndex;
            parameter = null;
            return false;
        }

        private bool readDataStringLiteralString(out DataStringParameter parameter)
        {
            int initialIndex = scanIndex;

            if (currentChar == STRING_ENCLOSING_CHAR)
            {
                scanIndex++;

                while (currentChar != '\0')
                {
                    if (currentChar == STRING_ENCLOSING_CHAR)
                        break;

                    scanIndex++;
                }

                if (scanIndex > initialIndex)
                {
                    scanIndex++;

                    parameter = new DataStringLiteralString(TextContent.Substring(initialIndex + 1, scanIndex - initialIndex - 2));
                    return true;
                }
            }

            scanIndex = initialIndex;
            parameter = null;
            return false;
        }

        private bool readDataStringSymbol(out DataStringParameter parameter)
        {
            string symbolName;

            if (readIdentifier(out symbolName))
            {
                parameter = new DataStringSymbol(symbolName);
                return true;
            }

            parameter = null;
            return false;
        }

        private bool readDataStringFunction(out DataStringParameter parameter)
        {
            int initialIndex = scanIndex;

            string functionName;

            if (readIdentifier(out functionName))
            {
                if (currentChar == FUNC_PARAMS_OPENING_CHAR)
                {
                    scanIndex++;

                    var generatedFunction = new DataStringFunction(functionName);

                    readAllItems(generatedFunction);

                    if (currentChar == FUNC_PARAMS_CLOSING_CHAR)
                    {
                        scanIndex++;

                        parameter = generatedFunction;
                        return true;
                    }
                    else
                        throw new InvalidOperationException("Could not parse string. Function parameters end expected.");
                }
            }

            scanIndex = initialIndex;
            parameter = null;
            return false;
        }

        private bool readIdentifier(out string identifier)
        {
            int initialIndex = scanIndex;

            if (LETTER_CHARSET.Contains(currentChar))
            {
                scanIndex++;

                while (ALPHANUMERIC_CHARSET.Contains(currentChar))
                    scanIndex++;

                if (scanIndex > initialIndex)
                {
                    identifier = TextContent.Substring(initialIndex, scanIndex - initialIndex);
                    return true;
                }
            }

            scanIndex = initialIndex;
            identifier = null;
            return false;
        }

        public DataStringParser(string textContent)
        {
            TextContent = textContent;
        }
    }
}