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

using Microsoft.Extensions.Configuration;

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal sealed class ApplicationLaunchOptions(IConfiguration configuration)
    {
        internal const string NonInteractiveConfigurationKey = "non-interactive";
        internal const string OutputConfigurationKey = "output";
        internal const string UnlockPasswordFromStandardInputConfigurationKey = "unlock-password-stdin";
        internal const string AssumeYesConfigurationKey = "assume-yes";

        internal static IReadOnlyList<string> LaunchOptionKeys { get; } =
        [
            UnlockPasswordFromStandardInputConfigurationKey,
            NonInteractiveConfigurationKey,
            AssumeYesConfigurationKey,
            OutputConfigurationKey,
        ];

        internal bool NonInteractive { get; } = ReadBooleanOption(configuration, NonInteractiveConfigurationKey);
        internal ApplicationOutputFormat OutputFormat { get; } = ReadOutputFormat(configuration, OutputConfigurationKey);
        internal bool UnlockPasswordFromStandardInput { get; } = ReadBooleanOption(configuration, UnlockPasswordFromStandardInputConfigurationKey);
        internal bool AssumeYes { get; } = ReadBooleanOption(configuration, AssumeYesConfigurationKey);

        private static ApplicationOutputFormat ReadOutputFormat(IConfiguration configuration, string key)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            return ReadOutputFormatValue(configuration[key]);
        }

        private static bool ReadBooleanOption(IConfiguration configuration, string key)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            return ReadBooleanValue(configuration[key]);
        }

        private static bool ReadBooleanValue(string? value)
        {
            return bool.TryParse(value, out bool parsedValue) && parsedValue;
        }

        private static ApplicationOutputFormat ReadOutputFormatValue(string? value)
        {
            return IsJsonOutputValue(value) ? ApplicationOutputFormat.Json : ApplicationOutputFormat.Text;
        }

        private static bool IsJsonOutputValue(string? value)
        {
            return string.Equals(value, "json", StringComparison.OrdinalIgnoreCase);
        }
    }

    internal enum ApplicationOutputFormat
    {
        Text,
        Json,
    }
}
