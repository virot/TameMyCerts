﻿// Copyright 2021-2025 Uwe Gradenegger <info@gradenegger.eu>

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

namespace TameMyCerts.Models;

internal static class SanTypes
{
    public const string DnsName = "dNSName";
    public const string Rfc822Name = "rfc822Name";
    public const string UniformResourceIdentifier = "uniformResourceIdentifier";
    public const string UserPrincipalName = "userPrincipalName";
    public const string IpAddress = "iPAddress";

    public static List<string> ToList()
    {
        return
        [
            DnsName,
            IpAddress,
            Rfc822Name,
            UniformResourceIdentifier,
            UserPrincipalName
        ];
    }
}