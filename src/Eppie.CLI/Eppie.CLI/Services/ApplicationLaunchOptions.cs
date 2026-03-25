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
        private const string StartupCommandConfigurationKey = "command";
        private const string UnlockPasswordFromStandardInputConfigurationKey = "unlock-password-stdin";

        internal string? StartupCommand { get; } = ReadStartupCommand(configuration);

        internal bool UnlockPasswordFromStandardInput { get; } = ReadFlagOption(configuration, UnlockPasswordFromStandardInputConfigurationKey);

        private static string? ReadStartupCommand(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            string? startupCommand = configuration[StartupCommandConfigurationKey];
            return string.IsNullOrWhiteSpace(startupCommand) ? null : startupCommand;
        }

        private static bool ReadFlagOption(IConfiguration configuration, string key)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            string? value = configuration[key];

            // Command-line flags are expected to provide an explicit boolean value, for example: --unlock-password-stdin=true.
            return bool.TryParse(value, out bool parsedValue) && parsedValue;
        }
    }
}
