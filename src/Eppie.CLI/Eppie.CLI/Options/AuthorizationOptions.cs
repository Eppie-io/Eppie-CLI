// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2024 Eppie (https://eppie.io)                                    //
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

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Eppie.CLI.Options
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal class AuthorizationOptions : IConfigurationSectionOptions
    {
        public string SectionName => "Authorization";

        public GmailAuthOptions Gmail { get; init; } = GmailAuthOptions.CreateDefault();
        public OutlookAuthOptions Outlook { get; init; } = OutlookAuthOptions.CreateDefault();
    }

    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal record GmailAuthOptions
    {
        public string? ClientId { get; init; }
        public string? ClientSecret { get; init; }
        public Uri? RedirectUri { get; init; }
        public Collection<string>? Scope => ScopeList ??
            [
                "https://mail.google.com/",
                "profile",
                "email"
            ];

        private Collection<string>? ScopeList { get; init; }

        internal static GmailAuthOptions CreateDefault()
        {
            return new GmailAuthOptions()
            {
                // Locally, you can set the secrets by running the commands from the project directory:
                //      > dotnet user-secrets set "Authorization:Gmail:ClientId" "<client-id>"
                //      > dotnet user-secrets set "Authorization:Gmail:ClientSecret" "<client-secret>"
                // Or run eppie client with arguments:
                // --Authorization:Gmail:ClientId="<client-id>" --Authorization:Gmail:ClientSecret="<client-secret>"

                ClientId = "Gmail ClientId is unset",
                ClientSecret = "Gmail ClientSecret is unset",
            };
        }
    }

    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal record OutlookAuthOptions
    {
        public string? ClientId { get; init; }
        public Uri? RedirectUri { get; init; }
        public Collection<string>? Scope => ScopeList ??
            [
                "https://outlook.office.com/user.read",
                "https://outlook.office.com/IMAP.AccessAsUser.All",
                "https://outlook.office.com/SMTP.Send",
                "offline_access"
            ];

        private Collection<string>? ScopeList { get; init; }

        internal static OutlookAuthOptions CreateDefault()
        {
            return new OutlookAuthOptions()
            {
                // Locally, you can set the secrets by running the command from the project directory:
                //      > dotnet user-secrets set "Authorization:Outlook:ClientId" "<client-id>"
                // Or run eppie client with arguments: --Authorization:Outlook:ClientId="<client-id>"

                ClientId = "Outlook ClientId is unset",
            };
        }
    }
}
