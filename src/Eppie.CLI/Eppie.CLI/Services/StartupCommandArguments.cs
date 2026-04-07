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

namespace Eppie.CLI.Services
{
    internal static class StartupCommandArguments
    {
        internal static string[] GetStartupCommandArguments(RawCommandLineArguments commandLineArguments)
        {
            ArgumentNullException.ThrowIfNull(commandLineArguments);

            int commandStartIndex = GetCommandStartIndex(commandLineArguments.Values);
            return [.. commandLineArguments.Values.Skip(commandStartIndex)];
        }

        private static int GetCommandStartIndex(IReadOnlyList<string> arguments)
        {
            ArgumentNullException.ThrowIfNull(arguments);

            int commandStartIndex = 0;

            while (commandStartIndex < arguments.Count
                && TryGetLeadingLaunchOptionArgumentCount(arguments, commandStartIndex, out int argumentCount))
            {
                commandStartIndex += argumentCount;
            }

            return commandStartIndex;
        }

        private static bool TryGetLeadingLaunchOptionArgumentCount(IReadOnlyList<string> arguments, int index, out int argumentCount)
        {
            ArgumentNullException.ThrowIfNull(arguments);
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, arguments.Count);

            string argument = arguments[index];
            ArgumentException.ThrowIfNullOrWhiteSpace(argument);

            foreach (string optionKey in ApplicationLaunchOptions.LaunchOptionKeys)
            {
                string optionName = $"--{optionKey}";

                if (argument.Equals(optionName, StringComparison.Ordinal))
                {
                    argumentCount = index + 1 < arguments.Count ? 2 : 1;
                    return true;
                }

                if (argument.StartsWith(optionName + "=", StringComparison.Ordinal))
                {
                    argumentCount = 1;
                    return true;
                }
            }

            argumentCount = 0;
            return false;
        }
    }
}
