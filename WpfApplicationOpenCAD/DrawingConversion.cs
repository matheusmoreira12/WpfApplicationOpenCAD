using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;
using netDxf;
using netDxf.Entities;

namespace OpenCad.DrawingConversion
{
    static class DrawingConversion
    {
        static SvgPath ToSvgArc(this Arc arc)
        {
            SvgPath svgarc = new SvgPath();
            double xradius = arc.Radius,
                yradius = arc.Radius
;//                rotation = arc;

            svgarc.Content = "";

            return svgarc;
        }

        static SvgDocument ConvertDXFtoSVG(DxfDocument value)
        {
            SvgDocument result = new SvgDocument();

            foreach (netDxf.Entities.Circle circle in value.Circles) {
                var svgcircle = new SvgCircle();
                svgcircle.CenterX = new SvgUnit((float)circle.Center.X);
                svgcircle.CenterY = new SvgUnit((float)circle.Center.X);
                svgcircle.Radius = new SvgUnit((float)circle.Radius);
                result.Children.Add(svgcircle);
            }

            return null;
        }
    }
}
