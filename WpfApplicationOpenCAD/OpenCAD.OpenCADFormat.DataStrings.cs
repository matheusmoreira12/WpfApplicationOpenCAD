using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenCAD.OpenCADFormat.DataStrings
{
    [System.AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class AnyFunction : System.Attribute
    {
        public string[] AllowedNames { get; private set; }

        public AnyFunction(params string[] allowedNames)
        {
            AllowedNames = allowedNames;
        }

        public AnyFunction()
        {
            AllowedNames = null;
        }
    }

    [System.AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class Function : System.Attribute
    {
        public string Name { get; private set; }

        public Function(string name)
        {
            Name = name;
        }

        public Function()
        {

        }
    }

    public class Parameter : System.Attribute
    {
        public int ParamIndex { get; private set; }

        protected Parameter(int paramIndex)
        {
            ParamIndex = paramIndex;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class FunctionParameter : Parameter
    {
        public Type AssociatedType { get; private set; }
        public string Name { get; private set; }

        public FunctionParameter(int paramIndex, string name, Type associatedType) : base(paramIndex)
        {
            Name = name;
            AssociatedType = associatedType;
        }

        public FunctionParameter(int paramIndex, Type associatedType) : base(paramIndex)
        {
            Name = null;
            AssociatedType = associatedType;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class FloatLiteral : Parameter
    {
        public FloatLiteral(int paramIndex) : base(paramIndex) { }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class StringLiteral : Parameter
    {
        public StringLiteral(int paramIndex) : base(paramIndex) { }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class Symbol : Parameter
    {
        public Symbol(int paramIndex) : base(paramIndex) { }
    }

    public class DataStringParameter
    {
        static public bool Test(DataStringParameter instance)
        {
            return instance is DataStringParameter;
        }

        public DataStringParameterCollection Parameters { get; private set; }
        public DataStringParameter Parent { get; private set; }

        public void DataStringParameterAdded(DataStringParameter parent)
        {
            Parent = parent;
        }

        public DataStringParameter()
        {
            Parameters = new DataStringParameterCollection(this);
        }

        public DataStringParameter(IEnumerable<DataStringParameter> originalItems)
        {
            Parameters = new DataStringParameterCollection(originalItems, this);
        }
    }

    public class DataStringMainContext : DataStringParameter
    {
        static public new bool Test(DataStringParameter instance)
        {
            return instance is DataStringMainContext;
        }

        public DataStringMainContext() : base() { }

        public DataStringMainContext(IEnumerable<DataStringParameter> originalItems) : base(originalItems) { }
    }

    public class DataStringLiteral : DataStringParameter
    {
        static public new bool Test(DataStringParameter instance)
        {
            return instance is DataStringLiteral;
        }
    }

    public class DataStringLiteralString : DataStringLiteral
    {
        static public new bool Test(DataStringParameter instance)
        {
            return instance is DataStringLiteralString;
        }

        public string Value { get; private set; }

        public DataStringLiteralString(string value) : base()
        {
            Value = value;
        }
    }

    public class DataStringLiteralHexadecimal : DataStringLiteral
    {
        static public new bool Test(DataStringParameter instance)
        {
            return instance is DataStringLiteralHexadecimal;
        }

        public string Value { get; private set; }

        public DataStringLiteralHexadecimal(string value) : base()
        {
            Value = value;
        }
    }

    public class DataStringLiteralBinary : DataStringLiteral
    {
        static public new bool Test(DataStringParameter instance)
        {
            return instance is DataStringLiteralBinary;
        }

        public byte[] Value { get; private set; }

        public DataStringLiteralBinary(byte[] value) : base()
        {
            Value = value;
        }
    }

    public class DataStringLiteralFloatingPoint : DataStringLiteral
    {
        static public new bool Test(DataStringParameter instance)
        {
            return instance is DataStringLiteralFloatingPoint;
        }

        public DataConversion.ArbitraryFloat Value { get; private set; }

        public DataStringLiteralFloatingPoint(DataConversion.ArbitraryFloat value) : base()
        {
            Value = value;
        }
    }

    public class DataStringLiteralInteger : DataStringLiteral
    {
        static public new bool Test(DataStringParameter instance)
        {
            return instance is DataStringLiteralInteger;
        }

        public BigInteger Value { get; private set; }

        public DataStringLiteralInteger(BigInteger value) : base()
        {
            Value = value;
        }
    }

    public class DataStringFunction : DataStringParameter
    {
        static public new bool Test(DataStringParameter instance)
        {
            return instance is DataStringFunction;
        }

        public string Name { get; private set; }

        public DataStringFunction(string name)
        {
            Name = name;
        }

        public DataStringFunction(string name, IEnumerable<DataStringParameter> param) : base(param)
        {
            Name = name;
        }
    }

    public class DataStringSymbol : DataStringParameter
    {
        static public new bool Test(DataStringParameter instance)
        {
            return instance is DataStringSymbol;
        }

        public string Name { get; private set; }

        public DataStringSymbol(string name)
        {

        }
    }

    public class DataStringParameterCollection : List<DataStringParameter>
    {
        public DataStringParameter Parent { get; private set; }

        public new void Add(DataStringParameter item)
        {
            item.DataStringParameterAdded(Parent);
            base.Add(item);
        }

        public DataStringParameterCollection(DataStringParameter parent)
        {
            Parent = parent;
        }

        public DataStringParameterCollection(IEnumerable<DataStringParameter> original, DataStringParameter parent)
        {
            Parent = parent;
            foreach (var item in original)
                Add(item);
        }
    }
}