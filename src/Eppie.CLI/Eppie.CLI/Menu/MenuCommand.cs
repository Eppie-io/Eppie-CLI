// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2023 Eppie (https://eppie.io)                                    //
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

using System.Diagnostics;

using Tuvi.Toolkit.Cli.CommandLine;

namespace Eppie.CLI.Menu
{
    internal static class MenuCommand
    {
        public const string Exit = "exit";
        public const string Initialize = "init";
        public const string Open = "open";
        public const string Reset = "reset";
        public const string Restore = "restore";
        public const string Send = "send";
        public const string Import = "import";
        public const string ListAccounts = "list-accounts";
        public const string AddAccount = "add-account";
        public const string ListContacts = "list-contacts";
        public const string ShowMessage = "show-message";
        public const string ShowMessages = "show-messages";

        public static class CommandAddAccountOptions
        {
            public enum AccountType
            {
                Email,
                Dec,
                Proton,
            }

            public static readonly IReadOnlyCollection<string> TypeOptionNames = new[] { "-t", "--type", "/Type" };

            public static IReadOnlyCollection<IOption> GetOptions(IAsyncParser parser)
            {
                Debug.Assert(parser is not null);

                return new IOption[]
                {
                    parser.CreateOption<AccountType>(TypeOptionNames, isRequired: true)
                };
            }

            public static AccountType GetTypeValue(IAsyncCommand cmd)
            {
                return GetOptionValue<AccountType>(cmd, TypeOptionNames.First());
            }
        }

        private static T? GetOptionValue<T>(IAsyncCommand cmd, string name)
        {
            ArgumentNullException.ThrowIfNull(cmd);
            return cmd.GetRequiredValue<T>(name);
        }
    }
}
