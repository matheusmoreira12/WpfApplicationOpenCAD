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

            var s = new OpenCAD.Serialization.LibrarySerialization.LibrarySerializer();
            var x = System.Xml.XmlWriter.Create("xml-serialization-test.oxl");

            s.Root.EmbeddedContent.Add(OpenCAD.Serialization.EmbeddedContent.CreateFromWebResource(
                "http://toshiba.semicon-storage.com/info/docget.jsp?did=20429&prodName=2SA1962"));

            var d = netDxf.DxfDocument.Load("Transistor-NPN-Electronics-TR.dxf");
//            var c = new OpenCAD.Serialization.LibrarySerialization.Component();
//            c.Symbol.DxfDocument = d;
//            s.Root.Components.Add(c);

            s.Serialize(x);
            x.Close();

            var m = System.Xml.XmlReader.Create("xml-serialization-test.oxl");
            s.Deserialize(m);

            if (s.Root.EmbeddedContent.Count > 0)
                s.Root.EmbeddedContent[0].SaveToTemporaryFile();
        }
    }
}
