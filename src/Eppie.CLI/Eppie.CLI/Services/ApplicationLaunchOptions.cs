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

using Eppie.CLI.Options;

using Microsoft.Extensions.Configuration;

namespace Eppie.CLI.Services
{
    // ToDo : ConfigurationBindingGenerator cannot read private properties or use `init`.
    // We can fix this by using a mutable DTO for binding, and then mapping to an immutable record type.
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal class ApplicationLaunchOptions
    {
        internal const string NonInteractiveConfigurationKey = "non-interactive";
        internal const string OutputConfigurationKey = "output";
        internal const string UnlockPasswordFromStandardInputConfigurationKey = "unlock-password-stdin";
        internal const string AssumeYesConfigurationKey = "assume-yes";

        [ConfigurationKeyName(NonInteractiveConfigurationKey)]
        public bool NonInteractive { get; set; }

        [ConfigurationKeyName(OutputConfigurationKey)]
        public string? OutputFormatKey { get; set; }

        [ConfigurationIgnore]
        internal ApplicationOutputFormat OutputFormat => OptionConverter.ConvertEnumValue<ApplicationOutputFormat>(OutputFormatKey ?? string.Empty, ApplicationOutputFormat.Text, true);

        [ConfigurationKeyName(UnlockPasswordFromStandardInputConfigurationKey)]
        public bool UnlockPasswordFromStandardInput { get; set; }

        [ConfigurationKeyName(AssumeYesConfigurationKey)]
        public bool AssumeYes { get; set; }

        [ConfigurationIgnore]
        internal static IReadOnlyList<string> LaunchOptionKeys { get; } =
        [
            UnlockPasswordFromStandardInputConfigurationKey,
            NonInteractiveConfigurationKey,
            AssumeYesConfigurationKey,
            OutputConfigurationKey,
        ];
    }

    internal enum ApplicationOutputFormat
    {
        Text,
        Json,
    }
}
