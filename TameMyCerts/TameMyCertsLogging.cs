using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Tracing;
using TameMyCerts.Validators;

namespace TameMyCerts
{
    [EventSource(Name = "TameMyCerts", LocalizationResources = "TameMyCerts.LocalizedStrings")]
    public sealed class TameMyCertsLogging : EventSource
    {
        public static TameMyCertsLogging Log = new TameMyCertsLogging();

        [Event(1, Message = "Localization", Level = EventLevel.Informational, Channel = EventChannel.Admin, Task = EventTask.None, Keywords = EventKeywords.None)]
        public void TameMyCerts_Initialized()
        {
            if (IsEnabled()) {
                WriteEvent(1, TameMyCerts.LocalizedStrings.event_TameMyCerts_Initialized); 
            }
        }

        [Event(4001, Level = EventLevel.Verbose, Channel = EventChannel.Debug, Task = EventTask.None, Keywords = EventKeywords.None)]
        public void DirVal_Debug_Matched_AllowedGroups(string usergroups, string policygroups)
        {
            if (IsEnabled())
            {
                WriteEvent(4001, usergroups, policygroups);
            }
        }

        [Event(4002, Level = EventLevel.Verbose, Channel = EventChannel.Debug, Task = EventTask.None, Keywords = EventKeywords.None)]
        public void DirVal_Debug_Matched_DisallowedGroups(string usergroups, string policygroups)
        {
            if (IsEnabled())
            {
                WriteEvent(4002, usergroups, policygroups);
            }
        }

        [Event(4003, Level = EventLevel.Verbose, Channel = EventChannel.Debug, Task = EventTask.None, Keywords = EventKeywords.None)]
        public void DirVal_Debug_No_Matched_AllowedGroups(string usergroups, string policygroups)
        {
            if (IsEnabled())
            {
                WriteEvent(4003, usergroups, policygroups);
            }
        }

        [Event(4004, Level = EventLevel.Verbose, Channel = EventChannel.Debug, Task = EventTask.None, Keywords = EventKeywords.None)]
        public void DirVal_Debug_No_Matched_DisallowedGroups(string usergroups, string policygroups)
        {
            if (IsEnabled())
            {
                WriteEvent(4004, usergroups, policygroups);
            }
        }
    }
}
