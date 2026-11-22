// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2026 Eppie (https://eppie.io)                                    //
//                                                                              //
//   Licensed under the Apache License, Version 2.0 (the "License"),            //
//   you may not use this file except in compliance with the License.           //
//   You may obtain a copy of the License at                                    //
//                                                                              //
//       http://www.apache.org/licenses/LICENSE-2.0                             //
//                                                                              //
//   Unless required by applicable law or agreed to in writing, software        //
//   distributed under the License is distributed on an "AS IS" BASIS,          //
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.   //
//   See the License for the specific language governing permissions and        //
//   limitations under the License.                                             //
//                                                                              //
// ---------------------------------------------------------------------------- //

using System.Diagnostics.CodeAnalysis;

using Eppie.CLI.Common;

namespace Eppie.CLI.Options
{
    // ToDo : ConfigurationBindingGenerator cannot read private properties or use `init`.
    // We can fix this by using a mutable DTO for binding, and then mapping to an immutable record type.
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal class MailOptions
    {
        public static string SectionName => nameof(MailOptions);

        public IReadOnlyDictionary<MailServer, MailServerConfiguration> Servers { get; set; } = new Dictionary<MailServer, MailServerConfiguration>();
    }

    internal record MailServerConfiguration(string SMTP = "", int SMTPPort = 0, string IMAP = "", int IMAPPort = 0);
}
