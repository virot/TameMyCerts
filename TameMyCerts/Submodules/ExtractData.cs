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
using System.Xml.Linq;
using TameMyCerts.Models;
using TameMyCerts.Validators;

namespace TameMyCerts.Submodules
{
    /// <summary>
    ///     This submodule is resonsible for notifying the users with a more informative test.
    /// </summary>
    internal class ExtractData
    {
        private const StringComparison Comparison = StringComparison.InvariantCultureIgnoreCase;

        public static DataExportModel CreateObject(CertificateDatabaseRow dbRow, CertificateRequestValidationResult result, Dictionary<string, string> dsObjectAttributes = null, Dictionary<string, string> yubikeyAttributes = null, Dictionary<string, string> SubjectRDN = null)
        {
            DataExportModel dataExport = new DataExportModel();
            dataExport.RequestDatabaseRow = new DataExportModel.RequestDatabaseRowType(dbRow);
            dataExport.ReplaceTokenValuesXML.AddRange(dbRow.SubjectAlternativeNames.Select(kvp => new DataExportModel.SerializableKeyValuePair { Key = $"san:{kvp.Key}", Value = kvp.Value }));
            dataExport.CertificateRequestValidationResult = new DataExportModel.CertificateRequestValidationResultType(result);
            if (dsObjectAttributes != null)
            {
                dataExport.ReplaceTokenValuesXML = dsObjectAttributes.Select(kvp => new DataExportModel.SerializableKeyValuePair { Key = $"ad:{kvp.Key}", Value = kvp.Value }).ToList();
            }
            if (yubikeyAttributes != null)
            {
                dataExport.ReplaceTokenValuesXML.AddRange(yubikeyAttributes.Select(kvp => new DataExportModel.SerializableKeyValuePair { Key = $"yk:{kvp.Key}", Value = kvp.Value }));
            }
            if (SubjectRDN != null)
            {
                dataExport.ReplaceTokenValuesXML.AddRange(SubjectRDN.Select(kvp => new DataExportModel.SerializableKeyValuePair { Key = $"sdn:{kvp.Key}", Value = kvp.Value }));
            }
            return dataExport;
        }

    }
}