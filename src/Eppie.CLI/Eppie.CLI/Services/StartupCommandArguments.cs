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
        internal const string CommandDelimiter = "--";

        internal static string[] GetStartupCommandArguments(RawCommandLineArguments commandLineArguments)
        {
            ArgumentNullException.ThrowIfNull(commandLineArguments);

            int commandStartIndex = GetCommandStartIndex(commandLineArguments.Values);
            return commandStartIndex >= 0
                ? [.. commandLineArguments.Values.Skip(commandStartIndex)]
                : [];
        }

        private static int GetCommandStartIndex(IReadOnlyList<string> arguments)
        {
            ArgumentNullException.ThrowIfNull(arguments);

            for (int index = 0; index < arguments.Count; index++)
            {
                if (string.Equals(arguments[index], CommandDelimiter, StringComparison.Ordinal))
                {
                    return index + 1 < arguments.Count ? index + 1 : -1;
                }
            }

            return -1;
        }
    }
}
