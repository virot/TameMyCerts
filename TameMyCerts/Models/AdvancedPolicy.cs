using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Xml.Linq;
using System.Runtime.CompilerServices;

namespace TameMyCerts.Models
{
    // Must be public due to XML serialization, otherwise 0x80131509 / System.InvalidOperationException
    [XmlRoot(ElementName = "AdvancedPolicy")]
    public class AdvancedPolicy
    {
        [XmlElement(ElementName = "Action")]
        public string Action { get; set; }

        [XmlElement(ElementName = "PolicyName")]
        public string PolicyName { get; set; } = String.Empty;

        [XmlElement(ElementName = "AdvancedADPolicy")]
        public List<AdvancedADPolicy> AdvancedADPolicy { get; set; } = new List<AdvancedADPolicy>();

        private static string ConvertToHumanReadableXml(string inputString)
        {
            var xmlWriterSettings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true,
                NewLineOnAttributes = true
            };

            var stringBuilder = new StringBuilder();

            var xElement = XElement.Parse(inputString);

            using (var xmlWriter = XmlWriter.Create(stringBuilder, xmlWriterSettings))
            {
                xElement.Save(xmlWriter);
            }

            return stringBuilder.ToString();
        }

        public void SaveToFile(string path)
        {
            var xmlSerializer = new XmlSerializer(typeof(AdvancedPolicy));

            using (var stringWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter))
                {
                    xmlSerializer.Serialize(xmlWriter, this);
                    var xmlData = stringWriter.ToString();

                    File.WriteAllText(path, ConvertToHumanReadableXml(xmlData));
                }
            }
        }
        public string SaveToString()
        {
            var xmlSerializer = new XmlSerializer(typeof(AdvancedPolicy));

            using (var stringWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter))
                {
                    xmlSerializer.Serialize(xmlWriter, this);
                    var xmlData = stringWriter.ToString();

                    return ConvertToHumanReadableXml(xmlData);
                }
            }
        }
    }
}