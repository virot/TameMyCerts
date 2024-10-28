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
    ///     This validator is involved with the Advanced Policies validator to do the validation for the AD parts
    /// </summary>
    internal class AdvancedADPoliciesValidator
    {
        private const StringComparison Comparison = StringComparison.InvariantCultureIgnoreCase;


        public bool FullfillsPolicy(AdvancedADPolicy policy, ActiveDirectoryObject dsObject)
        {
            if (policy.GroupMembership.Any())
            {
                Console.WriteLine("Checking if matchin group memberships");
                // Check if the user is a member of the group for this policy
                if (!dsObject.MemberOf.Intersect(policy.GroupMembership).Any())
                {
                    Console.WriteLine("Failed to match group memberships");
                    return false;
                }
            }

            if (policy.PermitDisabledAccounts == false && dsObject.UserAccountControl.HasFlag(UserAccountControl.ACCOUNTDISABLE))
            {
                Console.WriteLine("Failed to match disabled accounts");
                return false;
            }

            // Check that the OUs are correct
            if (policy.OrganizationalUnits.Any())
            {
                if (policy.OrganizationalUnits.Where(ou => dsObject.DistinguishedName.EndsWith($",{ou}", Comparison)).Any())
                {
                    Console.WriteLine("Failed to match OU");
                    return false;
                }
            }

            return true;
        }
    }
}