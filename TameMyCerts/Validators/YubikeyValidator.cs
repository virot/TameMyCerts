// Copyright 2021-2023 Uwe Gradenegger <uwe@gradenegger.eu>

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
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using TameMyCerts.ClassExtensions;
using TameMyCerts.Enums;
using TameMyCerts.Models;
using TameMyCerts.X509;

namespace TameMyCerts.Validators
{
    /// <summary>
    ///     This validator will check that the CSR is issued by a real Yubikey
    /// </summary>
    internal class YubikeyValidator
    {
        private CertificateRequest _CertificateRequest;
 
        public CertificateRequestValidationResult VerifyRequest(CertificateRequestValidationResult result,
            CertificateRequestPolicy policy, YubikeyObject yubikey)
        {

            if (result.DeniedForIssuance)
            {
                return result;
            }
            try
            {
                //_CertificateRequest.
                //_CertificateRequest. .LoadSigningRequest(certificateRequest)
            }
            catch (Exception ex)
            {
                result.SetFailureStatus(WinError.CERTSRV_E_TEMPLATE_DENIED, ex.Message);
            }

            return result;
        }
        public CertificateRequestValidationResult ExtractAttestion(CertificateRequestValidationResult result,
            CertificateRequestPolicy policy, CertificateDatabaseRow dbRow, out YubikeyObject yubikey)
        {
            //YubikeyObject yubikeyTemp = new YubikeyObject();

            if (dbRow.CertificateExtensions.ContainsKey(YubikeyOID.ATTESTION_DEVICE) && dbRow.CertificateExtensions.ContainsKey(YubikeyOID.ATTESTION_INTERMEDIATE))
            {
                Console.WriteLine("Update the yubikeyObject");
                dbRow.CertificateExtensions.TryGetValue(YubikeyOID.ATTESTION_DEVICE, out var AttestionCertificate);
                dbRow.CertificateExtensions.TryGetValue(YubikeyOID.ATTESTION_INTERMEDIATE, out var IntermediateCertificate);
                yubikey = new YubikeyObject("", AttestionCertificate, IntermediateCertificate);
            }
            else
            {
                yubikey = new YubikeyObject();
            }
            if (result.DeniedForIssuance || null == policy.YubikeyRequirement)
            {
                return result;
            }
            if (dbRow.CertificateExtensions.ContainsKey(YubikeyOID.ATTESTION_DEVICE))
            {
                Console.WriteLine("Yubikey Device Attestation found");
            }
            if (dbRow.CertificateExtensions.ContainsKey(YubikeyOID.ATTESTION_INTERMEDIATE))
            {
                Console.WriteLine("Yubikey Intermediate Attestation found");
            }
            

            try
            {
                //_CertificateRequest.
                //_CertificateRequest. .LoadSigningRequest(certificateRequest)
            }
            catch (Exception ex)
            {
                result.SetFailureStatus(WinError.CERTSRV_E_TEMPLATE_DENIED, ex.Message);
            }

            yubikey = new YubikeyObject();
            return result;
        }
    }
}