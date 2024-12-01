using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TameMyCerts.Models;

namespace TameMyCerts.Support
{
    internal class FileSystemStorer
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
