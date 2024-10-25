﻿// Copyright 2021-2024 Uwe Gradenegger <uwe@gradenegger.eu>
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

using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using TameMyCerts.Enums;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace TameMyCerts.Models
{
    internal class YubikeyObject
    {
        private const StringComparison COMPARISON = StringComparison.InvariantCultureIgnoreCase;

        private static X509Certificate2 YubikeyValidationCA = new X509Certificate2(new byte[] { 0x30, 0x82, 0x3, 0x17, 0x30, 0x82, 0x1, 0xFF, 0xA0, 0x3, 0x2, 0x1, 0x2, 0x2, 0x3, 0x4, 0x6, 0x47, 0x30, 0xD, 0x6, 0x9, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0xD, 0x1, 0x1, 0xB, 0x5, 0x0, 0x30, 0x2B, 0x31, 0x29, 0x30, 0x27, 0x6, 0x3, 0x55, 0x4, 0x3, 0xC, 0x20, 0x59, 0x75, 0x62, 0x69, 0x63, 0x6F, 0x20, 0x50, 0x49, 0x56, 0x20, 0x52, 0x6F, 0x6F, 0x74, 0x20, 0x43, 0x41, 0x20, 0x53, 0x65, 0x72, 0x69, 0x61, 0x6C, 0x20, 0x32, 0x36, 0x33, 0x37, 0x35, 0x31, 0x30, 0x20, 0x17, 0xD, 0x31, 0x36, 0x30, 0x33, 0x31, 0x34, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x5A, 0x18, 0xF, 0x32, 0x30, 0x35, 0x32, 0x30, 0x34, 0x31, 0x37, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x5A, 0x30, 0x2B, 0x31, 0x29, 0x30, 0x27, 0x6, 0x3, 0x55, 0x4, 0x3, 0xC, 0x20, 0x59, 0x75, 0x62, 0x69, 0x63, 0x6F, 0x20, 0x50, 0x49, 0x56, 0x20, 0x52, 0x6F, 0x6F, 0x74, 0x20, 0x43, 0x41, 0x20, 0x53, 0x65, 0x72, 0x69, 0x61, 0x6C, 0x20, 0x32, 0x36, 0x33, 0x37, 0x35, 0x31, 0x30, 0x82, 0x1, 0x22, 0x30, 0xD, 0x6, 0x9, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0xD, 0x1, 0x1, 0x1, 0x5, 0x0, 0x3, 0x82, 0x1, 0xF, 0x0, 0x30, 0x82, 0x1, 0xA, 0x2, 0x82, 0x1, 0x1, 0x0, 0xC3, 0x76, 0x70, 0xC4, 0xCD, 0x47, 0xA6, 0x2, 0x75, 0xC4, 0xC5, 0x47, 0x1B, 0x8F, 0xCB, 0x7D, 0x4F, 0x69, 0xB4, 0x67, 0xE6, 0x6E, 0xA9, 0x27, 0xE9, 0xD2, 0x13, 0x41, 0xD1, 0x5A, 0x9A, 0x1A, 0x33, 0xC7, 0xDC, 0xF3, 0x1, 0xC2, 0xF9, 0x39, 0x9B, 0xF7, 0xC8, 0xE6, 0x36, 0xF8, 0x56, 0x34, 0x4D, 0x84, 0x8A, 0x55, 0x3C, 0xE6, 0xE6, 0xA, 0x7C, 0x41, 0x4F, 0xF5, 0xDE, 0x90, 0xD8, 0x69, 0xB2, 0xB6, 0xA0, 0x67, 0xC5, 0x9B, 0x0, 0x6B, 0x72, 0xAA, 0x66, 0x20, 0x82, 0xC7, 0x62, 0xF0, 0x43, 0x88, 0x98, 0x10, 0xE6, 0xF5, 0x96, 0x58, 0x28, 0xB5, 0x5A, 0xFF, 0xC2, 0x11, 0x29, 0x75, 0x53, 0xAA, 0x8E, 0x85, 0x34, 0x3F, 0x97, 0xB5, 0x8F, 0x5C, 0xBB, 0x39, 0xFC, 0xE, 0xBE, 0x4C, 0xBF, 0xF8, 0x5, 0xC8, 0x37, 0xFF, 0x57, 0xA7, 0x45, 0x45, 0x95, 0x84, 0x64, 0xDA, 0xD4, 0x3D, 0x19, 0xC7, 0x58, 0x28, 0x39, 0xAA, 0x53, 0xE7, 0x5B, 0xF6, 0x22, 0xB0, 0xA4, 0xC, 0xE2, 0x77, 0x8A, 0x7, 0x5, 0x52, 0xC8, 0x86, 0x60, 0xF7, 0xA6, 0xF9, 0x16, 0x69, 0x10, 0x36, 0x1F, 0x70, 0xC0, 0xF6, 0xDE, 0xC7, 0xFC, 0x73, 0x6A, 0xE6, 0xFD, 0xCE, 0x88, 0xED, 0x63, 0xC8, 0xB6, 0x5E, 0x2A, 0xA6, 0x68, 0x31, 0xB3, 0xCE, 0x6E, 0xBC, 0x6A, 0xE, 0xF, 0xBD, 0x7C, 0xE7, 0x52, 0x87, 0x38, 0x1F, 0xC0, 0x2A, 0xA0, 0x4F, 0x75, 0xD5, 0x99, 0x37, 0xA2, 0xC2, 0xF0, 0x52, 0x4D, 0xCB, 0x72, 0x8B, 0xD9, 0x87, 0x41, 0xF6, 0x1D, 0xD8, 0x3C, 0x24, 0x6A, 0xAC, 0x51, 0x9C, 0xB6, 0xCD, 0x57, 0x22, 0xBD, 0xCE, 0x5F, 0x83, 0xCE, 0x34, 0x86, 0xA7, 0xD2, 0x21, 0x54, 0xF8, 0x95, 0xB4, 0x67, 0xAD, 0x5F, 0x4D, 0x9D, 0xC6, 0x14, 0x27, 0x19, 0x2E, 0xCA, 0xE8, 0x13, 0xB4, 0x41, 0xEF, 0x2, 0x3, 0x1, 0x0, 0x1, 0xA3, 0x42, 0x30, 0x40, 0x30, 0x1D, 0x6, 0x3, 0x55, 0x1D, 0xE, 0x4, 0x16, 0x4, 0x14, 0xCA, 0x5F, 0xCA, 0xF2, 0xC4, 0xA2, 0x31, 0x9C, 0xE9, 0x22, 0x5F, 0xF1, 0xEC, 0xF4, 0xD5, 0xDF, 0x2, 0xBF, 0x83, 0xBF, 0x30, 0xF, 0x6, 0x3, 0x55, 0x1D, 0x13, 0x4, 0x8, 0x30, 0x6, 0x1, 0x1, 0xFF, 0x2, 0x1, 0x1, 0x30, 0xE, 0x6, 0x3, 0x55, 0x1D, 0xF, 0x1, 0x1, 0xFF, 0x4, 0x4, 0x3, 0x2, 0x1, 0x6, 0x30, 0xD, 0x6, 0x9, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0xD, 0x1, 0x1, 0xB, 0x5, 0x0, 0x3, 0x82, 0x1, 0x1, 0x0, 0x5C, 0xEC, 0x88, 0x7C, 0x5, 0xCD, 0x5F, 0x90, 0x2F, 0x85, 0xC8, 0xDD, 0x5F, 0x86, 0x35, 0xA2, 0xA0, 0x10, 0x8C, 0xAF, 0x7B, 0xE3, 0x9D, 0xE8, 0x7B, 0x30, 0xB6, 0xC0, 0xEA, 0x44, 0xA8, 0xC9, 0x61, 0x7B, 0xD0, 0xDD, 0xEC, 0x5E, 0x16, 0xD7, 0xBD, 0x3E, 0x1E, 0x46, 0x1D, 0x21, 0xBF, 0x1A, 0xAF, 0x31, 0x93, 0x63, 0x3D, 0x4F, 0xD5, 0x95, 0x19, 0xFA, 0x80, 0xB5, 0x6D, 0xA0, 0x48, 0xA4, 0xC, 0xBA, 0xD8, 0x15, 0x73, 0x7A, 0x1E, 0x1E, 0x96, 0x9B, 0x2C, 0xB5, 0x19, 0x39, 0xEC, 0xA6, 0x73, 0xAF, 0x32, 0xFC, 0xF6, 0x94, 0xB2, 0xAE, 0xCA, 0x6F, 0x4A, 0x61, 0xD6, 0xB, 0xE, 0x9, 0xE3, 0xDC, 0x17, 0x80, 0xBF, 0x32, 0x21, 0x57, 0x3C, 0xD8, 0x49, 0xE5, 0x3B, 0xEF, 0xF0, 0xAE, 0xA6, 0x87, 0xE3, 0xD3, 0xDD, 0xCE, 0xB8, 0xB, 0x30, 0x5B, 0x48, 0xD8, 0xBD, 0x7B, 0x6, 0x4F, 0x28, 0xB1, 0xE8, 0x1D, 0xDD, 0x6D, 0x6E, 0x72, 0x5A, 0xFC, 0x92, 0xF7, 0x33, 0x57, 0x6A, 0xA1, 0x9A, 0x52, 0x63, 0xF7, 0x53, 0xDF, 0xDB, 0xE8, 0x39, 0x47, 0x74, 0x3A, 0x20, 0x30, 0xBB, 0xB7, 0x54, 0xBA, 0x41, 0x7, 0xD6, 0xE6, 0xE5, 0xB8, 0xDA, 0x29, 0x65, 0x89, 0x62, 0x5, 0xA5, 0xB4, 0x25, 0x60, 0x51, 0xB1, 0x6A, 0x16, 0xAC, 0xA2, 0xE3, 0xE2, 0x44, 0xD3, 0x5E, 0x1C, 0x4A, 0x4, 0x79, 0xEC, 0x97, 0x2E, 0xDD, 0xD6, 0x62, 0x7A, 0x10, 0x7A, 0x52, 0xD0, 0xF, 0x81, 0xA7, 0x7D, 0x2F, 0x97, 0xD, 0xBE, 0xE6, 0xBF, 0x21, 0x64, 0x66, 0x9B, 0xE0, 0xD, 0xCB, 0x73, 0xB6, 0x2C, 0x7F, 0xBE, 0x3F, 0x29, 0x7C, 0x49, 0x11, 0x33, 0x53, 0xCA, 0x27, 0x6C, 0x1B, 0x23, 0x32, 0xF, 0x50, 0xE, 0x24, 0x9F, 0xE6, 0x82, 0x4B, 0x2A, 0xF7, 0x7F, 0x45, 0xE9, 0xFE, 0xCC, 0x66, 0x3B });

        public Dictionary<string, string> Attributes { get; } =
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        public YubikeyObject()
        {
        }
        public YubikeyObject(byte[] publicKey, X509Certificate2 AttestationCertificate, X509Certificate2 IntermediateCertificate)
        {

            
            // This is simple in newer .NET versions, where we can load a custom CA Root. .NET Framework does not support this.
            // This is cluncky.

            X509Chain chain = new X509Chain();
            chain.ChainPolicy.ExtraStore.Add(YubikeyValidationCA);
            chain.ChainPolicy.ExtraStore.Add(IntermediateCertificate);
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

            if (! (chain.Build(AttestationCertificate)))
            {
                throw new Exception("Failed to build certificate path");
            }
            if (chain.ChainElements.Count != 3)
            {
                throw new Exception("Failed to build certificate path");
            }
            if (Convert.ToBase64String(chain.ChainElements[2].Certificate.PublicKey.EncodedKeyValue.RawData) != Convert.ToBase64String(YubikeyValidationCA.PublicKey.EncodedKeyValue.RawData))
            {
                throw new Exception("Certificate path does not end up at Yubikey CA");
            }
            if (! (publicKey.SequenceEqual(chain.ChainElements[0].Certificate.PublicKey.EncodedKeyValue.RawData)))
            {
                throw new Exception("Certificate CSR does not match attestion certificate");
            }

            #region Slot
            // the Slot number is located in the Subject for the Attestion certificate.

            Regex slotRegex = new Regex(attestionSlotPattern);
            Match slotMatch = slotRegex.Match(AttestationCertificate.Subject);
            if (slotMatch.Success)
            {
                this.Slot = slotMatch.Groups["slot"].Value;
            }
            #endregion
            #region PIN / touch policy
            byte[] PinTouchPolicy = AttestationCertificate.Extensions.Cast<X509Extension>().FirstOrDefault(x => x.Oid.Value == "1.3.6.1.4.1.41482.3.8")?.RawData;
            if ( PinTouchPolicy.Length == 2)
            {
                // Staring with the PIN Policy
                switch (PinTouchPolicy[0])
                {
                    case 0:
                        this.PinPolicy = "None";
                        break;
                    case 1:
                        this.PinPolicy = "Never";
                        break;
                    case 2:
                        this.PinPolicy = "Once";
                        break;
                    case 3:
                        this.PinPolicy = "Always";
                        break;
                    case 4:
                        this.PinPolicy = "MatchOnce";
                        break;
                    case 5:
                        this.PinPolicy = "MatchAlways";
                        break;
                }
                // Update the TouchPolicy
                switch (PinTouchPolicy[1])
                {
                    case 0:
                        this.TouchPolicy = "None";
                        break;
                    case 1:
                        this.TouchPolicy = "Never";
                        break;
                    case 2:
                        this.TouchPolicy = "Always";
                        break;
                    case 3:
                        this.TouchPolicy = "Cached";
                        break;
                }
            }
            #endregion
            #region FormFactor
            byte FormFactor = AttestationCertificate.Extensions.Cast<X509Extension>().FirstOrDefault(x => x.Oid.Value == YubikeyOID.FORMFACTOR)?.RawData[0] ?? 0;
            switch (FormFactor)
            {
                case 1:
                    this.FormFactor = "UsbAKeychain";
                    break;
                case 2:
                    this.FormFactor = "UsbANano";
                    break;
                case 3:
                    this.FormFactor = "UsbCKeychain";
                    break;
                case 4:
                    this.FormFactor = "UsbCNano";
                    break;
                case 5:
                    this.FormFactor = "UsbCLightning";
                    break;
                case 6:
                    this.FormFactor = "UsbABiometricKeychain";
                    break;
                case 7:
                    this.FormFactor = "UsbCBiometricKeychain";
                    break;
                default:
                    this.FormFactor = "Unknown";
                    break;
            }
            #endregion
            #region Firmware Version
            // Update the Firmware Version
            byte[] FirmwareVersion = AttestationCertificate.Extensions.Cast<X509Extension>().FirstOrDefault(x => x.Oid.Value == YubikeyOID.FIRMWARE)?.RawData;
            if (FirmwareVersion.Length == 3)
            {
                this.FirmwareVersion = new Version(FirmwareVersion[0], FirmwareVersion[1], FirmwareVersion[2]);
            }
            #endregion
            #region Serial Number
            // Update the Serial Number
            byte[] SerialNumber = AttestationCertificate.Extensions.Cast<X509Extension>().FirstOrDefault(x => x.Oid.Value == YubikeyOID.SERIALNUMBER)?.RawData;
            if (! (SerialNumber is null))
            {
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(SerialNumber);
                }
                this.SerialNumber = BitConverter.ToUInt32(SerialNumber, 0).ToString();
            }
            #endregion

            // Add to the attributes to allow for replacement
            Attributes.Add("FormFactor", this.FormFactor);
            Attributes.Add("FirmwareVersion", this.FirmwareVersion.ToString());
            Attributes.Add("PinPolicy", this.PinPolicy);
            Attributes.Add("TouchPolicy", this.TouchPolicy);
            Attributes.Add("Slot", this.Slot);
            Attributes.Add("SerialNumber", this.SerialNumber);
        }

        private bool VerifyChain(X509Certificate2 Signed, X509Certificate2 Signer)
        {

            X509Chain chain = new X509Chain();
            

            // Get the raw data of the certificate's TBSCertificate
            byte[] tbsCertificate = Signed.GetRawCertData();

            // Get the signature of the certificate
            //byte[] signature = Signed.GetSignature();

            // Get the public key of the signing certificate
            //RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)Signer.PublicKey.Key;

            // Verify the signature
            //bool isSigned = rsa.VerifyData(tbsCertificate, CryptoConfig.MapNameToOID("SHA256"), signature);

            //Console.WriteLine($"Chain status length: {isSigned}");
            return true;
        }

        public string TouchPolicy { get; } = "";
        public string PinPolicy { get; } = "";
        public string FormFactor { get; } = "";
        public string Slot { get; } = "";
        public string SerialNumber { get; } = "";
        public Version FirmwareVersion { get; } = new Version(0, 0, 0);
        public bool? Validated { get; } = false;
        private static string attestionSlotPattern = @"CN=YubiKey PIV Attestation (?<slot>[0-9A-Fa-f]{2})";
 
    }
}