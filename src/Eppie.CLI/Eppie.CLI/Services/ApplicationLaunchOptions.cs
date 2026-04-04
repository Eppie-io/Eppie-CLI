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
    internal sealed class ApplicationLaunchOptions(IConfiguration configuration, ApplicationCommandLineArguments commandLineArguments)
    {
        internal const string NonInteractiveConfigurationKey = "non-interactive";
        internal const string OutputConfigurationKey = "output";
        internal const string UnlockPasswordFromStandardInputConfigurationKey = "unlock-password-stdin";
        internal const string YesConfigurationKey = "yes";

        internal bool NonInteractive { get; } = ReadBooleanOption(configuration, commandLineArguments, NonInteractiveConfigurationKey);
        internal ApplicationOutputFormat OutputFormat { get; } = ReadOutputFormat(configuration, commandLineArguments, OutputConfigurationKey);
        internal bool UnlockPasswordFromStandardInput { get; } = ReadBooleanOption(configuration, commandLineArguments, UnlockPasswordFromStandardInputConfigurationKey);
        internal bool Yes { get; } = ReadBooleanOption(configuration, commandLineArguments, YesConfigurationKey);

        private static ApplicationOutputFormat ReadOutputFormat(IConfiguration configuration, ApplicationCommandLineArguments commandLineArguments, string key)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(commandLineArguments);

            return ApplicationLaunchCommandLine.TryReadOptionValue(commandLineArguments, key, out string? optionValue)
                ? ReadOutputFormatValue(optionValue)
                : ReadOutputFormatValue(configuration[key]);
        }

        private static bool ReadBooleanOption(IConfiguration configuration, ApplicationCommandLineArguments commandLineArguments, string key)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(commandLineArguments);

            return ApplicationLaunchCommandLine.TryReadOptionValue(commandLineArguments, key, out string? optionValue)
                ? ReadBooleanValue(optionValue, true)
                : ReadBooleanValue(configuration[key]);
        }

        private static bool ReadBooleanValue(string? value, bool defaultValue = false)
        {
            return value is null
                ? defaultValue
                : value.Length == 0 || (bool.TryParse(value, out bool parsedValue) ? parsedValue : defaultValue);
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
