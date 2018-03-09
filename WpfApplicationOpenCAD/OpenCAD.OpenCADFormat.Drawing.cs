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
    [DataStrings.Function("Point")]
    public struct DrawingPoint : IXmlSerializable
    {
        [DataStrings.StringLiteral()]
        public Measurement<Length> X;
        [DataStrings.StringLiteral()]
        public Measurement<Length> Y;

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
        }
    }

    [DataStrings.Function("Size")]
    public struct DrawingSize : IXmlSerializable
    {
        [DataStrings.StringLiteral()]
        public Measurement<Length> Width;
        [DataStrings.StringLiteral()]
        public Measurement<Length> Height;

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
        }

        public DrawingSize(Measurement<Length> width, Measurement<Length> height)
        {
            Width = width;
            Height = height;
        }
    }

    /// <summary>
    /// Represents a color value.
    /// </summary>
    [DataStrings.AnyFunction("color-rgb", "color-rgba", "color-hsl", "color-hsv", "color-cmyk")]
    abstract class Color
    {
        public static Color Parse(string text)
        {
            return null;
        }

        public abstract override string ToString();

        public abstract System.Drawing.Color ToColor();
    }

    /// <summary>
    /// Represents a RGB color value.
    /// </summary>
    [DataStrings.Function("ColorRGB")]
    class ColorRGB : Color
    {
        [DataStrings.IntegerLiteral()]
        public int R;
        [DataStrings.IntegerLiteral()]
        public int G;
        [DataStrings.IntegerLiteral()]
        public int B;

        public override System.Drawing.Color ToColor()
        {
            return System.Drawing.Color.FromArgb(R, G, B);
        }

        public override string ToString()
        {
            return null;
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
    [DataStrings.Function("ColorRGBA")]
    class ColorRGBA : Color
    {
        [DataStrings.IntegerLiteral()]
        public int R;
        [DataStrings.IntegerLiteral()]
        public int G;
        [DataStrings.IntegerLiteral()]
        public int B;
        [DataStrings.FloatLiteral()]
        public float A;

        public override System.Drawing.Color ToColor()
        {
            return System.Drawing.Color.FromArgb((int)(A * 255), R, G, B);
        }

        public override string ToString()
        {
            return null;
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
    [DataStrings.Function("ColorHSL")]
    class ColorHSL : Color
    {
        [DataStrings.FloatLiteral()]
        public float H;
        [DataStrings.FloatLiteral()]
        public float S;
        [DataStrings.FloatLiteral()]
        public float L;

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
            return null;
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
    [DataStrings.Function("ColorHSV")]
    class ColorHSV : Color
    {
        [DataStrings.FloatLiteral()]
        public float H;
        [DataStrings.FloatLiteral()]
        public float S;
        [DataStrings.FloatLiteral()]
        public float V;

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
            return null;
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
    [DataStrings.Function("ColorCMYK")]
    class ColorCMYK : Color
    {
        [DataStrings.FloatLiteral()]
        public float C;
        [DataStrings.FloatLiteral()]
        public float M;
        [DataStrings.FloatLiteral()]
        public float Y;
        [DataStrings.FloatLiteral()]
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
            return null;
        }

        public ColorCMYK(float c, float m, float y, float k)
        {
            C = c;
            M = m;
            Y = y;
            K = k;
        }
    }

    [Serializable]
    enum FillRule
    {
        [XmlEnum("EvenOdd")]
        EvenOdd,
        [XmlEnum("NonZero")]
        NonZero
    }

    class FillRuleExtension
    {

    }

    [Serializable]
    [DataStrings.Function("DashArray")]
    struct StrokeDashArray: IXmlSerializable
    {
        [DataStrings.FloatLiteral()]
        public int[] Dashes { get; private set; }

        public StrokeDashArray(int[] dashes)
        {
            Dashes = dashes;
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
        }
    }

    [Serializable]
    public class VisualElement
    {
        [XmlAttribute("FontColor")]
        Color Color;
        [XmlAttribute("Fill")]
        Color Fill;
        [XmlAttribute("FillRule")]
        FillRule FillRule;
        [XmlAttribute("Font")]
        string Font;
        [XmlAttribute("FontFamily")]
        string FontFamily;
        [XmlAttribute("FontHeight")]
        Measurement<Length> FontHeight;
        [XmlAttribute("FontStyle")]
        FontStyle FontStyle;
        //FontVariant FontVariant;
        //FontWheight FontWeight;
        [XmlAttribute("xlink:id", Form = XmlSchemaForm.Qualified)]
        string ID;
        [XmlAttribute("Stroke")]
        Color Stroke;
        //StrokeDashArray StrokeDasharray;
        [XmlAttribute("StrokeDashOffset")]
        Measurement<Length> StrokeDashOffset;
        //StrokeLineCap StrokeLineCap;
        //StrokeLineJoin StrokeLineJoin;
        [XmlAttribute("StrokeWidth")]
        Measurement<Length> StrokeWidth;
        //Transform Transform;
    }

    [Serializable]
    public class Shape : VisualElement
    {

    }

    [Serializable]
    public class Rectangle : Shape
    {
        [XmlAttribute("TopLeft")]
        public DrawingPoint TopLeft;
        [XmlAttribute("BottomRight")]
        public DrawingPoint BottomRight;
    }

    [Serializable]
    public class Circle : Shape
    {
        [XmlAttribute("Center", typeof(DrawingPoint))]
        public DrawingPoint Center;
        [XmlAttribute("Radius")]
        public IMeasurement<Length> Radius;
    }

    [Serializable]
    public class Ellipse : Shape
    {
        [XmlAttribute("Center", typeof(DrawingPoint))]
        public DrawingPoint Center;
        [XmlAttribute("Radius", typeof(DrawingSize))]
        public DrawingPoint Radius;
    }

    [Serializable]
    public class Line : Shape
    {
        [XmlAttribute("Start", typeof(DrawingPoint))]
        public DrawingPoint Start;
        [XmlAttribute("End", typeof(DrawingPoint))]
        public DrawingPoint End;
    }

    [Serializable]
    public class Polygon : Shape
    {
        [XmlElement(typeof(DrawingPoint))]
        public List<DrawingPoint> Points;
    }

    [Serializable]
    public class Polyline : Polygon
    {
    }

    [Serializable]
    public class ArcThreePoint : Shape
    {
        [XmlAttribute("Start", typeof(Point))]
        public DrawingPoint Start;
        [XmlAttribute("Control", typeof(Point))]
        public DrawingPoint Control;
        [XmlAttribute("End", typeof(Point))]
        public DrawingPoint End;
        [XmlAttribute("LargeArcFlag")]
        public bool LargeArcFlag;
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
            }

            return path;
        }


        public class PathSegment
        {

            public IMeasurement<Length> X;

            public IMeasurement<Length> Y;


            public bool Relative = true;
        }


        public class Arc : PathSegment
        {
            Arc()
            {
                new System.Windows.Media.ArcSegment();
            }
        }


        public class ClosePath : PathSegment
        {
        }


        public class CurveCubic : PathSegment
        {

            public IMeasurement<Length> X2;

            public IMeasurement<Length> Y2;
        }

        [Serializable]

        public class CurveQuadratic : PathSegment
        {

            public IMeasurement<Length> X2;

            public IMeasurement<Length> Y2;

            public IMeasurement<Length> X3;

            public IMeasurement<Length> Y3;
        }

        [Serializable]

        public class LineTo : PathSegment { }

        [Serializable]

        public class MoveTo : PathSegment { }
    }

    [Serializable]
    public class Drawing
    {

    }
}
