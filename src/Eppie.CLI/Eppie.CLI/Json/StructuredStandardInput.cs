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

using System.Text.Json.Serialization;

using Eppie.CLI.Services;

namespace Eppie.CLI.Json
{
    internal sealed record EmailStructuredStandardInput
    {
        [JsonPropertyName(InputName.Email)]
        public string? Email { get; init; }

        [JsonPropertyName(InputName.AccountPassword)]
        public string? AccountPassword { get; init; }

        [JsonPropertyName(InputName.ImapServer)]
        public string? ImapServer { get; init; }

        [JsonPropertyName(InputName.ImapPort)]
        public int? ImapPort { get; init; }

        [JsonPropertyName(InputName.SmtpServer)]
        public string? SmtpServer { get; init; }

        [JsonPropertyName(InputName.SmtpPort)]
        public int? SmtpPort { get; init; }
    }

    internal sealed record ProtonStructuredStandardInput
    {
        [JsonPropertyName(InputName.Email)]
        public string? Email { get; init; }

        [JsonPropertyName(InputName.AccountPassword)]
        public string? AccountPassword { get; init; }

        [JsonPropertyName(InputName.MailboxPassword)]
        public string? MailboxPassword { get; init; }

        [JsonPropertyName(InputName.TwoFactorCode)]
        public string? TwoFactorCode { get; init; }

        [JsonPropertyName(InputName.HumanVerificationToken)]
        public string? HumanVerificationToken { get; init; }
    }

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
                                 PropertyNameCaseInsensitive = true,
                                 NumberHandling = JsonNumberHandling.AllowReadingFromString)]
    [JsonSerializable(typeof(ProtonStructuredStandardInput))]
    [JsonSerializable(typeof(EmailStructuredStandardInput))]
    internal partial class StructuredStandardInput : JsonSerializerContext
    { }
}
