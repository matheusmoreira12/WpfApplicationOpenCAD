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

namespace WpfApplicationOpenCAD
{
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



            var s = new OpenCAD.Serialization.LibrarySerialization.LibrarySerializer();
            var x = System.Xml.XmlWriter.Create("xml-serialization-test.oxl");

            s.Root.EmbeddedContent.Add(OpenCAD.Serialization.EmbeddedContent.CreateFromWebResource(
                "http://toshiba.semicon-storage.com/info/docget.jsp?did=20429&prodName=2SA1962"));

            var svg = Svg.SvgDocument.Open("C:\\Users\\Matheus\\documents\\visual studio 2015\\Projects\\WpfApplicationOpenCAD\\WpfApplicationOpenCAD\\bin\\Debug\\13310609211506318453Standing Tiger.svg");
            var c = new OpenCAD.Serialization.LibrarySerialization.Component();
            c.Name.Content = "Tiger";
            c.Symbol.SvgDocument = svg;
            c.ReferenceDesignator.Content = "T";
            var b = s.Root.Components.Add(c);

            var p = new OpenCAD.OpenCADFormat.DataStrings.DataStringParser("move(0.; 0); line(0; 0); line(0; 0); line(0; 0); line(0; 0); line(0; 0); line(0; 0); line(0; 0); line(0; 0); line(0; 0); line(0; 0); line(0; 0); close()");
            var pc = p.Parse();

            var freq = OpenCAD.OpenCADFormat.Measures.Measurement<OpenCAD.OpenCADFormat.Measures.Quantities.Frequency>.Parse("1MHz");

            var amount = freq.Amount;

            var symbol = freq.Unit.Symbol;

            s.Serialize(x);
            x.Close();

            var m = System.Xml.XmlReader.Create("xml-serialization-test.oxl");
            s.Deserialize(m);

            if (s.Root.EmbeddedContent.Count > 0)
                s.Root.EmbeddedContent[0].SaveToTemporaryFile();
        }
    }
}
