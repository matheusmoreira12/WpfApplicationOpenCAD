using System;
using System.Xml.Serialization;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Net;
using System.Xml;
using System.Xml.Schema;

namespace OpenCAD
{
    namespace Serialization
    {
        /// <summary>
        /// Wraps a custom-formatted UTC date-time stamp.
        /// </summary>
        [Serializable]
        public class DateTimeUTCWrapper
        {
            static public implicit operator DateTime(DateTimeUTCWrapper datetime)
            {
                return datetime.Value;
            }

            /// <summary>
            /// Contains the default pattern used for the conversion to and from a string when none is provided.
            /// </summary>
            [XmlIgnore]
            public const string DefaultFormat = "yyyy-MM-dd+HH:mm:ss";

            /// <summary>
            /// Contains the pattern used for the conversion to and from a string.
            /// </summary>
            [XmlAttribute(AttributeName = "Format")]
            public string Format = DefaultFormat;

            [XmlText]
            public string TextContent;

            /// <summary>
            /// Gets or sets the wrapped UTC date-time value.
            /// </summary>
            [XmlIgnore]
            public DateTime Value
            {
                get
                {
                    return DateTime.ParseExact(TextContent, Format, CultureInfo.InvariantCulture, DateTimeStyles.None);
                }

                set
                {
                    TextContent = value.ToUniversalTime().ToString(Format);
                }
            }

            public DateTimeUTCWrapper()
            {
                Value = DateTime.UtcNow;
            }
            public DateTimeUTCWrapper(DateTime value)
            {
                Value = value;
            }
        }

        /// <summary>
        /// Wraps an application version number.
        /// </summary>
        [Serializable]
        public class VersionWrapper
        {
            static public implicit operator Version(VersionWrapper version)
            {
                return version.Value;
            }

            [XmlText]
            public string TextContent;

            /// <summary>
            /// Gets or sets the wrapped version number value.
            /// </summary>
            [XmlIgnore]
            public Version Value
            {
                get
                {
                    return Version.Parse(TextContent);
                }
                set
                {
                    TextContent = value.ToString();
                }
            }

            public VersionWrapper()
            {
                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(executingAssembly.Location);

                Value = Version.Parse(versionInfo.ProductVersion);
            }
            public VersionWrapper(Version value)
            {
                Value = value;
            }
        }

        /// <summary>
        /// Contains all the metadata associated with data.
        /// </summary>
        [Serializable]
        public class MetadataCollection : List<Metadata>
        {
            public Metadata this[string name]
            {
                get { return FindByName(name); }
                set { SetByName(name, value); }
            }

            /// <summary>
            /// Adds a new item to this collection.
            /// </summary>
            /// <param name="item">The item to be added to this collection.</param>
            /// <returns>A boolean indicating if the item has been successfully added.</returns>
            private new bool Add(Metadata item)
            {
                if (FindByName(item.Name) != null)
                    return false;

                base.Add(item);
                return true;
            }

            /// <summary>
            /// Finds a metadata entry by its name. If none is found, returns null.
            /// </summary>
            /// <param name="name">The name to be matched.</param>
            /// <returns>The matching item.</returns>
            public Metadata FindByName(string name)
            {
                foreach (Metadata item in this)
                    if (item.Name == name)
                        return item;

                return null;
            }

            /// <summary>
            /// Sets an item by its name.
            /// </summary>
            /// <param name="name">The name of the item being set.</param>
            /// <param name="item">The new value for the item.</param>
            public void SetByName(string name, Metadata item)
            {
                Metadata findres = FindByName(name);
                if (findres == null)
                    return;

                int index = this.IndexOf(findres);
                this[index] = item;
            }

            /// <summary>
            /// Finds metadata by their content.
            /// </summary>
            /// <param name="content">The content to be matched.</param>
            /// <returns>A list containing all the matching items.</returns>
            public List<Metadata> FindByContent(string content)
            {
                return FindAll(meta => meta.Content == content);
            }
        }

        /// <summary>
        /// Holds data describing data.
        /// </summary>
        [Serializable]
        public class Metadata
        {
            static public implicit operator string(Metadata metadata)
            {
                return metadata.Content;
            }

            public bool IsEmpty { get { return Content != null && Content != ""; } }
            public bool IsUnnamed { get { return Name != null && Name != ""; } }

            /// <summary>
            /// The identifier for this metadata tag.
            /// </summary>
            [XmlAttribute("Name")]
            public string Name;

            /// <summary>
            /// The content for this metadata tag.
            /// </summary>
            [XmlAttribute("Content")]
            public string Content;

            public Metadata() { }
            public Metadata(string name, string content)
            {
                Name = name;
                Content = content;
            }
        }

        /// <summary>
        /// Holds implicit data describing data.
        /// </summary>
        [Serializable]
        public class ImplicitMetadata
        {
            static public implicit operator string(ImplicitMetadata metadata)
            {
                return metadata.Content;
            }

            public bool IsEmpty { get { return Content != null && Content != ""; } }

            /// <summary>
            /// The content for this metadata tag.
            /// </summary>
            [XmlText]
            public string Content;

            public ImplicitMetadata() { }
            public ImplicitMetadata(string content)
            {
                Content = content;
            }
        }

        /// <summary>
        /// Wraps a string inside a CDATA section.
        /// </summary>
        [Serializable]
        public class CDATAWrapper
        {
            static public implicit operator string(CDATAWrapper cdata)
            {
                return cdata.TextContent;
            }

            [XmlText()]
            public string TextContent;

            private string wrapCDATA(string content)
            {
                return content.Replace("<![CDATA[", "").Replace("]]>", "");
            }

            private string unwrapCDATA(string wrappedContent)
            {
                return "<![CDATA[" + wrappedContent + "]]>";
            }

            /// <summary>
            /// Gets or sets the wrapped value.
            /// </summary>
            [XmlIgnore]
            public string Value
            {
                get
                {
                    return unwrapCDATA(TextContent);
                }
                set
                {
                    TextContent = wrapCDATA(TextContent);
                }
            }

            public CDATAWrapper() { }
            public CDATAWrapper(string value)
            {
                Value = value;
            }
        }

        /// <summary>
        /// Wraps a buffer encoded in base64 inside a CDATA section.
        /// </summary>
        [Serializable]
        public class Base64Wrapper
        {
            static public implicit operator string(Base64Wrapper base64)
            {
                return base64.Base64Content;
            }

            static public implicit operator byte[] (Base64Wrapper base64)
            {
                return base64.RawContent;
            }

            /// <summary>
            /// Gets or sets the base64 content.
            /// </summary>
            [XmlText]
            public string Base64Content
            {
                get { return Convert.ToBase64String(RawContent); }
                set { RawContent = Convert.FromBase64String(value); }
            }

            /// <summary>
            /// Gets or sets the raw binary content.
            /// </summary>
            [XmlIgnore]
            public byte[] RawContent = new byte[0];
        }

        /// <summary>
        /// Wraps embedded content in base64 inside a CDATA section, with or without compression.
        /// </summary>
        [Serializable]
        public class EmbeddedContent
        {
            private string wrapCDATA(string content)
            {
                return content.Replace("<![CDATA[", "").Replace("]]>", "");
            }

            private string unwrapCDATA(string wrappedContent)
            {
                return "<![CDATA[" + wrappedContent + "]]>";
            }

            /// <summary>
            /// Gets the mime type for the specified file extension.
            /// </summary>
            /// <param name="extension">The file extension.</param>
            /// <returns>A string containing the file mime type.</returns>
            static public string GetMimeType(string extension)
            {
                return MimeTypeMap.List.MimeTypeMap.GetMimeType(extension).FirstOrDefault();
            }

            /// <summary>
            /// Gets the file extension for the specified mime type.
            /// </summary>
            /// <param name="mimeType">The file mime type.</param>
            /// <returns>A string containing the file extension.</returns>
            static public string GetExtension(string mimeType)
            {
                return MimeTypeMap.List.MimeTypeMap.GetExtension(mimeType).FirstOrDefault();
            }

            /// <summary>
            /// Creates a new embedded content tag from a web resource.
            /// </summary>
            /// <param name="uriString">The resource location URI string.</param>
            /// <param name="compress">Indicates that the content shold get compressed.</param>
            /// <returns>The new embedded content tag.</returns>
            static public EmbeddedContent CreateFromWebResource(string uriString, bool compress = false)
            {
                EmbeddedContent result = new EmbeddedContent();
                result.ReadFromWebResource(uriString);
                result.Compressed = compress;

                return result;
            }

            /// <summary>
            /// Creates a new embedded content tag from 
            /// </summary>
            /// <param name="path">The file to be read from.</param>
            /// <param name="compress">Indicates that the content shold get compressed.</param>
            /// <returns>The new embedded content tag.</returns>
            static public EmbeddedContent CreateFromFile(string path, bool compress = false)
            {
                EmbeddedContent result = new EmbeddedContent();
                result.ReadFromFile(path);
                result.Compressed = compress;

                return result;
            }

            /// <summary>
            /// Gets the path to which the temporary file has been saved to.
            /// </summary>
            [XmlIgnore]
            public string TemporaryFilePath { get; private set; } = null;

            /// <summary>
            /// Saves the content to the specified file.
            /// </summary>
            /// <param name="location">The file path to write to.</param>
            public void SaveToFile(string path)
            {
                //Read from memory, copy and save to file
                using (MemoryStream tempmemstream = new MemoryStream(RawContent))
                using (FileStream tempfilestream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite,
                    FileShare.Read, RawContent.Length))
                {
                    tempmemstream.Seek(0, SeekOrigin.Begin);
                    tempmemstream.CopyTo(tempfilestream);
                }
            }

            /// <summary>
            /// Saves the content to a temporary file.
            /// </summary>
            public void SaveToTemporaryFile()
            {
                string tempfilepath = Path.GetTempPath() + Id + Extension;

                SaveToFile(tempfilepath);

                TemporaryFilePath = tempfilepath;
            }

            /// <summary>
            /// Reads from the specified web resource.
            /// </summary>
            /// <param name="uriString">The resource location URI string.</param>
            /// <returns>A boolean indicating if the operation was successful.</returns>
            public void ReadFromWebResource(string uriString)
            {
                //Request file and get response
                WebRequest filereq = HttpWebRequest.Create(uriString);
                WebResponse fileresp = filereq.GetResponse();

                //Copy the response stream to memory
                using (Stream stream = fileresp.GetResponseStream())
                using (MemoryStream tempmemstream = new MemoryStream())
                {
                    stream.CopyTo(tempmemstream);
                    RawContent = tempmemstream.ToArray();
                }

                MimeType = fileresp.ContentType;
            }

            /// <summary>
            /// Reads the content from the specified file.
            /// </summary>
            /// <param name="location">The file path to read from.</param>
            public void ReadFromFile(string path)
            {
                //Open, read and copy file to memory.
                using (FileStream tempfilestream = new FileStream(path, FileMode.Open))
                using (MemoryStream tempmemstream = new MemoryStream(RawContent, true))
                    tempfilestream.CopyTo(tempmemstream);
            }

            /// <summary>
            /// Reads the content from the temporary file.
            /// </summary>
            public void ReadFromTemporaryFile()
            {
                if (TemporaryFilePath == null)
                    return;

                ReadFromFile(TemporaryFilePath);
            }

            /// <summary>
            /// Deletes the generated temporary file.
            /// </summary>
            public void DeleteTemporaryFile()
            {
                if (TemporaryFilePath == null)
                    return;

                File.Delete(TemporaryFilePath);
            }

            /// <summary>
            /// Gets a boolean indicating if this embedded content tag is valid.
            /// </summary>
            [XmlIgnore]
            public bool IsValid { get { return _RawContent != null && _RawContent.Length > 0; } }

            /// <summary>
            /// Gets a boolean indicating if the embedded content is presented in plain-text form.
            /// </summary>
            [XmlIgnore]
            public bool IsPlainText { get { return MimeType.Contains("Text"); } }

            /// <summary>
            /// The identifier for the embedded content tag.
            /// </summary>
            [XmlAttribute(AttributeName = "id", Namespace = "http://www.w3.org/XML/1998/namespace")]
            public string Id;

            /// <summary>
            /// The mime type of the embedded content.
            /// </summary>
            [XmlAttribute("MimeType")]
            public string MimeType;

            /// <summary>
            /// Gets or sets the extension of the embedded content.
            /// </summary>
            [XmlIgnore]
            public string Extension
            {
                get { return GetExtension(MimeType); }
                set { MimeType = GetMimeType(value); }
            }

            /// <summary>
            /// Gets or sets if the embedded content is compressed with GZip.
            /// </summary>
            [XmlAttribute("Compressed")]
            public bool Compressed;

            /// <summary>
            /// Gets or sets the content as string.
            /// </summary>
            [XmlText]
            public string ContentAsString
            {
                get
                {
                    if (IsPlainText)
                        return unwrapCDATA(System.Text.Encoding.ASCII.GetString(_RawContent));
                    else
                        return Convert.ToBase64String(_RawContent);
                }
                set
                {
                    if (IsPlainText)
                        _RawContent = System.Text.Encoding.ASCII.GetBytes(wrapCDATA(value));
                    else
                        _RawContent = Convert.FromBase64String(value);
                }
            }

            /// <summary>
            /// Gets or sets the raw binary content, compressing or decompressing if needed.
            /// </summary>
            [XmlIgnore]
            public byte[] RawContent
            {
                get
                {
                    if (Compressed)
                        return CompressionUtility.CompressBuffer(_RawContent, CompressionMode.Decompress);
                    else
                        return _RawContent;
                }
                set
                {
                    if (Compressed)
                        _RawContent = CompressionUtility.CompressBuffer(value, CompressionMode.Compress);
                    else
                        _RawContent = value;
                }
            }
            private byte[] _RawContent = new byte[0];

            public EmbeddedContent() { }

            /// <summary>
            /// Creates a new wrapper from raw binary data.
            /// </summary>
            /// <param name="buffer">The buffer containing the raw binary data.</param>
            /// <param name="mimeType">The mime type corresponding to the file extension of the stored data.</param>
            /// <param name="compress"></param>
            public EmbeddedContent(byte[] buffer, string mimeType = "binary",
                bool compress = false) : base()
            {
                Compressed = compress;
                RawContent = buffer;
                MimeType = mimeType;
            }

            ~EmbeddedContent()
            {
                DeleteTemporaryFile();
            }
        }

        [Serializable]
        public class EmbeddedContentCollection : List<EmbeddedContent>
        {
            private string generateNewEmbeddedContentId()
            {
                return "EmbeddedContent_" + Guid.NewGuid().ToString();
            }

            public new bool Add(EmbeddedContent item)
            {
                if (!item.IsValid)
                    return false;

                item.Id = generateNewEmbeddedContentId();

                base.Add(item);
                return true;
            }

            public EmbeddedContent FindById(string id)
            {
                foreach (EmbeddedContent item in this)
                    if (id == item.Id)
                        return item;

                return null;
            }
        }

        [Serializable]
        public class Header
        {
            [XmlElement(ElementName = "Title")]
            public ImplicitMetadata Title = new ImplicitMetadata("Untitled");
            [XmlElement(ElementName = "Description")]
            public ImplicitMetadata Description = new ImplicitMetadata();

            [XmlElement(ElementName = "Metadata", Type = typeof(Metadata))]
            public MetadataCollection Metadata = new MetadataCollection();

            [XmlElement(ElementName = "CreationDate")]
            public DateTimeUTCWrapper CreationDate = new DateTimeUTCWrapper();
            [XmlElement(ElementName = "LastModificationDate")]
            public DateTimeUTCWrapper LastModificationDate = new DateTimeUTCWrapper();

            [XmlElement(ElementName = "NativeSoftwareVersion")]
            public VersionWrapper NativeSoftwareVersion = new VersionWrapper();

            [XmlElement(ElementName = "PortabilitySoftwareVersion")]
            public VersionWrapper PortabilitySoftwareVersion = null;
            [XmlElement(ElementName = "PortabilityConversionDate")]
            public DateTimeUTCWrapper PortabilityConversionDate = null;
            [XmlElement(ElementName = "PortabilityOriginalCompressed")]
            public EmbeddedContent PortabilityOriginalCompressed = null;

            [XmlElement(ElementName = "GitRepositoryInformation")]
            public GitRepositoryInformation GitRepositoryInformation = null;

            [XmlElement(ElementName = "CloudBackupInformation")]
            public CloudBackupInformation CloudBackupInformation = null;
        }

        [Serializable]
        public class GitRepositoryInformation
        {
        }

        [Serializable]
        public class CloudBackupInformation
        {
        }

        namespace LibrarySerialization
        {
            [Serializable]
            public class LibrarySerializer : XmlSerializer
            {
                public LibraryRoot Root;

                public new void Deserialize(XmlReader reader)
                {
                    Root = (LibraryRoot)base.Deserialize(reader);
                }

                public void Serialize(XmlWriter writer)
                {
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add("xml", "http://www.w3.org/XML/1998/namespace");
                    ns.Add("xlink", "http://www.w3.org/1999/xlink");
                    ns.Add("svg", "http://www.w3.org/2000/svg");
                    ns.Add("ocxlib", "http://www.opencad.org/2017/ocxlib");
                    ns.Add("ocxlib-dxf", "http://www.opencad.org/2017/ocxlib-dxf");

                    Serialize(writer, Root, ns);
                }

                public LibrarySerializer() : base(typeof(LibraryRoot))
                {
                    Root = new LibraryRoot();
                }
            }

            [XmlRoot(ElementName = "Library", Namespace = "http://www.opencad.org/2017/ocxlib")]
            public class LibraryRoot
            {
                [XmlElement(ElementName = "Header")]
                public Header Header = new Header();

                [XmlArray("EmbeddedContent")]
                [XmlArrayItem(ElementName = "Content", Type = typeof(EmbeddedContent))]
                public EmbeddedContentCollection EmbeddedContent = new EmbeddedContentCollection();

                [XmlArray("Components")]
                [XmlArrayItem(ElementName = "Component")]
                public ComponentCollection Components = new ComponentCollection();
            }

            [Serializable]
            public class ComponentCollection : List<Component>
            {
                /// <summary>
                /// Checks if the item is unique.
                /// </summary>
                /// <param name="item">The item being checked.</param>
                /// <returns>A boolean indicating that the item is unique.</returns>
                public bool HasSimilarItem(Component item) { return FindByFullName(item.FullName) != null; }

                /// <summary>
                /// Adds a new component to this collection.
                /// </summary>
                /// <param name="item">The item that will be added.</param>
                /// <returns>A boolean indicating if the operation was successful.</returns>
                public new bool Add(Component item)
                {
                    if (HasSimilarItem(item))
                        return false;
                    if (!item.IsValid)
                        return false;

                    base.Add(item);

                    return true;
                }

                public Component FindByName(string name)
                {
                    foreach (Component item in this)
                        if (item.Name == name)
                            return item;

                    return null;
                }

                public Component FindByFullName(string fullname)
                {
                    foreach (Component item in this)
                        if (item.FullName == fullname)
                            return item;

                    return null;
                }

                public List<Component> FindBySeries(string series)
                {
                    return FindAll(a => a.Series == series);
                }

                public List<Component> FindByVariant(string variant)
                {
                    return FindAll(a => a.Variant == variant);
                }

                public List<Component> FindByReferenceDesignator(string referenceDesignator)
                {
                    return FindAll(a => a.ReferenceDesignator == referenceDesignator);
                }

                public List<Component> FindByManufacturer(string manufacturer)
                {
                    return FindAll(a => a.Manufacturer == manufacturer);
                }

                public List<Component> FindByMetadata(string name, string content)
                {
                    return FindAll(a => a.Metadata.FindByName(name) == content);
                }

                public void SortByName()
                {
                    Sort((a, b) => string.Compare(a.Name.Content, b.Name.Content));
                }

                public void SortBySeries()
                {
                    Sort((a, b) => string.Compare(a.Name.Content, b.Name.Content));
                }

                public void SortByVariant()
                {
                    Sort((a, b) => string.Compare(a.Name.Content, b.Name.Content));
                }

                public void SortByReferenceDesignator()
                {
                    Sort((a, b) => string.Compare(a.Name.Content, b.Name.Content));
                }

                public void SortByManufacturer()
                {
                    Sort((a, b) => string.Compare(a.Name.Content, b.Name.Content));
                }

                public void SortByDatasheet()
                {
                    Sort((a, b) => string.Compare(a.Name.Content, b.Name.Content));
                }

                public void SortByMetadata(string name)
                {
                    Sort((a, b) => string.Compare(a.Metadata[name].Content, b.Metadata[name].Content));
                }
            }

            [Serializable]
            public class Component
            {
                [XmlIgnore]
                public bool IsValid { get { return !Name.IsEmpty && !Series.IsEmpty && !Variant.IsEmpty && !ReferenceDesignator.IsEmpty; } }

                [XmlElement(ElementName = "Name")]
                public ImplicitMetadata Name = new ImplicitMetadata("New Component");

                [XmlElement(ElementName = "Series")]
                public ImplicitMetadata Series = new ImplicitMetadata("");

                [XmlElement(ElementName = "Variant")]
                public ImplicitMetadata Variant = new ImplicitMetadata("");

                public string FullName { get { return Series + Name + Variant; } }

                [XmlElement(ElementName = "Value")]
                public ImplicitMetadata Value = new ImplicitMetadata("");

                [XmlElement(ElementName = "ReferenceDesignator")]
                public ImplicitMetadata ReferenceDesignator = new ImplicitMetadata("U");

                [XmlElement(ElementName = "Manufacturer")]
                public ImplicitMetadata Manufacturer = new ImplicitMetadata("");

                [XmlElement(ElementName = "Datasheet")]
                public ImplicitMetadata Datasheet = new ImplicitMetadata("");

                [XmlElement(ElementName = "Metadata", Type = typeof(Metadata))]
                public MetadataCollection Metadata = new MetadataCollection();

                [XmlElement(ElementName = "Symbol")]
                public ComponentSymbol Symbol;
            }
        }

        public struct Placement
        {
            static public Placement Parse(string value)
            {
                string[] values = value.Split(';');
                if (values.Length != 2)
                    throw new InvalidOperationException("String contains too many or too few coordinates.");

                return new Placement(decimal.Parse(values[0]), decimal.Parse(values[1]));
            }

            Cureos.Measures.Quantities.Length X;
            Cureos.Measures.Quantities.Length Y;

            public new string ToString()
            {
                return X.StandardAmount + ";" + Y.StandardAmount;
            }

            public Placement(decimal amountX, decimal amountY)
            {
                X = new Cureos.Measures.Quantities.Length(amountX);
                Y = new Cureos.Measures.Quantities.Length(amountY);
            }
        }

        public class ComponentPin : IXmlSerializable
        {
            Placement Placement = new Placement();
            
            public XmlSchema GetSchema()
            {
                return null;
            }

            public void ReadXml(XmlReader reader)
            {
                string plattr = reader.GetAttribute("Placement");
                if (plattr == null)
                    throw new InvalidOperationException("Missing required attribute \"Placement\".");

                Placement = Placement.Parse(plattr);
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteAttributeString("Placement", Placement.ToString());
            }
        }

        public class SvgSerializer : IXmlSerializable
        {
            Svg.SvgDocument Target;

            private void writeSvgElement(XmlWriter writer, Svg.SvgElement svgElement)
            {
                var unitconverter = new Svg.SvgUnitConverter();
                var colorconverter = new Svg.SvgColourConverter();

                writer.WriteAttributeString("color", colorconverter.ConvertToString(svgElement.Color));
                writer.WriteAttributeString("fill", colorconverter.ConvertToString(svgElement.Fill.Color));
                writer.WriteAttributeString("fill-opacity", svgElement.FillOpacity.ToString());
                writer.WriteAttributeString("fill-rule", new Svg.SvgFillRuleConverter().ConvertToString(svgElement.FillRule));
                writer.WriteAttributeString("font", svgElement.Font);
                writer.WriteAttributeString("font-family", svgElement.FontFamily);
                writer.WriteAttributeString("font-size", unitconverter.ConvertToString(svgElement.FontSize));
                writer.WriteAttributeString("font-style", new Svg.SvgFontStyleConverter().ConvertToString(svgElement.FontStyle));
                writer.WriteAttributeString("font-variant", new Svg.SvgFontVariantConverter().ConvertToString(svgElement.FontVariant));
                writer.WriteAttributeString("font-weight", new Svg.SvgFontWeightConverter().ConvertToString(svgElement.FontWeight));
                writer.WriteAttributeString("id", svgElement.ID);
                writer.WriteAttributeString("opacity", svgElement.Opacity.ToString());
                writer.WriteAttributeString("stroke", svgElement.Stroke.Content);
                writer.WriteAttributeString("stroke-dasharray", string.Join(",", svgElement.StrokeDashArray.Select(u => u.Value)));
                writer.WriteAttributeString("stroke-dashoffset", unitconverter.ConvertToString(svgElement.StrokeDashOffset));
                writer.WriteAttributeString("stroke-linecap", new Svg.SvgStrokeLineCapConverter().ConvertToString(svgElement.StrokeLineCap));
                writer.WriteAttributeString("stroke-linejoin", new Svg.SvgStrokeLineJoinConverter().ConvertToString(svgElement.StrokeLineJoin));
                writer.WriteAttributeString("stroke-opacity", svgElement.StrokeOpacity.ToString());
                writer.WriteAttributeString("stroke-width", unitconverter.ConvertToString(svgElement.StrokeWidth));
                writer.WriteAttributeString("transform", new Svg.Transforms.SvgTransformConverter().ConvertToString(svgElement.Transforms));

                foreach (string key in svgElement.CustomAttributes.Keys)
                {
                    string value = svgElement.CustomAttributes[key];
                    writer.WriteAttributeString(key, value);
                }

                foreach (Svg.SvgElement child in svgElement.Children)
                {
                    if (child is Svg.SvgCircle)
                    {
                        var circle = child as Svg.SvgCircle;
                        writer.WriteStartElement("circle");
                        writer.WriteAttributeString("r", unitconverter.ConvertToString(circle.Radius));
                        writer.WriteAttributeString("cx", unitconverter.ConvertToString(circle.CenterX));
                        writer.WriteAttributeString("xy", unitconverter.ConvertToString(circle.CenterY));
                        writer.WriteEndElement();
                    }
                    if (child is Svg.SvgPath)
                    {
                        var path = child as Svg.SvgPath;
                        writer.WriteStartElement("path");
                        writer.WriteAttributeString("d", new Svg.SvgPathBuilder().ConvertToString(path.PathData));
                        writer.WriteEndElement();
                    }
                    if (child is Svg.SvgClipPath)
                    {
                        var clippath = child as Svg.SvgClipPath;
                        writer.WriteStartElement("clipPath");
                        writer.WriteEndElement();
                    }
                    if (child is Svg.SvgDefinitionList)
                    {
                        var deflist = child as Svg.SvgDefinitionList;
                    }
                    if (child is Svg.SvgDescription)
                    {
                        var desc = child as Svg.SvgDescription;
                    }
                    if (child is Svg.SvgDescription)
                    {
                        var desc = child as Svg.SvgDescription;
                    }

                    writeSvgElement(writer, child);
                }
            }

            public void ReadXml(XmlReader reader)
            {
                
            }

            public void WriteXml(XmlWriter writer)
            {
                throw new NotImplementedException();
            }

            public XmlSchema GetSchema()
            {
                throw new NotImplementedException();
            }
        }

        [Serializable]
        public class ComponentSymbol
        {
            [XmlAttribute("Format")]
            public ComponentSymbolFormat Format = ComponentSymbolFormat.Empty;

            [XmlElement(ElementName = "SerializedDXF", Namespace = "http://www.opencad.org/2017/ocxlib_dxf")]
            public netDxf.DxfDocument DxfDocument;

            [XmlElement(ElementName = "svg", Namespace = "http://www.w3.org/2000/svg")]
            public Svg.SvgDocument SvgDocument;

            [XmlElement(Namespace = "Pin")]
            public ComponentPin Pins;
        }

        [Serializable]
        public enum ComponentSymbolFormat
        {
            [XmlEnum("serialized-dxf")]
            CustomSerializedDxf,
            [XmlEnum("custom-svg")]
            CustomSvg,
            [XmlEnum("empty")]
            Empty
        }

        namespace SchematicDesignSerialization
        {
            [XmlRoot(ElementName = "Schematic", Namespace = "http://www.opencad.org/2017/ocxsch/")]
            public class SchematicDesignRoot
            {
                [XmlElement(ElementName = "Header")]
                public Header Header;

                [XmlElement(ElementName = "Content")]
                public SchematicDesignContent LibraryContent;
            }

            [Serializable]
            public class SchematicDesignContent
            {

            }
        }

        namespace PCBDesignSerialization
        {
            [XmlRoot(ElementName = "PCB", Namespace = "http://www.opencad.org/2017/ocxpcb/")]
            public class PCBDesignRoot
            {
                [XmlElement(ElementName = "Header")]
                public Header Header;

                [XmlElement(ElementName = "Content")]
                public PCBDesignContent LibraryContent;
            }

            [Serializable]
            public class PCBDesignContent
            {

            }
        }
    }
}