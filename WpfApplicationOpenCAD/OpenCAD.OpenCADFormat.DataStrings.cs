using OpenCAD.OpenCADFormat.DataConversion;
using OpenCAD.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenCAD.OpenCADFormat.DataStrings
{
    internal static class GlobalConsts
    {
        private static char[] getCharRange(char start, char end)
        {
            return Enumerable.Range(start, end - start + 1).Select(i => (char)i).ToArray();
        }

        public static readonly char[] NUMERIC_CHARSET = getCharRange('0', '9');
        public static readonly char[] BINARY_CHARSET = new[] { '0', '1' };
        public static readonly char[] OCTAL_CHARSET = getCharRange('1', '8');
        public static readonly char[] HEXADECIMAL_CHARSET = NUMERIC_CHARSET.Concat(getCharRange('a', 'f')).ToArray();
        public static readonly char[] ALPHANUMERIC_CHARSET = StringUtils.LETTERS_CHARSET.Concat(StringUtils.NUMERAL_CHARSET).ToArray();
        public static readonly char[] WHITESPACE_CHARACTERS = new[] { ' ', '\n' };

        public const char SEPARATOR_CHARACTER = ';';
        public const char FUNC_PARAMS_OPENING_CHAR = '(';
        public const char FUNC_PARAMS_CLOSING_CHAR = ')';
        public const char PARAM_NAME_SEPARATOR_CHAR = ':';
        public const char STRING_ENCLOSING_CHAR = '\'';
    }

    internal static class Utils
    {
        public static string EncloseString(string content)
        {
            return string.Format("{0}{1}{0}", GlobalConsts.STRING_ENCLOSING_CHAR, content);
        }

        public static bool ReadIdentifier(StringScanner scanner, out string identifier)
        {
            using (var token = scanner.SaveIndex())
            {
                if (StringUtils.CharIsLetter(scanner.CurrentChar))
                {
                    scanner.Increment();

                    while (StringUtils.WORD_CHARSET.Contains(scanner.CurrentChar))
                        scanner.Increment();

                    if (scanner.CurrentIndex > scanner.GetIndex(token))
                    {
                        identifier = scanner.GetString(token);
                        return true;
                    }
                }

                scanner.RestoreIndex(token);
                identifier = null;
                return false;
            }
        }

        public static bool ReadDataStringSeparator(StringScanner scanner)
        {
            if (scanner.CurrentChar == GlobalConsts.SEPARATOR_CHARACTER)
            {
                scanner.Increment();

                ReadDataStringWhitespace(scanner);

                return true;
            }

            return false;
        }

        public static bool ReadDataStringWhitespace(StringScanner scanner)
        {
            int initialIndex = scanner.CurrentIndex;

            while (GlobalConsts.WHITESPACE_CHARACTERS.Contains(scanner.CurrentChar))
                scanner.Increment();

            return scanner.CurrentIndex - initialIndex > 0;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class MainContextAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class AnyFunctionAttribute : Attribute
    {
        public string[] FunctionNames;

        public AnyFunctionAttribute() { }
        public AnyFunctionAttribute(params string[] functionNames)
        {
            FunctionNames = functionNames;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class FunctionAttribute : Attribute
    {
        public string FunctionName;

        public FunctionAttribute(string functionName)
        {
            FunctionName = functionName;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class FunctionItemAttribute : Attribute
    {
        public Type TargetType;

        public FunctionItemAttribute(Type targetType)
        {
            TargetType = targetType;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class StringLiteralAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class FloatLiteralAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class IntegerLiteralAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class BinaryLiteralAttribute : Attribute
    {
        public DataStringLiteralBinaryRepresentation OriginalRepresentation = DataStringLiteralBinaryRepresentation.Binary;
    }

    public abstract class DataStringItem
    {
        #region String Reading/Writing
        public static DataStringItem Parse(string content)
        {
            StringScanner scanner = new StringScanner(content);
            DataStringItem result;

            if (ReadFromString(scanner, out result))
                return result;

            return null;
        }

        internal static bool ReadFromString(StringScanner scanner, out DataStringItem item)
        {
            return DataStringFunction.ReadFromString(scanner, out item) ||
                DataStringSymbol.ReadFromString(scanner, out item) ||
                DataStringLiteral.ReadFromString(scanner, out item);
        }

        internal abstract void WriteToString(StringWriter writer);
        #endregion

        public DataStringItemCollection Items { get; private set; }
        public DataStringItem Parent { get; private set; }

        internal void DataStringItemAdded(DataStringItem parent)
        {
            Parent = parent;
        }

        public DataStringItem()
        {
            Items = new DataStringItemCollection(this);
        }
        public DataStringItem(IEnumerable<DataStringItem> items)
        {
            Items = new DataStringItemCollection(items, this);
        }
    }

    public class DataStringMainContext : DataStringItem
    {
        #region String Reading/Writing
        public static new DataStringMainContext Parse(string content)
        {
            StringScanner scanner = new StringScanner(content);

            return ReadFromString(scanner);
        }

        internal static DataStringMainContext ReadFromString(StringScanner scanner)
        {
            DataStringItem[] items = DataStringItemCollection.ReadFromString(scanner).ToArray();

            return new DataStringMainContext(items);
        }

        public override string ToString()
        {
            StringWriter writer = new StringWriter("");
            WriteToString(writer);

            return writer.Content;
        }

        internal override void WriteToString(StringWriter writer)
        {
            Items.WriteToString(writer);
        }
        #endregion

        public DataStringMainContext() : base() { }

        public DataStringMainContext(IEnumerable<DataStringItem> items) : base(items) { }
    }

    public abstract class DataStringLiteral : DataStringItem
    {
        #region String Reading/Writing
        internal static new bool ReadFromString(StringScanner scanner, out DataStringItem item)
        {
            return DataStringLiteralString.ReadFromString(scanner, out item) ||
                DataStringLiteralBinary.ReadFromString(scanner, out item) ||
                DataStringLiteralNumber.ReadFromString(scanner, out item);
        }
        #endregion
    }

    public class DataStringLiteralString : DataStringLiteral
    {
        #region String Reading/Writing
        internal static new bool ReadFromString(StringScanner scanner, out DataStringItem item)
        {
            using (var token = scanner.SaveIndex())
            {
                if (scanner.CurrentChar == GlobalConsts.STRING_ENCLOSING_CHAR)
                {
                    while (scanner.CurrentChar != GlobalConsts.STRING_ENCLOSING_CHAR)
                        scanner.Increment();

                    item = new DataStringLiteralString(scanner.GetString(token));
                }

                scanner.RestoreIndex(token);
                item = null;
                return false;
            }
        }

        internal override void WriteToString(StringWriter writer)
        {
            writer.Content += Utils.EncloseString(Value);
        }
        #endregion

        public string Value { get; private set; }

        public DataStringLiteralString(string value) : base()
        {
            Value = value;
        }
    }

    public enum DataStringLiteralBinaryRepresentation
    {
        Binary,
        Octal,
        Hexadecimal
    }

    public class DataStringLiteralBinary : DataStringItem
    {
        #region String Reading/Writing
        internal static new bool ReadFromString(StringScanner scanner, out DataStringItem item)
        {
            using (var token = scanner.SaveIndex())
            {
                if (scanner.CurrentChar == '0')
                {
                    scanner.Increment();

                    bool literalIsBinary = scanner.CurrentChar == 'b',
                        literalIsHexadecimal = scanner.CurrentChar == 'x';

                    if (literalIsBinary || literalIsHexadecimal)
                        scanner.Increment();

                    using (var repStrToken = scanner.SaveIndex())
                    {

                        while (literalIsBinary && GlobalConsts.BINARY_CHARSET.Contains(scanner.CurrentChar) ||
                            literalIsHexadecimal && GlobalConsts.HEXADECIMAL_CHARSET.Contains(scanner.CurrentChar) ||
                            !(literalIsHexadecimal || literalIsHexadecimal) && GlobalConsts.OCTAL_CHARSET.Contains(scanner.CurrentChar))
                            scanner.Increment();

                        string repStr = scanner.GetString(repStrToken);

                        if (repStr.Length > 0)
                        {
                            if (literalIsBinary)
                                item = new DataStringLiteralBinary(BinaryConversion.FromRepresentation(repStr, 2),
                                    DataStringLiteralBinaryRepresentation.Binary);
                            else if (literalIsHexadecimal)
                                item = new DataStringLiteralBinary(BinaryConversion.FromRepresentation(repStr, 16),
                                    DataStringLiteralBinaryRepresentation.Hexadecimal);
                            else
                                item = new DataStringLiteralBinary(BinaryConversion.FromRepresentation(repStr, 8),
                                    DataStringLiteralBinaryRepresentation.Octal);

                            return true;
                        }
                    }
                }

                scanner.RestoreIndex(token);
                item = null;
                return false;
            }
        }

        #region String Writing
        internal override void WriteToString(StringWriter writer)
        {
            int fromBase = OriginalRepresentation == DataStringLiteralBinaryRepresentation.Binary ? 2 :
                OriginalRepresentation == DataStringLiteralBinaryRepresentation.Octal ? 4 :
                OriginalRepresentation == DataStringLiteralBinaryRepresentation.Hexadecimal ? 16 : 0;

            writer.Content += $"{BinaryConversion.ToRepresentation(Value, fromBase)}";
        }
        #endregion
        #endregion

        public BitArray Value { get; private set; }

        public DataStringLiteralBinaryRepresentation OriginalRepresentation { get; private set; }

        public DataStringLiteralBinary(BitArray value, DataStringLiteralBinaryRepresentation originalRepresentation) : base()
        {
            Value = value;
            OriginalRepresentation = originalRepresentation;
        }
    }

    public abstract class DataStringLiteralNumber : DataStringLiteral
    {
        #region String Reading
        internal static new bool ReadFromString(StringScanner scanner, out DataStringItem item)
        {
            string decimalStr;
            bool isFloatingPoint,
                hasExponent;

            if (StringUtils.ReadDecimalString(scanner, out decimalStr, out isFloatingPoint, out hasExponent))
            {
                if (isFloatingPoint)
                    item = new DataStringLiteralFloatingPoint(BigFloat.Parse(decimalStr));
                else
                    item = new DataStringLiteralInteger(BigInteger.Parse(decimalStr));

                return true;
            }
            else
            {
                item = null;
                return false;
            }
        }
        #endregion
    }

    public class DataStringLiteralFloatingPoint : DataStringLiteralNumber
    {
        #region String Writing
        internal override void WriteToString(StringWriter writer)
        {
            writer.Content += Value;
        }
        #endregion

        public BigFloat Value { get; private set; }

        public DataStringLiteralFloatingPoint(BigFloat value) : base()
        {
            Value = value;
        }
    }

    public class DataStringLiteralInteger : DataStringLiteralNumber
    {
        #region String Writing
        internal override void WriteToString(StringWriter writer)
        {
            writer.Content += Value;
        }
        #endregion

        public BigInteger Value { get; private set; }

        public DataStringLiteralInteger(BigInteger value) : base()
        {
            Value = value;
        }
    }

    public class DataStringFunction : DataStringItem
    {
        #region String Reading/Writing
        internal static new bool ReadFromString(StringScanner scanner, out DataStringItem item)
        {
            using (var token = scanner.SaveIndex())
            {
                string functionName;

                if (Utils.ReadIdentifier(scanner, out functionName))
                {
                    if (scanner.CurrentChar == GlobalConsts.FUNC_PARAMS_OPENING_CHAR)
                    {
                        scanner.Increment();

                        item = new DataStringFunction(functionName);
                        item.Items.AddRange(DataStringItemCollection.ReadFromString(scanner).ToList());

                        if (scanner.CurrentChar == GlobalConsts.FUNC_PARAMS_CLOSING_CHAR)
                        {
                            scanner.Increment();
                            return true;
                        }
                    }
                }

                scanner.RestoreIndex(token);
                item = null;
                return false;
            }
        }

        public override string ToString()
        {
            StringWriter writer = new StringWriter("");
            WriteToString(writer);

            return writer.Content;
        }

        internal override void WriteToString(StringWriter writer)
        {
            writer.Content += Name + GlobalConsts.FUNC_PARAMS_OPENING_CHAR;

            Items.WriteToString(writer);

            writer.Content += GlobalConsts.FUNC_PARAMS_CLOSING_CHAR;
        }
        #endregion

        public string Name { get; private set; }

        public DataStringFunction(string name)
        {
            Name = name;
        }
        public DataStringFunction(string name, IEnumerable<DataStringItem> parameters) : base(parameters)
        {
            Name = name;
        }
    }

    public class DataStringSymbol : DataStringItem
    {
        #region String Reading/Writing
        internal static new bool ReadFromString(StringScanner scanner, out DataStringItem item)
        {
            string symbolName;

            if (Utils.ReadIdentifier(scanner, out symbolName))
            {
                item = new DataStringSymbol(symbolName);
                return true;
            }

            item = null;
            return false;
        }

        internal override void WriteToString(StringWriter writer)
        {
            writer.Content += Name;
        }
        #endregion

        public string Name { get; private set; }

        public DataStringSymbol(string name)
        {

        }
    }

    public class DataStringItemCollection : List<DataStringItem>
    {
        #region String Reading/Writing
        internal static IEnumerable<DataStringItem> ReadFromString(StringScanner scanner)
        {
            DataStringItem generatedItem = null;

            while (DataStringItem.ReadFromString(scanner, out generatedItem) || Utils.ReadDataStringSeparator(scanner) ||
                Utils.ReadDataStringWhitespace(scanner))
            {
                if (generatedItem != null)
                    yield return generatedItem;

                generatedItem = null;
            }
        }

        internal void WriteToString(StringWriter writer)
        {
            for (int i = 0, c = Count; i < c; i++)
            {
                DataStringItem item = this[i];
                item.WriteToString(writer);

                if (i < c - 1)
                    writer.Content += GlobalConsts.SEPARATOR_CHARACTER + " ";
            }
        }
        #endregion

        public DataStringItem Parent { get; private set; }

        public new void Add(DataStringItem item)
        {
            item.DataStringItemAdded(Parent);
            base.Add(item);
        }

        public new void AddRange(IEnumerable<DataStringItem> collection)
        {
            foreach (var item in collection)
                item.DataStringItemAdded(Parent);

            base.AddRange(collection);
        }

        public DataStringItemCollection(DataStringItem parent)
        {
            Parent = parent;
        }

        public DataStringItemCollection(IEnumerable<DataStringItem> original, DataStringItem parent)
        {
            Parent = parent;
            AddRange(original);
        }
    }
}