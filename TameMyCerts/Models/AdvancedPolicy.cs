using System.Collections.Generic;
using System.Xml.Serialization;
using System;

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
    }
}