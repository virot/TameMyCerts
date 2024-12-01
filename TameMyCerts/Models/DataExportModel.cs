
using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using TameMyCerts.Enums;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Linq;

namespace TameMyCerts.Models
{
    // Must be public due to XML serialization, otherwise 0x80131509 / System.InvalidOperationException
    [XmlRoot(ElementName = "DataExport")]
    public class DataExportModel
    {
        [XmlElement(ElementName = "RequestDatabaseRow")]
        public RequestDatabaseRowType RequestDatabaseRow { get; set; }
        [XmlElement(ElementName = "CertificateRequestValidationResult")]
        public CertificateRequestValidationResultType CertificateRequestValidationResult { get; set; }

        [XmlArray(ElementName = "ReplaceTokenValues")]
        [XmlArrayItem(ElementName = "KeyValuePair")]
        public List<SerializableKeyValuePair> ReplaceTokenValuesXML { get; set; } = new List<SerializableKeyValuePair>();
        [XmlIgnore]
        public Dictionary<string, string> ReplaceTokenValues
        {
            get
            {
                return this.ReplaceTokenValuesXML is not null ? this.ReplaceTokenValuesXML.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : new Dictionary<string, string>();
            }
        }

        public class CertificateRequestValidationResultType
        {
            [XmlElement(ElementName = "NotBefore")]
            public DateTimeOffset NotBefore { get; set; }
            [XmlElement(ElementName = "NotAfter")]
            public DateTimeOffset NotAfter { get; set; }
            [XmlElement(ElementName = "StatusCode")]
            public int StatusCode { get; set; }
            [XmlIgnore]
            public bool DeniedForIssuance => StatusCode != WinError.ERROR_SUCCESS;
            [XmlArray(ElementName = "Description")]
            [XmlArrayItem(ElementName = "string")]
            public List<string> Description { get; set; }
            [XmlArray(ElementName = "Warning")]
            [XmlArrayItem(ElementName = "string")]
            public List<string> Warning { get; set; }

            public CertificateRequestValidationResultType()
            { }
            internal CertificateRequestValidationResultType(CertificateRequestValidationResult validationResult)
            {
                NotBefore = validationResult.NotBefore;
                NotAfter = validationResult.NotAfter;
                StatusCode = validationResult.StatusCode;
                Description = validationResult.Description;
                //CertificateExtensions = validationResult.CertificateExtensions;
                //SubjectAlternativeNameExtension = validationResult.SubjectAlternativeNameExtension;
            }
        }

        public class RequestDatabaseRowType
        {
            [XmlElement(ElementName = "RequestID")]
            public int RequestID { get; set; }
            [XmlElement(ElementName = "NotBefore")]
            public DateTimeOffset NotBefore { get; set; }
            [XmlElement(ElementName = "NotAfter")]
            public DateTimeOffset NotAfter { get; set; }
            [XmlElement(ElementName = "RequestAttributes")]
            Dictionary<string, string> OriginalRequestAttributes { get; set; }
            [XmlElement(ElementName = "CertificateExtensions")]
            Dictionary<string, byte[]> CertificateExtensions { get; set; }
            [XmlElement(ElementName = "KeyAlgorithm")]
            public KeyAlgorithmFamily KeyAlgorithm { get; set; }
            [XmlArray(ElementName = "SubjectRelativeDistinguishedNames")]
            [XmlArrayItem(ElementName = "KeyValuePair")]
            public List<SerializableKeyValuePair> SubjectRelativeDistinguishedNames { get; set; }
            [XmlElement(ElementName = "CertificateTemplate")]
            public string CertificateTemplate { get; set; }
            public RequestDatabaseRowType()
            { }
            internal RequestDatabaseRowType(CertificateDatabaseRow databaseRow)
            {
                RequestID = databaseRow.RequestID;
                NotBefore = databaseRow.NotBefore;
                NotAfter = databaseRow.NotAfter;
                OriginalRequestAttributes = databaseRow.RequestAttributes;
                CertificateExtensions = databaseRow.CertificateExtensions;
                KeyAlgorithm = databaseRow.KeyAlgorithm;
                SubjectRelativeDistinguishedNames = databaseRow.SubjectRelativeDistinguishedNames.Select(kvp => new SerializableKeyValuePair { Key = kvp.Key, Value = kvp.Value }).ToList();
                CertificateTemplate = databaseRow.CertificateTemplate;
            }
        }

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
            var xmlSerializer = new XmlSerializer(typeof(DataExportModel));

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
            var xmlSerializer = new XmlSerializer(typeof(DataExportModel));

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

        public static DataExportModel LoadFromFile(string path)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(DataExportModel));
            //xmlSerializer.UnknownElement += new XmlElementEventHandler(UnknownElementHandler);
            //xmlSerializer.UnknownAttribute += new XmlAttributeEventHandler(UnknownAttributeHandler);

            using (StreamReader reader = new StreamReader(path))
            {
                return (DataExportModel)xmlSerializer.Deserialize(reader.BaseStream);
            }
        }

        public class SerializableKeyValuePair
        {
            [XmlAttribute("Key")]
            public string Key { get; set; }

            [XmlAttribute("Value")]
            public string Value { get; set; }

            public SerializableKeyValuePair() { } // Parameterless constructor for XML serialization

            public SerializableKeyValuePair(string key, string value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}