using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace iTaxSuite.WinForms.Extensions
{
    internal class GenUtil
    {
        public static T xmlDeserialize<T>(string xmlString) where T : class
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

        public static string xmlSerialize<T>(T entry, bool omitNamespace = false, bool indent = false) where T : class
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

        public static bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) { return false; }

            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch //some other exception
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool IsValidXML(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) { return false; }

            strInput = strInput.Trim();
            if (strInput.StartsWith("<") && strInput.EndsWith(">"))
            {
                try
                {
                    var doc = XDocument.Parse(strInput);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static string xmlFormat(string xmlString, bool indent)
        {
            var stringBuilder = new StringBuilder();

            var element = XElement.Parse(xmlString);

            var settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = indent;
            //settings.NewLineOnAttributes = true;

            using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
            {
                element.Save(xmlWriter);
            }

            return stringBuilder.ToString();
        }

        public static string PrettyPrint(string xmlString, bool indent)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);

            var stringWriter = new StringWriter(new StringBuilder());
            var xmlTextWriter = new XmlTextWriter(stringWriter) { Formatting = (indent ? Formatting.Indented : Formatting.None) };
            doc.Save(xmlTextWriter);
            return stringWriter.ToString();
        }

        public static string PrettyPrintX2(string xmlString)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);
            // Format the XML text.
            StringWriter string_writer = new StringWriter();
            XmlTextWriter xml_text_writer = new XmlTextWriter(string_writer);
            xml_text_writer.Formatting = System.Xml.Formatting.Indented;
            doc.WriteTo(xml_text_writer);
            string result = string_writer.ToString();
            return result;
        }

        public static string patternCleanup(string data, string pattern, string replace)
        {
            if (string.IsNullOrWhiteSpace(data) || string.IsNullOrWhiteSpace(pattern) || string.IsNullOrWhiteSpace(replace))
                return data;

            return Regex.Replace(data, pattern, replace);
        }

        public static string GetNetworkByMobile(string mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
                return string.Empty;
            if (Regex.IsMatch(mobileNumber, "^((?:\\+|0{2})?265|0)?(88\\d{7})$"))
            {
                return "TNM";
            }
            else if (Regex.IsMatch(mobileNumber, "^((?:\\+|0{2})?265|0)?(99\\d{7})$"))
            {
                return "AIRTEL";
            }
            return string.Empty;
        }

        public static string strAppendLine(string source, string line)
        {
            if (string.IsNullOrWhiteSpace(source))
                return line;
            else
                return source + "\r\n" + line;
        }

        public static bool validateModel<T>(T model, out string error)
        {
            error = string.Empty;

            ValidationContext _valContext = new ValidationContext(model);
            List<ValidationResult> _valResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(model, _valContext, _valResults, true))
            {
                foreach (var vError in _valResults)
                {
                    error = strAppendLine(error, vError.ErrorMessage);
                }
                return false;
            }

            return true;
        }
    }

}
