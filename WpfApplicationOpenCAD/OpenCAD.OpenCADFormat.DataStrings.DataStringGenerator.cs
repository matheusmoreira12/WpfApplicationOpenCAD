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
        private static char[] getCharRange(char start, char end)
        {
            return Enumerable.Range(start, end - start + 1).Select(i => (char)i).ToArray();
        }
        
        public DataStringMainContext MainContext { get; private set; }

        public string Generate()
        {
            string result = "";

            writeAllItems(MainContext, ref result);

            return result;
        }

        private void writeAllItems(DataStringItem parameter, ref string result)
        {
            for (int i = 0; i < parameter.Items.Count; i++)
            {
                DataStringItem child = parameter.Items[i];

                if (i > 0)
                    result += GlobalConsts.SEPARATOR_CHARACTER;

                if (child is DataStringFunction)
                {
                    var functionItem = child as DataStringFunction;
                    result += functionItem.Name + GlobalConsts.FUNC_PARAMS_OPENING_CHAR;

                    writeAllItems(child, ref result);

                    result += GlobalConsts.FUNC_PARAMS_CLOSING_CHAR;
                }
                else if (child is DataStringLiteralFloatingPoint)
                {
                    var floatLiteralItem = child as DataStringLiteralFloatingPoint;
                    result += floatLiteralItem.Value.ToString();
                }
                else if (child is DataStringLiteralString)
                {
                    var stringLiteralItem = child as DataStringLiteralString;
                    result += GlobalConsts.STRING_ENCLOSING_CHAR + stringLiteralItem.Value + GlobalConsts.STRING_ENCLOSING_CHAR;
                }
                else if (child is DataStringSymbol)
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