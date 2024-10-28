using System.Collections.Generic;
using System.Xml.Serialization;
using System;

namespace TameMyCerts.Models
{
    // Must be public due to XML serialization, otherwise 0x80131509 / System.InvalidOperationException
    [XmlRoot(ElementName = "AdvancedADPolicy")]
    public class AdvancedADPolicy
    {
        [XmlArray(ElementName = "GroupsMembership")]
        [XmlArrayItem(ElementName = "string")]
        public List<string> GroupMembership { get; set; } = new List<string>();

        [XmlArray(ElementName = "OrganizationalUnits")]
        [XmlArrayItem(ElementName = "string")]
        public List<string> OrganizationalUnits { get; set; } = new List<string>();

        [XmlElement(ElementName = "PermitDisabledAccounts")]
        public bool PermitDisabledAccounts { get; set; }
    }
}