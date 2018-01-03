using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using OpenCAD.OpenCADFormat.Measures;
using OpenCAD.OpenCADFormat.Measures.Quantities;

namespace OpenCAD.OpenCADFormat.Drawing
{
    [Serializable]
    [DataStrings.Function("point")]
    public struct DrawingPoint : IXmlSerializable
    {
        public Measures.IUnit<Measures.Quantities.Length> Unit;
        [DataStrings.FloatLiteral(0)]
        public float X;
        [DataStrings.FloatLiteral(1)]
        public float Y;

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            X = float.Parse(reader.GetAttribute("X"), CultureInfo.InvariantCulture);
            Y = float.Parse(reader.GetAttribute("Y"), CultureInfo.InvariantCulture);
            Unit = Quantity<Length>.GetUnitBySymbol(reader.GetAttribute("Unit"));
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("X", X.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("Y", Y.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("Unit", Unit.Symbol);
        }

        public DrawingPoint(float x, float y, Measures.Unit<Measures.Quantities.Length> unit)
        {
            X = x;
            Y = y;
            Unit = unit;
        }
    }

    /// <summary>
    /// Represents a color value.
    /// </summary>
    [DataStrings.AnyFunction("rgb", "rgba", "hsl", "hsv", "cmyk")]
    abstract class Color
    {
        static public Color Parse(string text)
        {
            var parser = new DataStrings.DataStringParser(text);
            var mainContext = parser.Parse();

            var func = mainContext.Parameters[0] as DataStrings.DataStringFunction;
            if (func == null)
                throw new InvalidOperationException("Could not parse color string. Invalid color format.");

            switch (func.Name)
            {
                case "rgb":
                    {
                        var litFloatR = func.Parameters[0] as DataStrings.DataStringLiteralInteger;
                        var litFloatG = func.Parameters[1] as DataStrings.DataStringLiteralInteger;
                        var litFloatB = func.Parameters[2] as DataStrings.DataStringLiteralInteger;

                        return new ColorRGB((int)litFloatB.Value, (int)litFloatR.Value, (int)litFloatB.Value);
                    }
                case "rgba":
                    {
                        var litFloatR = func.Parameters[0] as DataStrings.DataStringLiteralInteger;
                        var litFloatG = func.Parameters[1] as DataStrings.DataStringLiteralInteger;
                        var litFloatB = func.Parameters[2] as DataStrings.DataStringLiteralInteger;
                        var litFloatA = func.Parameters[3] as DataStrings.DataStringLiteralInteger;

                        return new ColorRGBA((int)litFloatR.Value, (int)litFloatG.Value, (int)litFloatB.Value, (int)litFloatA.Value);
                    }
                case "hsl":
                    {
                        var litFloatH = func.Parameters[0] as DataStrings.DataStringLiteralFloatingPoint;
                        var litFloatS = func.Parameters[1] as DataStrings.DataStringLiteralFloatingPoint;
                        var litFloatL = func.Parameters[2] as DataStrings.DataStringLiteralFloatingPoint;

                        return new ColorHSL((float)litFloatH.Value, (float)litFloatS.Value, (float)litFloatL.Value);
                    }
                    break;
                case "hsv":
                    {
                        var litFloatH = func.Parameters[0] as DataStrings.DataStringLiteralFloatingPoint;
                        var litFloatS = func.Parameters[1] as DataStrings.DataStringLiteralFloatingPoint;
                        var litFloatV = func.Parameters[2] as DataStrings.DataStringLiteralFloatingPoint;

                        return new ColorHSV((float)litFloatH.Value, (float)litFloatS.Value, (float)litFloatV.Value);
                    }
                    break;
                case "cmyk":
                    {
                        var litFloatC = func.Parameters[0] as DataStrings.DataStringLiteralFloatingPoint;
                        var litFloatM = func.Parameters[1] as DataStrings.DataStringLiteralFloatingPoint;
                        var litFloatY = func.Parameters[2] as DataStrings.DataStringLiteralFloatingPoint;
                        var litFloatK = func.Parameters[3] as DataStrings.DataStringLiteralFloatingPoint;

                        return new ColorCMYK((float)litFloatC.Value, (float)litFloatM.Value, (float)litFloatY.Value, (float)litFloatY.Value);
                    }
                default:
                    throw new InvalidOperationException("Could not parse color string. The specified color space is invalid.");
            }
        }

        public abstract string ToString();

        public abstract System.Drawing.Color ToColor();
    }

    /// <summary>
    /// Represents a RGB color value.
    /// </summary>
    [DataStrings.Function("rgb")]
    class ColorRGB : Color
    {
        [DataStrings.FloatLiteral(0)]
        public int R;
        [DataStrings.FloatLiteral(1)]
        public int G;
        [DataStrings.FloatLiteral(2)]
        public int B;

        public override System.Drawing.Color ToColor()
        {
            return System.Drawing.Color.FromArgb(R, G, B);
        }

        public override string ToString()
        {
            var mainContext = new DataStrings.DataStringMainContext();

            var func = new DataStrings.DataStringFunction("rgb");
            mainContext.Parameters.Add(func);

            var litFloatR = new DataStrings.DataStringLiteralFloatingPoint((DataConversion.ArbitraryFloat)R);
            func.Parameters.Add(litFloatR);

            var litFloatG = new DataStrings.DataStringLiteralFloatingPoint((DataConversion.ArbitraryFloat)G);
            func.Parameters.Add(litFloatG);

            var litFloatB = new DataStrings.DataStringLiteralFloatingPoint((DataConversion.ArbitraryFloat)B);
            func.Parameters.Add(litFloatB);

            var generator = new DataStrings.DataStringGenerator(mainContext);

            return generator.Generate();
        }

        public ColorRGB(int r, int g, int b)
        {
            R = r;
            G = g;
            B = b;
        }
    }

    /// <summary>
    /// Represents a RGB color value with alpha channel.
    /// </summary>
    [DataStrings.Function("rgba")]
    class ColorRGBA : Color
    {
        [DataStrings.FloatLiteral(0)]
        public int R;
        [DataStrings.FloatLiteral(1)]
        public int G;
        [DataStrings.FloatLiteral(2)]
        public int B;
        [DataStrings.FloatLiteral(3)]
        public float A;

        public override System.Drawing.Color ToColor()
        {
            return System.Drawing.Color.FromArgb((int)(A * 255), R, G, B);
        }

        public override string ToString()
        {
            var mainContext = new DataStrings.DataStringMainContext();

            var func = new DataStrings.DataStringFunction("rgba");
            mainContext.Parameters.Add(func);

            var litFloatR = new DataStrings.DataStringLiteralInteger((BigInteger)R);
            func.Parameters.Add(litFloatR);

            var litFloatG = new DataStrings.DataStringLiteralInteger((BigInteger)G);
            func.Parameters.Add(litFloatG);

            var litFloatB = new DataStrings.DataStringLiteralInteger((BigInteger)B);
            func.Parameters.Add(litFloatB);

            var litFloatA = new DataStrings.DataStringLiteralFloatingPoint((DataConversion.ArbitraryFloat)A);
            func.Parameters.Add(litFloatA);

            var generator = new DataStrings.DataStringGenerator(mainContext);

            return generator.Generate();
        }

        public ColorRGBA(int r, int g, int b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = b;
        }
    }

    /// <summary>
    /// Represents an HSL color value.
    /// </summary>
    [DataStrings.Function("hsl")]
    class ColorHSL : Color
    {
        [DataStrings.FloatLiteral(0)]
        static float H;
        [DataStrings.FloatLiteral(1)]
        static float S;
        [DataStrings.FloatLiteral(2)]
        static float L;

        public override System.Drawing.Color ToColor()
        {
            float c = (1.0f - Math.Abs(2.0f * L - 1.0f)) * S,
                x = c * (1.0f - Math.Abs((H / 60.0f) % 2.0f - 1.0f)),
                m = L - c / 2;

            //all possibilities for R, G and B factors relative to hue rotation
            var f = new float[][] { new float[] { c, x, 0 }, new float[] { x, c, 0 }, new float[] { 0, c, x },
                new float[] { 0, x, c }, new float[] { x, 0, c }, new float[] { c, 0, x } };

            //select R, G and B factors based on hue rotation
            var _f = f[((int)H % 360) / 60];

            //combine the factors with the luminance and get rgb values
            float r = _f[0] * L,
                g = _f[1] * L,
                b = _f[2] * L;

            return System.Drawing.Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        public override string ToString()
        {
            var mainContext = new DataStrings.DataStringMainContext();

            var func = new DataStrings.DataStringFunction("hsl");
            mainContext.Parameters.Add(func);

            var litFloatH = new DataStrings.DataStringLiteralFloatingPoint((DataConversion.ArbitraryFloat)H);
            func.Parameters.Add(litFloatH);

            var litFloatS = new DataStrings.DataStringLiteralFloatingPoint((DataConversion.ArbitraryFloat)S);
            func.Parameters.Add(litFloatS);

            var litFloatL = new DataStrings.DataStringLiteralFloatingPoint((DataConversion.ArbitraryFloat)L);
            func.Parameters.Add(litFloatL);

            var generator = new DataStrings.DataStringGenerator(mainContext);

            return generator.Generate();
        }

        public ColorHSL(float h, float s, float l)
        {
            H = h;
            S = s;
            L = l;
        }
    }

    /// <summary>
    /// Represents an HSV color value.
    /// </summary>
    [DataStrings.Function("hsv")]
    class ColorHSV : Color
    {
        [DataStrings.FloatLiteral(0)]
        static float H;
        [DataStrings.FloatLiteral(1)]
        static float S;
        [DataStrings.FloatLiteral(2)]
        static float V;

        public override System.Drawing.Color ToColor()
        {
            float c = V * S,
                x = c * (1.0f - Math.Abs((H / 60.0f) % 2.0f - 1.0f)),
                m = V - c;

            //all possibilities for R, G and B factors relative to hue rotation
            var f = new float[][] { new float[] { c, x, 0 }, new float[] { x, c, 0 }, new float[] { 0, c, x },
                new float[] { 0, x, c }, new float[] { x, 0, c }, new float[] { c, 0, x } };

            //select R, G and B factors based on hue rotation
            var _f = f[((int)H % 360) / 60];

            //combine the factors with the luminance and get rgb values
            float r = _f[0] + m,
                g = _f[1] + m,
                b = _f[2] + m;

            return System.Drawing.Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        public override string ToString()
        {
            var mainContext = new DataStrings.DataStringMainContext();

            var func = new DataStrings.DataStringFunction("hsv");
            mainContext.Parameters.Add(func);

            var litFloatH = new DataStrings.DataStringLiteralFloatingPoint((DataConversion.ArbitraryFloat)H);
            func.Parameters.Add(litFloatH);

            var litFloatS = new DataStrings.DataStringLiteralFloatingPoint((DataConversion.ArbitraryFloat)S);
            func.Parameters.Add(litFloatS);

            var litFloatV = new DataStrings.DataStringLiteralFloatingPoint((DataConversion.ArbitraryFloat)V);
            func.Parameters.Add(litFloatV);

            var generator = new DataStrings.DataStringGenerator(mainContext);

            return generator.Generate();
        }

        public ColorHSV(float h, float s, float v)
        {
            H = h;
            S = s;
            V = v;
        }
    }

    /// <summary>
    /// Represents a CMYK color value;
    /// </summary>
    [DataStrings.Function("cmyk")]
    class ColorCMYK : Color
    {
        [DataStrings.FloatLiteral(0)]
        public float C;
        [DataStrings.FloatLiteral(1)]
        public float M;
        [DataStrings.FloatLiteral(2)]
        public float Y;
        [DataStrings.FloatLiteral(3)]
        public float K;

        public override System.Drawing.Color ToColor()
        {
            byte r = (byte)(255 * (1 - C) * (1 - K)),
                g = (byte)(255 * (1 - M) * (1 - K)),
                b = (byte)(255 * (1 - Y) * (1 - K));

            return System.Drawing.Color.FromArgb(r, g, b);
        }

        public override string ToString()
        {
            var mainContext = new DataStrings.DataStringMainContext();

            var func = new DataStrings.DataStringFunction("cmyk");
            mainContext.Parameters.Add(func);

            var litFloatC = new DataStrings.DataStringLiteralFloatingPoint((DataConversion.ArbitraryFloat)C);
            func.Parameters.Add(litFloatC);

            var litFloatM = new DataStrings.DataStringLiteralFloatingPoint((DataConversion.ArbitraryFloat)M);
            func.Parameters.Add(litFloatM);

            var litFloatY = new DataStrings.DataStringLiteralFloatingPoint((DataConversion.ArbitraryFloat)Y);
            func.Parameters.Add(litFloatY);

            var litFloatK = new DataStrings.DataStringLiteralFloatingPoint((DataConversion.ArbitraryFloat)K);
            func.Parameters.Add(litFloatK);

            var generator = new DataStrings.DataStringGenerator(mainContext);

            return generator.Generate();
        }

        public ColorCMYK(float c, float m, float y, float k)
        {
            C = c;
            M = m;
            Y = y;
            K = k;
        }
    }

    enum FillRule
    {
        EvenOdd,
        NonZero
    }

    class FillRuleExtension
    {

    }

    [Serializable]
    public class VisualElement
    {
        Color Color;
        Color Fill;
        FillRule FillRule;
        string Font;
        string FontFamily;
        Measures.Measurement<Measures.Quantities.Length> FontSize;
        FontStyle FontStyle;
        //FontVariant FontVariant;
        //FontWheight FontWeight;
        string ID;
        Color Stroke;
        //StrokeDashArray StrokeDasharray;
        Measures.Measurement<Measures.Quantities.Length> StrokeDashOffset;
        //StrokeLineCap StrokeLineCap;
        //StrokeLineJoin StrokeLineJoin;
        Measures.Measurement<Measures.Quantities.Length> StrokeWidth;
        //Transform Transform;
    }

    [Serializable]
    public class Shape : VisualElement
    {

    }

    [Serializable]
    public class Circle : Shape
    {

    }

    [Serializable]
    public class Ellipse : Shape
    {

    }

    [Serializable]
    public class Line : Shape, IXmlSerializable
    {
        public float X1 = 0;
        public float Y1 = 0;
        public float X2 = 0;
        public float Y2 = 0;
        public Measures.IUnit<Measures.Quantities.Length> Unit;

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            X1 = float.Parse(reader.GetAttribute("X1"), CultureInfo.InvariantCulture);
            Y1 = float.Parse(reader.GetAttribute("Y1"), CultureInfo.InvariantCulture);
            X2 = float.Parse(reader.GetAttribute("X2"), CultureInfo.InvariantCulture);
            Y2 = float.Parse(reader.GetAttribute("Y2"), CultureInfo.InvariantCulture);
            Unit = Quantity<Length>.GetUnitBySymbol(reader.GetAttribute("Unit"));
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("X1", X1.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("Y1", Y1.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("X2", X2.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("Y2", Y2.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("Unit", Unit.Symbol);
        }
    }

    [Serializable]
    public class Path : Shape
    {
        List<PathSegment> Segments = new List<PathSegment> { };

        public System.Drawing.Drawing2D.GraphicsPath ToGraphicsPath()
        {
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

            foreach (var segment in Segments)
            {
                if (segment is Arc)
                {
                    Arc arc = segment as Arc;
                    path.AddArc((float)(arc.X - arc.RadiusX), (float)(arc.Y - arc.RadiusY), (float)(arc.RadiusX * 2),
                        (float)(arc.RadiusY * 2), (float)(arc.Angle), (float)(arc.Sweep));
                }
            }

            return path;
        }

        [Serializable]
        public class PathSegment : IXmlSerializable
        {
            public float X;
            public float Y;
            public bool Relative;
            public Measures.IUnit<Measures.Quantities.Length> Unit;

            XmlSchema IXmlSerializable.GetSchema()
            {
                return null;
            }

            public virtual void ReadXml(XmlReader reader)
            {
                X = float.Parse(reader.GetAttribute("X"), CultureInfo.InvariantCulture);
                Y = float.Parse(reader.GetAttribute("Y"), CultureInfo.InvariantCulture);
                Relative = bool.Parse(reader.GetAttribute("Relative"));
                Unit = Quantity<Length>.GetUnitBySymbol(reader.GetAttribute("Unit"));
            }

            public virtual void WriteXml(XmlWriter writer)
            {
                writer.WriteAttributeString("X", X.ToString(CultureInfo.InvariantCulture));
                writer.WriteAttributeString("Y", Y.ToString(CultureInfo.InvariantCulture));
                writer.WriteAttributeString("Relative", Relative.ToString());
                writer.WriteAttributeString("Unit", Unit.Symbol);
            }

            public PathSegment() { }
            public PathSegment(float x, float y, bool relative, Measures.IUnit<Measures.Quantities.Length> unit)
            {
                X = x;
                Y = y;
                Relative = relative;
                Unit = unit;
            }
        }

        [Serializable]
        public class Arc : PathSegment
        {
            public Measures.IUnit<Measures.Quantities.PlaneAngle> AngleUnit;
            public float Angle;
            public float Sweep;
            public float RadiusX;
            public float RadiusY;

            public override void ReadXml(XmlReader reader)
            {
                AngleUnit = Measures.Quantities.PlaneAngle.SupportedUnits.FindBySymbol(reader.GetAttribute("AngleUnit"));
                Angle = float.Parse(reader.GetAttribute("A"), CultureInfo.InvariantCulture);
                Sweep = float.Parse(reader.GetAttribute("S"), CultureInfo.InvariantCulture);
                RadiusX = float.Parse(reader.GetAttribute("RX"), CultureInfo.InvariantCulture);
                RadiusY = float.Parse(reader.GetAttribute("RY"), CultureInfo.InvariantCulture);

                base.ReadXml(reader);
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteAttributeString("AngleUnit", AngleUnit.Symbol);
                writer.WriteAttributeString("A", Angle.ToString(CultureInfo.InvariantCulture));
                writer.WriteAttributeString("S", Sweep.ToString(CultureInfo.InvariantCulture));
                writer.WriteAttributeString("RX", RadiusX.ToString(CultureInfo.InvariantCulture));
                writer.WriteAttributeString("RY", RadiusY.ToString(CultureInfo.InvariantCulture));

                base.WriteXml(writer);
            }
        }

        [Serializable]
        public class Curve : PathSegment
        {
            public float X2;
            public float Y2;

            public override void ReadXml(XmlReader reader)
            {
                X2 = float.Parse(reader.GetAttribute("X2"), CultureInfo.InvariantCulture);
                Y2 = float.Parse(reader.GetAttribute("Y2"), CultureInfo.InvariantCulture);

                base.ReadXml(reader);
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteAttributeString("X2", X2.ToString(CultureInfo.InvariantCulture));
                writer.WriteAttributeString("Y2", Y2.ToString(CultureInfo.InvariantCulture));

                base.WriteXml(writer);
            }
        }

        [Serializable]
        public class LineTo : PathSegment
        {
        }

        [Serializable]
        public class MoveTo : PathSegment
        {
        }

        [Serializable]
        public class QuadraticCurve : PathSegment
        {
            public float X2;
            public float Y2;
            public float X3;
            public float Y3;

            public override void ReadXml(XmlReader reader)
            {
                X2 = float.Parse(reader.GetAttribute("X2"), CultureInfo.InvariantCulture);
                Y2 = float.Parse(reader.GetAttribute("Y2"), CultureInfo.InvariantCulture);
                X3 = float.Parse(reader.GetAttribute("X3"), CultureInfo.InvariantCulture);
                Y3 = float.Parse(reader.GetAttribute("Y3"), CultureInfo.InvariantCulture);

                base.ReadXml(reader);
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteAttributeString("X2", X2.ToString(CultureInfo.InvariantCulture));
                writer.WriteAttributeString("Y2", Y2.ToString(CultureInfo.InvariantCulture));
                writer.WriteAttributeString("X3", X3.ToString(CultureInfo.InvariantCulture));
                writer.WriteAttributeString("Y3", Y3.ToString(CultureInfo.InvariantCulture));

                base.WriteXml(writer);
            }
        }
    }

    [Serializable]
    public class Polygon : Shape
    {
        [XmlIgnore]
        List<DrawingPoint> Points;
    }

    [Serializable]
    public class Polyline : Polygon
    {

    }

    [Serializable]
    public class Rectangle : Shape
    {

    }

    [Serializable]
    public class Drawing
    {

    }
}
