using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Globalization;
using System.Collections;

namespace OpenCAD.DrawingSerialization
{
    public class AttributeCollection : List<KeyValuePair<string, string>>
    {
        public static AttributeCollection Parse(string input)
        {
            var result = new AttributeCollection { };

            if (input == null || input == "")
                return result;

            string[] keyValuesStr = input.Split(';');

            foreach (string keyValueStr in keyValuesStr)
            {
                string[] keyAndValueStr = keyValueStr.Split('=');

                if (keyAndValueStr.Length != 2)
                    throw new InvalidOperationException("Cannot parse attribute because the string does not represent a valid key-value pair.");

                result.Add(new KeyValuePair<string, string>(keyAndValueStr[0], keyAndValueStr[1]));
            }

            return result;
        }

        public string this[string key]
        {
            get
            {
                foreach (var keyValuePair in this)
                    if (keyValuePair.Key == key)
                        return keyValuePair.Value;

                return null;
            }
            set
            {
                for (int i = 0; i < Count; i++)
                    if (this[i].Key == key)
                        this[i] = new KeyValuePair<string, string>(key, value);
            }
        }

        public new string ToString()
        {
            string[] keyValuesStr = new string[Count];

            for (int i = 0; i < Count; i++)
            {
                var keyValuePair = this[i];
                keyValuesStr[i] = keyValuePair.Key + "=" + keyValuePair.Value;
            }

            return string.Join(";", keyValuesStr);
        }
    }

    public class SvgDrawingSerializable : IXmlSerializable
    {
        Svg.SvgDocument Target;

        static protected Svg.SvgUnitConverter UnitConverter = new Svg.SvgUnitConverter();
        static protected Svg.SvgColourConverter ColourConverter = new Svg.SvgColourConverter();
        static protected Svg.SvgFillRuleConverter FillRuleConverter = new Svg.SvgFillRuleConverter();
        static protected Svg.SvgFontStyleConverter FontStyleConverter = new Svg.SvgFontStyleConverter();
        static protected Svg.SvgFontVariantConverter FontVariantConverter = new Svg.SvgFontVariantConverter();
        static protected Svg.SvgFontWeightConverter FontWeightConverter = new Svg.SvgFontWeightConverter();
        static protected Svg.SvgStrokeLineCapConverter StrokeLineCapConverter = new Svg.SvgStrokeLineCapConverter();
        static protected Svg.SvgStrokeLineJoinConverter StrokeLineJoinConverter = new Svg.SvgStrokeLineJoinConverter();
        static protected Svg.Transforms.SvgTransformConverter TransformConverter = new Svg.Transforms.SvgTransformConverter();


        private const string NAMESPACE = "http://www.opencad.org/2017/ocxdraw";

        #region Svg Writing
        private void writeStartElement(XmlWriter writer, string name)
        {
            writer.WriteStartElement(name, NAMESPACE);
        }

        private Svg.SvgPoint[] polygonToPointArray(Svg.SvgPolygon polygon)
        {
            var polypoints = polygon.Points;
            var result = new Svg.SvgPoint[polygon.Points.Count / 2];

            for (int i = 0; i < polypoints.Count; i += 2)
                result[i / 2] = new Svg.SvgPoint(polypoints[i], polypoints[i + 1]);

            return result;
        }

        private void writeSvgPolygon(XmlWriter writer, Svg.SvgPolygon polygon, string tagName)
        {
            writeStartElement(writer, tagName);

            writeAllAttributes(writer, polygon);

            var points = polygonToPointArray(polygon);
            foreach (var point in points)
            {
                writeStartElement(writer, "Point");
                writer.WriteAttributeString("X", UnitConverter.ConvertToInvariantString(point.X));
                writer.WriteAttributeString("Y", UnitConverter.ConvertToInvariantString(point.X));
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private string pointfToString(System.Drawing.PointF point)
        {
            if (point == null)
                return null;

            return string.Join(";", point.X.ToString(CultureInfo.InvariantCulture),
                point.Y.ToString(CultureInfo.InvariantCulture));
        }

        private string floatToString(float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private void writeSvgPath(XmlWriter writer, Svg.SvgPath path)
        {
            writeStartElement(writer, "Path");

            writeAllAttributes(writer, path);

            foreach (var segment in path.PathData)
            {
                if (segment is Svg.Pathing.SvgArcSegment)
                {
                    var arc = segment as Svg.Pathing.SvgArcSegment;
                    writeStartElement(writer, "Arc");
                    writer.WriteAttributeString("Angle", arc.Angle.ToString());
                    writer.WriteAttributeString("Sweep", arc.Sweep.ToString());
                    writer.WriteAttributeString("RX", floatToString(arc.RadiusX));
                    writer.WriteAttributeString("RY", floatToString(arc.RadiusY));
                }
                else if (segment is Svg.Pathing.SvgClosePathSegment)
                {
                    var closepath = segment as Svg.Pathing.SvgClosePathSegment;
                    writeStartElement(writer, "ClosePath");
                }
                else if (segment is Svg.Pathing.SvgCubicCurveSegment)
                {
                    var curve = segment as Svg.Pathing.SvgCubicCurveSegment;
                    writeStartElement(writer, "Curve");
                    writer.WriteAttributeString("Control1", pointfToString(curve.FirstControlPoint));
                    writer.WriteAttributeString("Control2", pointfToString(curve.SecondControlPoint));
                }
                else if (segment is Svg.Pathing.SvgLineSegment)
                {
                    var line = segment as Svg.Pathing.SvgLineSegment;
                    writeStartElement(writer, "LineTo");
                }
                else if (segment is Svg.Pathing.SvgMoveToSegment)
                {
                    var moveto = segment as Svg.Pathing.SvgMoveToSegment;
                    writeStartElement(writer, "MoveTo");
                }
                else if (segment is Svg.Pathing.SvgQuadraticCurveSegment)
                {
                    var quadcurve = segment as Svg.Pathing.SvgQuadraticCurveSegment;
                    writeStartElement(writer, "QuadraticCurve");
                    writer.WriteAttributeString("Control", pointfToString(quadcurve.ControlPoint));
                }

                writer.WriteAttributeString("Start", pointfToString(segment.Start));
                writer.WriteAttributeString("End", pointfToString(segment.End));

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private void writeSvgLine(XmlWriter writer, Svg.SvgLine line)
        {
            writeStartElement(writer, "Line");

            writeAllAttributes(writer, line);

            writer.WriteAttributeString("X1", UnitConverter.ConvertToInvariantString(line.StartX));
            writer.WriteAttributeString("Y1", UnitConverter.ConvertToInvariantString(line.StartY));
            writer.WriteAttributeString("X2", UnitConverter.ConvertToInvariantString(line.EndX));
            writer.WriteAttributeString("Y2", UnitConverter.ConvertToInvariantString(line.EndY));

            writer.WriteEndElement();
        }

        private void writeSvgEllipse(XmlWriter writer, Svg.SvgEllipse ellipse)
        {
            writeStartElement(writer, "Ellipse");

            writeAllAttributes(writer, ellipse);

            writer.WriteAttributeString("CX", UnitConverter.ConvertToInvariantString(ellipse.CenterX));
            writer.WriteAttributeString("CY", UnitConverter.ConvertToInvariantString(ellipse.CenterY));
            writer.WriteAttributeString("RX", UnitConverter.ConvertToInvariantString(ellipse.RadiusX));
            writer.WriteAttributeString("RY", UnitConverter.ConvertToInvariantString(ellipse.RadiusY));

            writer.WriteEndElement();
        }

        private void writeSvgCircle(XmlWriter writer, Svg.SvgCircle circle)
        {
            writeStartElement(writer, "Circle");

            writeAllAttributes(writer, circle);

            writer.WriteAttributeString("CX", UnitConverter.ConvertToInvariantString(circle.CenterX));
            writer.WriteAttributeString("CY", UnitConverter.ConvertToInvariantString(circle.CenterY));
            writer.WriteAttributeString("Radius", UnitConverter.ConvertToInvariantString(circle.Radius));

            writer.WriteEndElement();
        }

        private void writeSvgRectangle(XmlWriter writer, Svg.SvgRectangle rect)
        {
            writeStartElement(writer, "Rectangle");

            writeAllAttributes(writer, rect);

            writer.WriteAttributeString("X", UnitConverter.ConvertToInvariantString(rect.X));
            writer.WriteAttributeString("Y", UnitConverter.ConvertToInvariantString(rect.Y));
            writer.WriteAttributeString("Width", UnitConverter.ConvertToInvariantString(rect.Width));
            writer.WriteAttributeString("Height", UnitConverter.ConvertToInvariantString(rect.Height));

            writer.WriteEndElement();
        }

        private void writeSvgGroup(XmlWriter writer, Svg.SvgGroup group)
        {
            writeStartElement(writer, "Group");

            writeAllAttributes(writer, group);

            writeAllChildren(writer, group);

            writer.WriteEndElement();
        }

        private void writeSvgClipPath(XmlWriter writer, Svg.SvgClipPath clipPath)
        {
            writeStartElement(writer, "ClipPath");

            writeAllChildren(writer, clipPath);
            writer.WriteAttributeString("ClipPathUnits", new Svg.SvgCoordinateUnitsConverter().ConvertToInvariantString(clipPath.ClipPathUnits));

            writer.WriteEndElement();
        }

        private void writeSvgGlyph(XmlWriter writer, Svg.SvgGlyph glyph, string tagName)
        {
            writeStartElement(writer, tagName);

            writeAllChildren(writer, glyph);
            writer.WriteAttributeString("Unicode", glyph.Unicode);
            writer.WriteAttributeString("VertAdvY", floatToString(glyph.VertAdvY));
            writer.WriteAttributeString("HorizAdvX", floatToString(glyph.HorizAdvX));
            writer.WriteAttributeString("VertOriginX", floatToString(glyph.VertOriginX));
            writer.WriteAttributeString("VertOriginY", floatToString(glyph.VertOriginY));

            writeAllChildren(writer, glyph);

            writer.WriteEndElement();
        }

        private void writeSvgKern(XmlWriter writer, Svg.SvgKern kern, string tagName)
        {
            writeStartElement(writer, tagName);

            writeAllChildren(writer, kern);
            writer.WriteAttributeString("Glyph1", kern.Glyph1);
            writer.WriteAttributeString("Glyph2", kern.Glyph2);
            writer.WriteAttributeString("Kerning", floatToString(kern.Kerning));
            writer.WriteAttributeString("Unicode1", kern.Unicode1);
            writer.WriteAttributeString("Unicode2", kern.Unicode2);

            writer.WriteEndElement();
        }

        private void writeSvgFontFace(XmlWriter writer, Svg.SvgFontFace fontface)
        {
            writeStartElement(writer, "FontFace");

            writer.WriteAttributeString("Alphabetic", floatToString(fontface.Alphabetic));
            writer.WriteAttributeString("Ascent", floatToString(fontface.Ascent));
            writer.WriteAttributeString("AscentHeight", floatToString(fontface.AscentHeight));
            writer.WriteAttributeString("Descent", floatToString(fontface.Descent));
            writer.WriteAttributeString("XHeight", floatToString(fontface.XHeight));
            writer.WriteAttributeString("UnitsPerEm", floatToString(fontface.UnitsPerEm));

            writer.WriteEndElement();
        }

        private void writeSvgFont(XmlWriter writer, Svg.SvgFont font)
        {
            writeStartElement(writer, "Font");

            writer.WriteAttributeString("VertAdvY", floatToString(font.VertAdvY));
            writer.WriteAttributeString("HorizAdvX", floatToString(font.HorizAdvX));
            writer.WriteAttributeString("VertOriginX", floatToString(font.VertOriginX));
            writer.WriteAttributeString("VertOriginY", floatToString(font.VertOriginY));

            foreach (var child in font.Children)
            {
                if (child is Svg.SvgVerticalKern)
                    writeSvgKern(writer, child as Svg.SvgKern, "VerticalKern");
                else if (child is Svg.SvgHorizontalKern)
                    writeSvgKern(writer, child as Svg.SvgKern, "HorizontalKern");
                else if (child is Svg.SvgGlyph)
                    writeSvgGlyph(writer, child as Svg.SvgGlyph, "Glyph");
                else if (child is Svg.SvgMissingGlyph)
                    writeSvgGlyph(writer, child as Svg.SvgGlyph, "MissingGlyph");
                else if (child is Svg.SvgFontFace)
                    writeSvgFontFace(writer, child as Svg.SvgFontFace);
            }

            writer.WriteEndElement();
        }

        private string colorToString(Svg.SvgPaintServer color)
        {
            if (color == null)
                return null;

            return ColourConverter.ConvertToInvariantString(((Svg.SvgColourServer)color).Colour);
        }

        private string strokeDashArrayToString(Svg.SvgUnitCollection dashArray)
        {
            if (dashArray == null)
                return null;

            return string.Join(";", dashArray.Select(u => u.Value.ToString(CultureInfo.InvariantCulture)));
        }

        private void writeAllAttributes(XmlWriter writer, Svg.SvgElement element)
        {
            writer.WriteAttributeString("Color", colorToString(element.Color));
            writer.WriteAttributeString("Fill", colorToString(element.Fill));
            writer.WriteAttributeString("FillRule", FillRuleConverter.ConvertToInvariantString(element.FillRule));
            writer.WriteAttributeString("Font", element.Font);
            writer.WriteAttributeString("FontFamily", element.FontFamily);
            writer.WriteAttributeString("FontSize", UnitConverter.ConvertToInvariantString(element.FontSize));
            writer.WriteAttributeString("FontStyle", FontStyleConverter.ConvertToInvariantString(element.FontStyle));
            writer.WriteAttributeString("FontVariant", FontVariantConverter.ConvertToInvariantString(element.FontVariant));
            writer.WriteAttributeString("FontWeight", FontWeightConverter.ConvertToInvariantString(element.FontWeight));
            writer.WriteAttributeString("ID", element.ID);
            writer.WriteAttributeString("Stroke", colorToString(element.Stroke));
            writer.WriteAttributeString("StrokeDasharray", strokeDashArrayToString(element.StrokeDashArray));
            writer.WriteAttributeString("StrokeDashOffset", UnitConverter.ConvertToInvariantString(element.StrokeDashOffset));
            writer.WriteAttributeString("StrokeLinecap", StrokeLineCapConverter.ConvertToInvariantString(element.StrokeLineCap));
            writer.WriteAttributeString("StrokeLinejoin", StrokeLineJoinConverter.ConvertToInvariantString(element.StrokeLineJoin));
            writer.WriteAttributeString("StrokeWidth", UnitConverter.ConvertToInvariantString(element.StrokeWidth));
            writer.WriteAttributeString("Transform", TransformConverter.ConvertToInvariantString(element.Transforms));
        }

        private string viewboxToString(Svg.SvgViewBox viewbox)
        {
            if (viewbox == null)
                return null;

            return string.Join(";", floatToString(viewbox.MinX), floatToString(viewbox.MinY),
                floatToString(viewbox.Width), floatToString(viewbox.Height));
        }

        private void writeSvgDocument(XmlWriter writer, Svg.SvgDocument document)
        {
            writeStartElement(writer, "Drawing");

            writer.WriteAttributeString("ViewBox", viewboxToString(document.ViewBox));
            writer.WriteAttributeString("Width", UnitConverter.ConvertToInvariantString(document.Width));
            writer.WriteAttributeString("Height", UnitConverter.ConvertToInvariantString(document.Height));

            writeAllChildren(writer, document);

            writer.WriteEndElement();
        }

        private void writeAllChildren(XmlWriter writer, Svg.SvgElement element)
        {
            foreach (var child in element.Children)
            {
                if (child is Svg.SvgRectangle)
                    writeSvgRectangle(writer, child as Svg.SvgRectangle);
                else if (child is Svg.SvgCircle)
                    writeSvgCircle(writer, child as Svg.SvgCircle);
                else if (child is Svg.SvgEllipse)
                    writeSvgEllipse(writer, child as Svg.SvgEllipse);
                else if (child is Svg.SvgLine)
                    writeSvgLine(writer, child as Svg.SvgLine);
                else if (child is Svg.SvgPolyline)
                    writeSvgPolygon(writer, child as Svg.SvgPolyline, "Polyline");
                else if (child is Svg.SvgPolygon)
                    writeSvgPolygon(writer, child as Svg.SvgPolygon, "Polygon");
                else if (child is Svg.SvgPath)
                    writeSvgPath(writer, child as Svg.SvgPath);
                else if (child is Svg.SvgClipPath)
                    writeSvgClipPath(writer, child as Svg.SvgClipPath);
                else if (child is Svg.SvgFont)
                    writeSvgFont(writer, child as Svg.SvgFont);
                else if (child is Svg.SvgGroup)
                    writeSvgGroup(writer, child as Svg.SvgGroup);
            }

        }
        #endregion

        #region Svg Reading
        private void readStartElement(XmlReader reader, string name)
        {
            reader.ReadStartElement(name, NAMESPACE);
        }

        private Svg.SvgPolygon pointArrayToPolygon(Svg.SvgPoint[] points)
        {
            return null;
        }

        private Svg.SvgPolygon readSvgPolygon(XmlReader reader, string tagName)
        {
            return null;
        }

        private System.Drawing.PointF stringToPoint(string text)
        {
            if (text == null)
                return System.Drawing.PointF.Empty;
            var values = text.Split(';');
            if (values.Length != 2)
                throw new InvalidOperationException("Cannot convert the specified string to a valid PointF because it contains too" +
                    " many or too few parameters.");
            return new System.Drawing.PointF(stringToFloat(values[0]), stringToFloat(values[1]));
        }

        private float stringToFloat(string text)
        {
            if (text == null || text == "")
                return 0;

            return float.Parse(text, CultureInfo.InvariantCulture);
        }

        private Svg.SvgPath readSvgPath(XmlReader reader)
        {
            var result = new Svg.SvgPath();
            return result;
        }

        private Svg.SvgLine readSvgLine(XmlReader reader)
        {
            var result = new Svg.SvgLine();

            readAllAttributes(reader, result);

            result.StartX = (Svg.SvgUnit)UnitConverter.ConvertFromInvariantString(reader.GetAttribute("X1"));
            result.StartX = (Svg.SvgUnit)UnitConverter.ConvertFromInvariantString(reader.GetAttribute("Y1"));
            result.EndX = (Svg.SvgUnit)UnitConverter.ConvertFromInvariantString(reader.GetAttribute("X2"));
            result.EndX = (Svg.SvgUnit)UnitConverter.ConvertFromInvariantString(reader.GetAttribute("Y2"));

            return result;
        }

        private Svg.SvgEllipse readSvgEllipse(XmlReader reader)
        {
            var result = new Svg.SvgEllipse();

            result.CenterX = (Svg.SvgUnit)UnitConverter.ConvertFromInvariantString(reader.GetAttribute("CX"));
            result.CenterX = (Svg.SvgUnit)UnitConverter.ConvertFromInvariantString(reader.GetAttribute("CX"));

            return result;
        }

        private Svg.SvgCircle readSvgCircle(XmlReader reader)
        {
            var result = new Svg.SvgCircle();
            return result;
        }

        private Svg.SvgRectangle readSvgRectangle(XmlReader reader)
        {
            var result = new Svg.SvgRectangle();
            return result;
        }

        private Svg.SvgGroup readSvgGroup(XmlReader reader)
        {
            var result = new Svg.SvgGroup();
            return result;
        }

        private Svg.SvgClipPath readSvgClipPath(XmlReader reader)
        {
            var result = new Svg.SvgClipPath();
            return result;
        }

        private Svg.SvgGlyph readSvgGlyph(XmlReader reader, string tagName)
        {
            var result = new Svg.SvgGlyph();
            return result;
        }

        /*        private Svg.SvgKern readSvgKern(XmlReader reader, string tagName)
                {
                    var result = new Svg.SvgKern();
                    return result;
                }*/

        private Svg.SvgFontFace readSvgFontFace(XmlReader reader)
        {
            var result = new Svg.SvgFontFace();
            return result;
        }

        private Svg.SvgFont readSvgFont(XmlReader reader)
        {
            var result = new Svg.SvgFont();
            return result;
        }

        private Svg.SvgPaintServer stringToColor(string text)
        {
            return new Svg.SvgColourServer();
        }

        private Svg.SvgUnitCollection stringToStrokeDashArray(string text)
        {
            var result = new Svg.SvgUnitCollection();
            return result;
        }

        private void readAllAttributes(XmlReader reader, Svg.SvgElement element)
        {
        }

        private Svg.SvgViewBox stringToViewBox(string text)
        {
            return new Svg.SvgViewBox();
        }

        private Svg.SvgDocument readSvgDocument(XmlReader reader)
        {
            var result = new Svg.SvgDocument();
            return result;
        }

        private void readAllChildren(XmlReader reader, Svg.SvgElement element)
        {
        }
        #endregion

        public void ReadXml(XmlReader reader)
        {
            if (Target == null)
                Target = new Svg.SvgDocument();

            Target = readSvgDocument(reader);
        }

        public void WriteXml(XmlWriter writer)
        {
            if (Target == null)
                return;

            writeAllChildren(writer, Target);
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public SvgDrawingSerializable() { }
        public SvgDrawingSerializable(Svg.SvgDocument target)
        {
            Target = target;
        }
    }
}
