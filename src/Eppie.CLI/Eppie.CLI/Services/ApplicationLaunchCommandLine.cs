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

namespace Eppie.CLI.Services
{
    internal static class ApplicationLaunchCommandLine
    {
        private static readonly string[] KnownOptionKeys =
        [
            ApplicationLaunchOptions.UnlockPasswordFromStandardInputConfigurationKey,
            ApplicationLaunchOptions.NonInteractiveConfigurationKey,
            ApplicationLaunchOptions.YesConfigurationKey,
            ApplicationLaunchOptions.OutputConfigurationKey,
        ];

        internal static bool TryReadOptionValue(ApplicationCommandLineArguments commandLineArguments, string key, out string? optionValue)
        {
            ArgumentNullException.ThrowIfNull(commandLineArguments);

            return TryReadOptionValue(commandLineArguments.Values, key, out optionValue);
        }

        internal static int GetOptionArgumentCount(IReadOnlyList<string> arguments, int index)
        {
            ArgumentNullException.ThrowIfNull(arguments);
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, arguments.Count);

            return TryMatchOption(arguments[index], out string? key, out bool hasInlineValue)
                ? (hasInlineValue || !CanConsumeNextValue(key, index + 1 < arguments.Count ? arguments[index + 1] : null) ? 1 : 2)
                : 0;
        }

        private static bool TryReadOptionValue(IReadOnlyList<string> arguments, string key, out string? optionValue)
        {
            ArgumentNullException.ThrowIfNull(arguments);
            ArgumentException.ThrowIfNullOrWhiteSpace(key);

            string optionName = $"--{key}";

            for (int i = 0; i < arguments.Count; i++)
            {
                string argument = arguments[i];

                if (argument.Equals(optionName, StringComparison.Ordinal))
                {
                    optionValue = CanConsumeNextValue(key, i + 1 < arguments.Count ? arguments[i + 1] : null)
                        ? arguments[i + 1]
                        : string.Empty;
                    return true;
                }

                if (argument.StartsWith(optionName + "=", StringComparison.Ordinal))
                {
                    optionValue = argument[(optionName.Length + 1)..];
                    return true;
                }
            }

            optionValue = null;
            return false;
        }

        private static bool TryMatchOption(string argument, [NotNullWhen(true)] out string? key, out bool hasInlineValue)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(argument);

            foreach (string optionKey in KnownOptionKeys)
            {
                string optionName = $"--{optionKey}";

                if (argument.Equals(optionName, StringComparison.Ordinal))
                {
                    key = optionKey;
                    hasInlineValue = false;
                    return true;
                }

                if (argument.StartsWith(optionName + "=", StringComparison.Ordinal))
                {
                    key = optionKey;
                    hasInlineValue = true;
                    return true;
                }
            }

            key = null;
            hasInlineValue = false;
            return false;
        }

        private static bool CanConsumeNextValue(string key, string? nextArgument)
        {
            return nextArgument is not null
                && (key == ApplicationLaunchOptions.OutputConfigurationKey ? IsOutputFormatValue(nextArgument) : IsBooleanValue(nextArgument));
        }

        private static bool IsOutputFormatValue(string value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return string.Equals(value, "text", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "json", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsBooleanValue(string value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return bool.TryParse(value, out _);
        }
    }
}
