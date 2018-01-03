using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCAD.OpenCADFormat.DataStrings
{
    public class DataStringGenerator
    {
        static private char[] getCharRange(char start, char end)
        {
            return Enumerable.Range(start, end - start + 1).Select(i => (char)i).ToArray();
        }

        static char[] LOWER_CASE_LETTER_CHARS { get; } = getCharRange('a', 'z');
        static char[] UPPER_CASE_LETTER_CHARS { get; } = getCharRange('A', 'Z');
        static char[] LETTER_CHARSET { get; } = LOWER_CASE_LETTER_CHARS.Concat(UPPER_CASE_LETTER_CHARS).ToArray();
        static char[] NUMERIC_CHARSET { get; } = getCharRange('0', '9');
        static char[] FLOAT_NUMERIC_CHARSET { get; } = NUMERIC_CHARSET.Concat(new char[] { '.' }).ToArray();
        static char[] ALPHANUMERIC_CHARSET { get; } =
            LOWER_CASE_LETTER_CHARS.Concat(UPPER_CASE_LETTER_CHARS).Concat(NUMERIC_CHARSET).ToArray();
        static char[] IDENTIFIER_CHARSET { get; } = ALPHANUMERIC_CHARSET.Concat(new char[] { '_' }).ToArray();
        static char[] LITERAL_CHARSET { get; } = NUMERIC_CHARSET.Concat(IDENTIFIER_CHARSET).Concat(new char[] { '#' }).ToArray();

        const char SEPARATOR_CHARACTER = ';';
        const char FUNC_PARAMS_OPENING_CHAR = '(';
        const char FUNC_PARAMS_CLOSING_CHAR = ')';
        const char PARAM_NAME_SEPARATOR_CHAR = ':';
        const char STRING_ENCLOSING_CHAR = '\'';

        public DataStringMainContext MainContext { get; private set; }

        public string Generate()
        {
            string result = "";

            writeAllItems(MainContext, ref result);

            return result;
        }

        private void writeAllItems(DataStringParameter parameter, ref string result)
        {
            for (int i = 0; i < parameter.Parameters.Count; i++)
            {
                DataStringParameter child = parameter.Parameters[i];

                if (i > 0)
                    result += SEPARATOR_CHARACTER;

                if (DataStringFunction.Test(child))
                {
                    var functionItem = child as DataStringFunction;
                    result += functionItem.Name + FUNC_PARAMS_OPENING_CHAR;

                    writeAllItems(child, ref result);

                    result += FUNC_PARAMS_CLOSING_CHAR;
                }
                else if (DataStringLiteralFloatingPoint.Test(child))
                {
                    var floatLiteralItem = child as DataStringLiteralFloatingPoint;
                    result += floatLiteralItem.Value.ToString();
                }
                else if (DataStringLiteralString.Test(child))
                {
                    var stringLiteralItem = child as DataStringLiteralString;
                    result += STRING_ENCLOSING_CHAR + stringLiteralItem.Value + STRING_ENCLOSING_CHAR;
                }
                else if (DataStringSymbol.Test(child))
                {
                    var symbolItem = child as DataStringSymbol;
                    result += symbolItem.Name;
                }
            }
        }

        public DataStringGenerator(DataStringMainContext mainContext)
        {
            MainContext = mainContext;
        }
    }
}