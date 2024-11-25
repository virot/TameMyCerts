// Copyright 2021-2024 Uwe Gradenegger <uwe@gradenegger.eu>
// Copyright 2024 Oscar Virot <virot@virot.com>

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at

// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using TameMyCerts.Enums;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Runtime.CompilerServices;

namespace TameMyCerts.Models
{
    // Must be public due to XML serialization, otherwise 0x80131509 / System.InvalidOperationException
    [XmlRoot(ElementName = "Notifyer")]
    public class NotifyerPolicy
    {
        [XmlElement(ElementName = "NotifyOnSuccess")]
        public Boolean NotifyOnSuccess { get; set; }
        [XmlElement(ElementName = "NotifyOnFailure")]
        public Boolean NotifyOnFailure { get; set; }

        [XmlElement(ElementName = "MailTo")]
        public string MailTo { get; set; }
        [XmlElement(ElementName = "MailFrom")]
        public string MailFrom { get; set; }
        [XmlArray(ElementName = "MailBCC")]
        [XmlArrayItem(ElementName = "string")]
        public List<string> MailBCC { get; set; } = new List<string>();
        [XmlArray(ElementName = "MailCC")]
        [XmlArrayItem(ElementName = "string")]
        public List<string> MailCC { get; set; } = new List<string>();

        [XmlElement(ElementName = "MailServer")]
        public string MailServer { get; set; }
        [XmlElement(ElementName = "MailPort")]
        public int MailPort { get; set; } = 25;
        [XmlElement(ElementName = "MailUseSSL")] 
        public Boolean MailUseSSL { get; set; }

        [XmlElement(ElementName = "MailSendUser")]
        public string MailSendUser { get; set; }

        [XmlElement(ElementName = "MailSendPassword")]
        public string MailSendPassword { get; set; }

        [XmlElement(ElementName = "MailSuccess")]
        public SuccessMail MailSuccess { get; set; }
        [XmlElement(ElementName = "MailFailure")]
        public SuccessMail MailFailure { get; set; }

        [XmlRoot(ElementName = "MailSuccess")]
        public class SuccessMail
        {
            [XmlElement(ElementName = "MailSubject")]
            public string MailSubject { get; set; }
            [XmlElement(ElementName = "MailBody")]
            public string MailBody { get; set; }
        };
        public class FailureMail
        {
            [XmlElement(ElementName = "MailSubject")]
            public string MailSubject { get; set; }
            [XmlElement(ElementName = "MailBody")]
            public string MailBody { get; set; }
        };

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
            var xmlSerializer = new XmlSerializer(typeof(NotifyerPolicy));

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
            var xmlSerializer = new XmlSerializer(typeof(NotifyerPolicy));

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