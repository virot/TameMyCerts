using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Principal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TameMyCerts.Enums;
using TameMyCerts.Models;
using TameMyCerts.Validators;

namespace TameMyCerts.Tests
{
    [TestClass]
    public class YubikeyValidatorTests
    {
        private readonly CertificateDatabaseRow _dbRow;
        private readonly ActiveDirectoryObject _dsObject;
        private readonly ActiveDirectoryObject _dsObject2;
        private readonly CertificateRequestPolicy _policy;
        private readonly YubikeyValidator _validator = new YubikeyValidator();

        public YubikeyValidatorTests()
        {
            // 2048 Bit RSA Key
            // CN=intranet.adcslabor.de
            var request =
                "-----BEGIN CERTIFICATE REQUEST-----\n" +
"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\n" +
"Oq0a7fphvaY3PqAsU4JOFVw55ukrXnUSof+z\n" +
"-----END CERTIFICATE REQUEST-----\n";


             _policy = new CertificateRequestPolicy {
                 YubikeyRequirement = new YubikeyRequirement
                 {
                            AllowedPinPolicies = new List<string>
                                    { "Always" }
                        }
             };

            _dbRow = new CertificateDatabaseRow(request, CertCli.CR_IN_PKCS10);
        }

        internal void PrintResult(CertificateRequestValidationResult result)
        {
            Console.WriteLine("0x{0:X} ({0}) {1}.", result.StatusCode,
                new Win32Exception(result.StatusCode).Message);
            Console.WriteLine(string.Join("\n", result.Description));
        }

        [TestMethod]
        public void Extract_Yubikey_Attestion()
        {
            var result = new CertificateRequestValidationResult(_dbRow);
            result = _validator.ExtractAttestion(result, _policy, _dbRow, out var yubikey);

            PrintResult(result);

            Assert.IsFalse(result.DeniedForIssuance);
            //Assert.IsTrue(result.StatusCode.Equals(WinError.NTE_FAIL));
        }

    }
}