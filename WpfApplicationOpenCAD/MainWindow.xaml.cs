using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using OpenCAD.Drawing;

using OpenCAD.OpenCADFormat.Measures.Quantities;
using OpenCAD.OpenCADFormat.Measures;

namespace WpfApplicationOpenCAD
{
    [OpenCAD.OpenCADFormat.DataStrings.Function("test")]
    public class TestClass
    {
        [OpenCAD.OpenCADFormat.DataStrings.FloatLiteral()]
        public float TestFloat0 = 100;
        [OpenCAD.OpenCADFormat.DataStrings.FloatLiteral()]
        public float TestFloat1 = 200;
        [OpenCAD.OpenCADFormat.DataStrings.IntegerLiteral()]
        public float TestInt1 = 300;
        [OpenCAD.OpenCADFormat.DataStrings.IntegerLiteral()]
        public float TestInt2 = 400;
        [OpenCAD.OpenCADFormat.DataStrings.StringLiteral()]
        public string TestString1 = "Hello world!";
        [OpenCAD.OpenCADFormat.DataStrings.BinaryLiteral(OriginalRepresentation = OpenCAD.OpenCADFormat.DataStrings.DataStringLiteralBinaryRepresentation.Hexadecimal)]
        public System.Collections.BitArray TestBinary1 = new System.Collections.BitArray(new[] { true, true, true, true, false });
        [OpenCAD.OpenCADFormat.DataStrings.StringLiteral()]
        public IMeasurement<Frequency> TestMeasurement1 = new Measurement<Frequency>(120, Frequency.Hertz, MetricPrefixes.Mega);
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            var stops = new OpenCAD.Drawing.GradientStopCollection(new OpenCAD.Drawing.GradientStop[] {
                new OpenCAD.Drawing.GradientStop(System.Drawing.Color.Red, 0f),
                new OpenCAD.Drawing.GradientStop(System.Drawing.Color.Yellow, .16f),
                new OpenCAD.Drawing.GradientStop(System.Drawing.Color.Lime, .33f),
                new OpenCAD.Drawing.GradientStop(System.Drawing.Color.Aqua, .49f),
                new OpenCAD.Drawing.GradientStop(System.Drawing.Color.Blue, .66f),
                new OpenCAD.Drawing.GradientStop(System.Drawing.Color.Magenta, .82f),
                new OpenCAD.Drawing.GradientStop(System.Drawing.Color.Red, 1f)});

            var grad = new OpenCAD.Drawing.ConicGradientBrush(stops);

            var unicodeCategories = OpenCAD.Unicode.UnicodeCategory.AllCategories;

            /*            var s = new OpenCAD.Serialization.LibrarySerialization.LibrarySerializer();
                        var x = System.Xml.XmlWriter.Create(@"xml-serialization-test.oxl");

                        s.Root.EmbeddedContent.Add(OpenCAD.Serialization.EmbeddedContent.CreateFromWebResource(
                            "http://toshiba.semicon-storage.com/info/docget.jsp?did=20429&prodName=2SA1962"));

                        var svg = Svg.SvgDocument.Open(@"C:\Users\Matheus\documents\visual studio 2015\Projects\WpfApplicationOpenCAD\WpfApplicationOpenCAD\bin\Debug\13310609211506318453Standing Tiger.svg");
                        var c = new OpenCAD.Serialization.LibrarySerialization.Component();
                        c.Name.Content = "Tiger";
                        c.Symbol.SvgDocument = svg;
                        c.ReferenceDesignator.Content = "T";
                        var b = s.Root.Components.Add(c);

                        var measurement = new Measurement<Frequency>(120, Frequency.Hertz, MetricPrefixes.Mega);

                        var measurement1 = measurement.ConvertTo(Frequency.Hertz);

                        var measurement2 = measurement1.ConvertTo(Frequency.Hertz, MetricPrefixes.Tera);
                        */
            var mc = OpenCAD.OpenCADFormat.DataStrings.DataStringMainContext.Parse("move(0; 0); line(0; 0); line(0; 0); line(0; 0); line(0; 0); line(0; 0); line(0; 0); line(0; 0); line(0; 0); line(0; 0); line(0; 0); line(0; 0); close()");
            var str = mc.ToString();


            var drcol = new OpenCAD.OpenCADFormat.Drawing.ColorCMYK(0, 0, 0, 1);
            var col = drcol.ToColor();
            var enc = new OpenCAD.OpenCADFormat.DataStrings.Serialization.Encoder(drcol);
            var e = enc.Encode();
            var drcolstr = ((OpenCAD.OpenCADFormat.DataStrings.DataStringFunction)e).ToString();

            /*            s.Serialize(x);
                        x.Close();

                        var m = System.Xml.XmlReader.Create(@"xml-serialization-test.oxl");
                        s.Deserialize(m);

                        if (s.Root.EmbeddedContent.Count > 0)
                            s.Root.EmbeddedContent[0].SaveToTemporaryFile();*/
        }
    }
}
