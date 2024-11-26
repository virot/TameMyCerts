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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using TameMyCerts.Models;
using TameMyCerts.Validators;

namespace TameMyCerts.Submodules
{
    /// <summary>
    ///     This submodule is resonsible for notifying the users with a more informative test.
    /// </summary>
    internal class NotifyerSubModule
    {
        private const StringComparison Comparison = StringComparison.InvariantCultureIgnoreCase;

        public static void Notify(int requestID, string template, CertificateRequestValidationResult result, NotifyerPolicy notifyerPolicy, ActiveDirectoryObject dsObject)
        {
            if (!notifyerPolicy.NotifyOnSuccess && !notifyerPolicy.NotifyOnFailure)
            {// No notification is requested
                return;
            }

            // Use EWT to dump the policy to debug if wanted
            EWTLogger.Log.Notifyer_4402_Debug_Notifyer_Policy(template, notifyerPolicy.SaveToString());


            // Start by going through the policy and check if we have the minimum amount of information.
            if (notifyerPolicy.MailTo is null || notifyerPolicy.MailTo.Length == 0)
            {
                EWTLogger.Log.Notifyer_4401_Missing_Required_Information(requestID, template, "MailTo");
                return;
            }
            if (!notifyerPolicy.MailFrom.Any())
            {
                EWTLogger.Log.Notifyer_4401_Missing_Required_Information(requestID, template, "MailFrom");
                return;
            }
            if (!notifyerPolicy.MailServer.Any())
            {
                EWTLogger.Log.Notifyer_4401_Missing_Required_Information(requestID, template, "MailServer");
                return;
            }

            string replacedMailTo = CertificateContentValidator.ReplaceTokenValues(notifyerPolicy.MailTo, "ad", null != dsObject ? dsObject.Attributes.ToList() : new List<KeyValuePair<string, string>>());

            MailMessage mailMessage = new MailMessage();
            try
            {
                // Build the email.
                mailMessage.From = new MailAddress(notifyerPolicy.MailFrom);
                mailMessage.To.Add(new MailAddress(replacedMailTo));
                mailMessage.Priority = MailPriority.Normal;


                if (result.DeniedForIssuance && notifyerPolicy.NotifyOnFailure == true)
                {
                    mailMessage.Subject = notifyerPolicy.MailFailure.MailSubject.Any() ? notifyerPolicy.MailFailure.MailSubject : "Certificate Issuance Failed";
                    mailMessage.Body = notifyerPolicy.MailFailure.MailBody.Any() ? notifyerPolicy.MailFailure.MailBody : "Certificate Issuance Failed";
                }
                else if (result.DeniedForIssuance == false && notifyerPolicy.NotifyOnSuccess == true)
                {
                    mailMessage.Subject = notifyerPolicy.MailSuccess.MailSubject.Any() ? notifyerPolicy.MailSuccess.MailSubject : "Certificate Issuance Succeded";
                    mailMessage.Body = notifyerPolicy.MailSuccess.MailBody.Any() ? notifyerPolicy.MailSuccess.MailBody : "Certificate Issuance Succeded";
                }
                else
                {
                    // We dont match any of the conditions to send an email.
                    return;
                }
            }
            catch (Exception ex)
            {
                EWTLogger.Log.Notifyer_4403_Failed_to_build_email(requestID, template, ex.ToString());
                return;
            }

            // Build a custom replacement list for the email template
            List<KeyValuePair<string, string>> vrAttributes = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("RequestID", requestID.ToString()),
                new KeyValuePair<string, string>("Template", template),
                new KeyValuePair<string, string>("Status", result.DeniedForIssuance ? "Denied" : "Approved"),
                new KeyValuePair<string, string>("Reason", result.Description.Any() ? string.Join("\n", result.Description.Distinct().ToList()): "No reason given")
            };

            // Replace vrAttributes in the email templates data
            mailMessage.Subject = CertificateContentValidator.ReplaceTokenValues(mailMessage.Subject, "vr", vrAttributes);
            mailMessage.Body = CertificateContentValidator.ReplaceTokenValues(mailMessage.Body, "vr", vrAttributes);


            try
            {
                using (SmtpClient MailClient = new SmtpClient())
                {
                    MailClient.Host = notifyerPolicy.MailServer;
                    MailClient.Port = notifyerPolicy.MailPort;
                    MailClient.EnableSsl = notifyerPolicy.MailUseSSL;
                    //if (notifyerPolicy.MailSendUser.Any() && notifyerPolicy.MailSendPassword.Any())
                    //{
                    //    MailClient.Credentials = new System.Net.NetworkCredential(notifyerPolicy.MailSendUser, notifyerPolicy.MailSendPassword);
                    //}
                    MailClient.Send(mailMessage);
                    EWTLogger.Log.Notifyer_4405_Success_sending_mail(requestID, template, string.Join(",", mailMessage.To.Select(ma => ma.Address)), mailMessage.Subject, mailMessage.Body);
                }
            }
            catch (Exception ex)
            {
                EWTLogger.Log.Notifyer_4404_Failed_to_send_email(requestID, template, ex.ToString());

            }
            return;
        }

    }
}