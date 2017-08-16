using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using netDxf;

namespace OpenCAD.DXFXmlSerialization
{
    class DxfXmlSerializer : XmlSerializer
    {
        DxfXmlSerializer() : base(typeof (XmlDocument))
        {

        }
    }
}
