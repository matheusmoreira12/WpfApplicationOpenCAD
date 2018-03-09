using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenCAD.OpenCADFormat.DataConversion;
using System.Numerics;
using OpenCAD.Utils;
using System.Reflection;
using System.Collections;

namespace OpenCAD.OpenCADFormat.DataStrings.Serialization
{
    public class AttributeException : Exception
    {
        public Type[] AttributeTypes
        {
            get
            {
                return (Type[])Data["AttributeTypes"];
            }
            set
            {
                if (AttributeType != null)
                    throw new InvalidOperationException("Cannot set both AttributeType and AttributeTypes properties.");

                Data["AttributeTypes"] = value;
            }
        }

        public Type AttributeType
        {
            get
            {
                return (Type)Data["AttributeType"];
            }
            set
            {
                if (AttributeTypes != null)
                    throw new InvalidOperationException("Cannot set both AttributeType and AttributeTypes properties.");

                Data["AttributeType"] = value;
            }
        }
        public MemberInfo Member
        {
            get
            {
                return (MemberInfo)Data["Member"];
            }
            set
            {
                Data["Member"] = value;
            }
        }

        public AttributeException() { }
        public AttributeException(string message = "") : base(message) { }
    }

    public class AttributeExpectedException : AttributeException
    {
        public AttributeExpectedException() { }
        public AttributeExpectedException(string message = "") : base(message) { }
    }

    public class InvalidAttributeContextException : AttributeException
    {
        public InvalidAttributeContextException() { }
        public InvalidAttributeContextException(string message = "") : base(message) { }
    }


    internal sealed class Encoder
    {
        #region Encoding
        private DataStringItem encodeRoot(object target)
        {
            Type targetType = target.GetType();

            bool targetTypeIsMarkedAsFunction = Attribute.IsDefined(targetType, typeof(FunctionAttribute)),
                targetTypeIsMarkedAsMainContext = Attribute.IsDefined(targetType, typeof(MainContextAttribute));

            if (targetTypeIsMarkedAsFunction)
                return encodeAsFunction(target, target.GetType());
            else if (targetTypeIsMarkedAsMainContext)
                return encodeAsMainContext(target);
            else
                throw new AttributeExpectedException()
                { AttributeTypes = new[] { typeof(FunctionAttribute), typeof(AnyFunctionAttribute), typeof(MainContextAttribute) } };
        }

        private IEnumerable<DataStringItem> encodeAllFields(object target)
        {
            FieldInfo[] targetFields = target.GetType().GetFields();

            foreach (var targetField in targetFields)
            {
                DataStringItem encodedField = encodeField(target, targetField);

                if (encodedField != null)
                    yield return encodedField;
            }
        }

        private DataStringItem encodeField(object target, FieldInfo targetField)
        {
            bool fieldIsMarkedAsBinaryLiteral = Attribute.IsDefined(targetField, typeof(BinaryLiteralAttribute)),
                fieldIsMarkedAsFloatLiteral = Attribute.IsDefined(targetField, typeof(FloatLiteralAttribute)),
                fieldIsMarkedAsIntegerLiteral = Attribute.IsDefined(targetField, typeof(IntegerLiteralAttribute)),
                fieldIsMarkedAsStringLiteral = Attribute.IsDefined(targetField, typeof(StringLiteralAttribute)),
                fieldIsMarkedAsFunction = Attribute.IsDefined(targetField, typeof(FunctionAttribute));

            object fieldValue = targetField.GetValue(target);

            if (fieldIsMarkedAsBinaryLiteral)
            {
                BinaryLiteralAttribute fieldBinaryLiteralAttribute = targetField.GetCustomAttribute<BinaryLiteralAttribute>();

                return encodeAsBinaryLiteral(fieldValue, fieldBinaryLiteralAttribute.OriginalRepresentation);
            }
            else if (fieldIsMarkedAsFloatLiteral)
                return encodeAsFloatLiteral(fieldValue);
            else if (fieldIsMarkedAsIntegerLiteral)
                return encodeAsIntegerLiteral(fieldValue);
            else if (fieldIsMarkedAsStringLiteral)
                return encodeAsStringLiteral(fieldValue);
            else if (fieldIsMarkedAsFunction)
            {
                FunctionItemAttribute fieldFunctionAttribute = targetField.GetCustomAttribute<FunctionItemAttribute>();

                return encodeAsFunction(fieldValue, fieldFunctionAttribute.TargetType);
            }

            return null;
        }

        private DataStringMainContext encodeAsMainContext(object target)
        {
            DataStringItem[] mainContextItems = encodeAllFields(target).ToArray();
            return new DataStringMainContext(mainContextItems);
        }

        private DataStringLiteralBinary encodeAsBinaryLiteral(object value, DataStringLiteralBinaryRepresentation originalRepr)
        {
            BitArray fieldAsBitArray = null;

            if (ObjectConversion.TryConvertTo(value, out fieldAsBitArray))
                return new DataStringLiteralBinary(fieldAsBitArray, originalRepr);

            return null;
        }

        private DataStringLiteralFloatingPoint encodeAsFloatLiteral(object value)
        {
            BigFloat fieldAsBigFloat;

            if (ObjectConversion.TryConvertTo(value, out fieldAsBigFloat))
                return new DataStringLiteralFloatingPoint(fieldAsBigFloat);

            return null;
        }

        private DataStringLiteralInteger encodeAsIntegerLiteral(object value)
        {
            BigInteger? fieldAsBigInteger;

            if (ObjectConversion.TryConvertTo(value, out fieldAsBigInteger))
                return new DataStringLiteralInteger((BigInteger)fieldAsBigInteger);

            return null;
        }

        private DataStringLiteralString encodeAsStringLiteral(object value)
        {
            return new DataStringLiteralString(value.ToString());
        }

        private string resolveFunctionName(Type targetType)
        {
            bool typeIsMarkedAsFunction = Attribute.IsDefined(targetType, typeof(FunctionAttribute));

            if (typeIsMarkedAsFunction)
            {
                FunctionAttribute typeFunctionAttribute = targetType.GetCustomAttribute<FunctionAttribute>();

                return typeFunctionAttribute.FunctionName;
            }
            else
                throw new AttributeExpectedException() { AttributeType = typeof(FunctionAttribute), Member = targetType };
        }

        private DataStringFunction encodeAsFunction(object target, Type targetType)
        {
            if (targetType.IsInstanceOfType(target))
            {
                string functionName = resolveFunctionName(target.GetType());

                DataStringItem[] functionParameters = encodeAllFields(target).ToArray();
                return new DataStringFunction(functionName, functionParameters);
            }

            return null;
        }
        #endregion

        public object Target { get; private set; }

        public DataStringItem Encode()
        {
            return encodeRoot(Target);
        }

        public Encoder(object value)
        {
            Target = value;
        }
    }
}