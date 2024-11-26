using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Principal;
using Xunit;
using TameMyCerts.Enums;
using TameMyCerts.Models;
using TameMyCerts.Validators;
using Xunit.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;
using netDumbster.smtp;
using TameMyCerts.Submodules;
using System.Globalization;
using System.Threading;

namespace TameMyCerts.Tests
{
    public class NotifyerTests
    {
        private readonly CertificateRequestPolicy _policy;
        private readonly ActiveDirectoryObject _dsObject;
        private readonly ITestOutputHelper output;
        private static SimpleSmtpServer _smtpserver;
        private readonly string _defaultCsr;


        private EWTLoggerListener _listener;

        public NotifyerTests(ITestOutputHelper output)
        {
            _policy = new CertificateRequestPolicy {
                Notifyer = new NotifyerPolicy
                {
                    NotifyOnSuccess = false,
                    NotifyOnFailure = false,
                    MailFrom = "UnitTests <unittest@example.invalid>",
                    MailPort = 2525,
                    MailServer = "localhost",
                    MailUseSSL = false,
                    MailSuccess = new NotifyerPolicy.SuccessMail
                    {
                        MailBody = "Success",
                        MailSubject = "Success"
                    },
                    MailFailure = new NotifyerPolicy.SuccessMail
                    {
                        MailBody = "Failure",
                        MailSubject = "Failure"
                    }
                }
            };

            _dsObject = new ActiveDirectoryObject(
    "CN=rudi,OU=Test-Users,DC=intra,DC=adcslabor,DC=de",
    0,
    new List<string> { "CN=PKI_UserCert,OU=ADCSLabor Gruppen,DC=intra,DC=adcslabor,DC=de" },
    new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
    {
                { "c", "DE" },
                { "company", "ADCS Labor" },
                { "displayName", "Rudi Ratlos" },
                { "department", "IT Operations" },
                { "givenName", "Rudi" },
                { "initials", "RR" },
                { "l", "München" },
                { "mail", "rudi@adcslabor.de" },
                { "name", "rudi" },
                { "sAMAccountName", "rudi" },
                { "sn", "Ratlos" },
                { "st", "Bavaria" },
                // Note that streetAddress is left out intentionally
                { "title", "General Manager" },
                { "userPrincipalName", "rudi@intra.adcslabor.de" },
                { "extensionAttribute1", "rudi1@intra.adcslabor.de" },
                { "extensionAttribute2", "rudi2@intra.adcslabor.de" }
    },
    new SecurityIdentifier("S-1-5-21-1381186052-4247692386-135928078-1225"),
    new List<string>()
);

            _defaultCsr =
            "-----BEGIN NEW CERTIFICATE REQUEST-----\n" +
            "MIIDbTCCAlUCAQAwIDEeMBwGA1UEAxMVaW50cmFuZXQuYWRjc2xhYm9yLmRlMIIB\n" +
            "IjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEApucZpFuF0+fvdL5C3jggO6vO\n" +
            "9PA39MnPG0VQBy1n2pdhD/WwIt3St6UuMTXyNzEqSqm396Dw6+1iLCcP4DioLywd\n" +
            "9rVHOAFmYNeahM24rYk9z+8rgx5a4GhtK6uSXD87aNDwz7l+QCnjapZu1bqfe/s+\n" +
            "Wzo3e/jiSNIUUiY6/DQnHcZpPn/nBruLih0muZFWCevIRwu/w05DMrX9KTKax06l\n" +
            "TJw+bQshKasiVDDW+0K5eDzvLu7cS6/Z9vVYHD7gGJNmX+YaJY+JS9tGaGyvDUiV\n" +
            "ww+Do5S8p13dXqY/xwMngkq3kkvTB8hstxE1pd07OQojZ1SaLFEyh3pX7abXMQID\n" +
            "AQABoIIBBjAcBgorBgEEAYI3DQIDMQ4WDDEwLjAuMTkwNDQuMjA+BgkqhkiG9w0B\n" +
            "CQ4xMTAvMA4GA1UdDwEB/wQEAwIHgDAdBgNVHQ4EFgQUsp05C4spRvndIOKWrM7O\n" +
            "aXVZLCUwPgYJKwYBBAGCNxUUMTEwLwIBBQwKb3R0aS1vdHRlbAwOT1RUSS1PVFRF\n" +
            "TFx1d2UMDnBvd2Vyc2hlbGwuZXhlMGYGCisGAQQBgjcNAgIxWDBWAgEAHk4ATQBp\n" +
            "AGMAcgBvAHMAbwBmAHQAIABTAG8AZgB0AHcAYQByAGUAIABLAGUAeQAgAFMAdABv\n" +
            "AHIAYQBnAGUAIABQAHIAbwB2AGkAZABlAHIDAQAwDQYJKoZIhvcNAQELBQADggEB\n" +
            "ABCVBVb7DJjiDP5SbSpw08nvrwnx5kiQ21xR7AJmtSYPLmsmC7uIPxk8Jsq1hDUO\n" +
            "e2adcbMup6QY7GJGuc4OWhiaisKAeZB7Tcy5SEZIWe85DlkxEgLVFB9opmf+V3fA\n" +
            "d/ZtYS0J7MPg6F9UEra30T3CcHlH5Y8NlMtaZmqjfXyw2C5YkahEfSmk2WVaZiSf\n" +
            "8edZDjIw5eRZY/9QMi2JEcmSbq0DImiP4ou46aQ0U5iRGSNX+armMIhGJ1ycDXTM\n" +
            "SBDUN6qWGioX8NHTlUmebLijw3zSFMnIuYWhXF7FZ1IKMPySzVmquvBAjzT4kWSw\n" +
            "0bAr5OaOzHm7POogsgE8J1Y=\n" +
            "-----END NEW CERTIFICATE REQUEST-----";

            this.output = output;
            this._listener = new EWTLoggerListener();
        }

        internal void PrintResult(CertificateRequestValidationResult result)
        {
            output.WriteLine("0x{0:X} ({0}) {1}.", result.StatusCode,
                new Win32Exception(result.StatusCode).Message);
            output.WriteLine(string.Join("\n", result.Description));
        }
       


        [Fact]
        public void Validate_Notifyer_MailTo()
        {
            _smtpserver = SimpleSmtpServer.Start(2525);

            var dbRow = new CertificateDatabaseRow(_defaultCsr, CertCli.CR_IN_PKCS10);
            CertificateRequestPolicy policy = _policy;
            policy.Notifyer.MailTo = "{ad:mail}";
            policy.Notifyer.NotifyOnSuccess = true;
            
            CertificateRequestValidationResult result = new CertificateRequestValidationResult(dbRow);

            NotifyerSubModule.Notify(1001, "UnitTest", result, policy.Notifyer, _dsObject);

            Assert.Equal(1, _smtpserver.ReceivedEmailCount);
            Assert.Contains("rudi@adcslabor.de", _smtpserver.ReceivedEmail[0].ToAddresses.Select(a => a.Address));
            _smtpserver.Stop();
            _smtpserver = null;

        }

        [Fact]
        public void Validate_Notifyer_Failure_Reason()
        {
            _smtpserver = SimpleSmtpServer.Start(2525);

            var dbRow = new CertificateDatabaseRow(_defaultCsr, CertCli.CR_IN_PKCS10);
            CertificateRequestPolicy policy = _policy;
            policy.Notifyer.MailTo = "{ad:mail}";
            policy.Notifyer.NotifyOnFailure = true;
            policy.Notifyer.MailFailure.MailSubject = "Certificate request {vr:requestid} is {vr:status}";
            policy.Notifyer.MailFailure.MailBody = "The certificate request was {vr:status} due to {vr:reason}";
            
            CertificateRequestValidationResult result = new CertificateRequestValidationResult(dbRow);
            result.SetFailureStatus(WinError.CERTSRV_E_TEMPLATE_DENIED, string.Format(LocalizedStrings.DirVal_Account_Password_to_old, 1337));

            NotifyerSubModule.Notify(1002, "UnitTest", result, policy.Notifyer, _dsObject);

            Assert.Equal(1, _smtpserver.ReceivedEmailCount);
            Assert.Contains("rudi@adcslabor.de", _smtpserver.ReceivedEmail[0].ToAddresses.Select(a => a.Address));
            Assert.Contains("1337", _smtpserver.ReceivedEmail[0].MessageParts[0].BodyData);
            Assert.Contains("Certificate request 1002 is Denied", _smtpserver.ReceivedEmail[0].Subject);

            _smtpserver.Stop();
            _smtpserver = null;


        }

        [Fact]
        public void Validate_Fail_on_missing_recipient_missing()
        {
            //_smtpserver = SimpleSmtpServer.Start(2525);
            _listener.ClearEvents();

            var dbRow = new CertificateDatabaseRow(_defaultCsr, CertCli.CR_IN_PKCS10);
            CertificateRequestPolicy policy = _policy;
            policy.Notifyer.NotifyOnSuccess = true;

            CertificateRequestValidationResult result = new CertificateRequestValidationResult(dbRow);

            NotifyerSubModule.Notify(1003, "UnitTest", result, policy.Notifyer, _dsObject);

            //Assert.Equal(0, _smtpserver.ReceivedEmailCount);
            Assert.Contains(4401, _listener.Events.Select(e => e.EventId));
            //_smtpserver.Stop();
            //_smtpserver = null;
        }

        [Fact]
        public void Validate_Fail_on_incorrect_recipient()
        {
            //_smtpserver = SimpleSmtpServer.Start(2525);
            _listener.ClearEvents();

            var dbRow = new CertificateDatabaseRow(_defaultCsr, CertCli.CR_IN_PKCS10);
            CertificateRequestPolicy policy = _policy;
            policy.Notifyer.NotifyOnSuccess = true;
            policy.Notifyer.MailTo = "mail@example@invalid";

            CertificateRequestValidationResult result = new CertificateRequestValidationResult(dbRow);

            NotifyerSubModule.Notify(1004, "UnitTest", result, policy.Notifyer, _dsObject);

            //Assert.Equal(0, _smtpserver.ReceivedEmailCount);
            Assert.Contains(4403, _listener.Events.Select(e => e.EventId));
            //_smtpserver.Stop();
            //_smtpserver = null;
        }
    }
}