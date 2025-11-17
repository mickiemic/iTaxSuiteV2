using System.Xml;
using System.Xml.Serialization;

namespace iTaxSuite.Library.Extensions
{
    public class XmlUtils
    {
        public static T Deserialize<T>(string xmlString) where T : class
        {
            T result = null;
            if (string.IsNullOrWhiteSpace(xmlString))
                throw new Exception("Invalid XML string provided for deserialization");

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (TextReader reader = new StringReader(xmlString))
            {
                result = xmlSerializer.Deserialize(reader) as T;
            }

            return result;
        }

        public static string Serialize<T>(T entry, bool omitNamespace = false, bool indent = false) where T : class
        {
            string xmlString = string.Empty;

            XmlSerializer xmlSerializer = new XmlSerializer(entry.GetType());
            using (StringWriter xmlStream = new StringWriter())
            {
                XmlWriterSettings writerSettings = new XmlWriterSettings
                {
                    Indent = indent,
                    OmitXmlDeclaration = omitNamespace
                };
                using (XmlWriter xmlWriter = XmlWriter.Create(xmlStream, writerSettings))
                {
                    if (omitNamespace)
                    {
                        xmlSerializer.Serialize(xmlWriter, entry, new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
                    }
                    else
                        xmlSerializer.Serialize(xmlWriter, entry);
                }
                xmlString = xmlStream.ToString();
            }

            return xmlString;
        }
    }
}
