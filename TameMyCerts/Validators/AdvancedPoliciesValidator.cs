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
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using TameMyCerts.ClassExtensions;
using TameMyCerts.Enums;
using TameMyCerts.Models;
using TameMyCerts.X509;

namespace TameMyCerts.Validators
{
    /// <summary>
    ///     This validator will go throught he AdvancedPolicies to find a matching policy
    /// </summary>
    /// 
    internal class AdvancedPoliciesValidator
    {
        private const StringComparison Comparison = StringComparison.InvariantCultureIgnoreCase;
        internal AdvancedADPoliciesValidator _advancedADPoliciesValidator = new AdvancedADPoliciesValidator();


        public CertificateRequestValidationResult VerifyRequest(CertificateRequestValidationResult result,
            List<AdvancedPolicy> policies, CertificateDatabaseRow dbRow, ActiveDirectoryObject dsObject,
            CertificateAuthorityConfiguration caConfig)
        {
            foreach (var policy in policies)
            {
                AdvancedPolicy matchedPolicy = new AdvancedPolicy();
                matchedPolicy.PolicyName = policy.PolicyName;
                matchedPolicy.Action = policy.Action;
                bool policyMatch = true;
                // Check if the policy is a match for the AdvancedADPolicy
                Console.WriteLine("Checking AdvancedADPolicy");
                if (policy.AdvancedADPolicy.Any())
                {
                    bool adpMatch = false;
                    // If there are any AdvancedADPolicies, then check if any of them are a match, else go to next policy
                    foreach (var adp in policy.AdvancedADPolicy)
                    {
                        if (_advancedADPoliciesValidator.FullfillsPolicy(adp, dsObject))
                        {
                            // We have a match, 
                            Console.WriteLine("Matched one AD Policy");
                            matchedPolicy.AdvancedADPolicy.Add(adp);
                            adpMatch = true;
                            break;
                        }
                        // If we get here, then no advancedADPolicy matched, so we can set the policyMatch to false
                    }
                    if (adpMatch == false)
                    {
                        Console.WriteLine("Matched no AD Policy");
                        policyMatch = false;
                    }
                }
                // Check for match with a AdvancedCertificateRequestPolicy
                // Same logic as above but for the CertificateRequestPolicy

                // Check if the policy is a match for the AdvancedYubikeyPolicy
                // Same logic as above but for the Yubikey

                if (policy.Action == "Issue" && policyMatch == true)
                {
                    // Log to the operational log what policy was matched
                    Console.WriteLine(matchedPolicy.SaveToString());
                    return result;
                }
                else if (policy.Action == "IssueOnFail" && policyMatch != false)
                {
                    // Log to the operational log what policy failed to matched
                    return result;
                }
                else if (policy.Action == "Reject" && policyMatch == true)
                {
                    // Log to the operational log what policy was matched
                    result.SetFailureStatus(WinError.CERTSRV_E_TEMPLATE_DENIED, string.Format(
                        "Failed on policy: {0}",
                        policy.PolicyName));
                    return result;
                }
                else if (policy.Action == "RejectOnFail" && policyMatch == false)
                {
                    // Log to the operational log what policy failed to matched
                    result.SetFailureStatus(WinError.CERTSRV_E_TEMPLATE_DENIED, string.Format(
                        "Failed on policy: {0}",
                        policy.PolicyName));
                    return result;
                }
            }

            return result;
        }
    }
}