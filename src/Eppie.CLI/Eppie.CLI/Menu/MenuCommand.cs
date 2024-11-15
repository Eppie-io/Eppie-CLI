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

using System.Diagnostics;

using Eppie.CLI.Services;

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
        public const string ShowAllMessages = "show-all-messages";
        public const string SyncFolder = "sync-folder";
        public const string ShowFolderMessages = "show-folder-messages";
        public const string ShowContactMessages = "show-contact-messages";

        public static class CommandAddAccountOptions
        {
            public enum AccountType
            {
                Email,
                Dec,
                Proton,
            }

            public static readonly IReadOnlyCollection<string> TypeOptionNames = ["-t", "--type", "/Type"];

            public static IReadOnlyCollection<IOption> GetOptions(IAsyncParser parser, ResourceLoader resourceLoader)
            {
                Debug.Assert(parser is not null);
                Debug.Assert(resourceLoader is not null);

                return
                [
                    parser.CreateOption<AccountType>(TypeOptionNames, isRequired: true, description: resourceLoader.Strings.AccountTypeDescription)
                ];
            }

            public static AccountType GetTypeValue(IAsyncCommand cmd)
            {
                return GetRequiredOptionValue<AccountType>(cmd, TypeOptionNames.First());
            }
        }

        public static class CommandSendOptions
        {
            public static readonly IReadOnlyCollection<string> SenderOptionNames = ["-s", "--sender", "/Sender"];
            public static readonly IReadOnlyCollection<string> ReceiverOptionNames = ["-r", "--receiver", "/Receiver"];
            public static readonly IReadOnlyCollection<string> SubjectOptionNames = ["-t", "--subject", "/Subject"];

            public static IReadOnlyCollection<IOption> GetOptions(IAsyncParser parser, ResourceLoader resourceLoader)
            {
                Debug.Assert(parser is not null);
                Debug.Assert(resourceLoader is not null);

                return
                [
                    parser.CreateOption<string>(SenderOptionNames, isRequired: true, description: resourceLoader.Strings.SenderDescription),
                    parser.CreateOption<string>(ReceiverOptionNames, isRequired: true, description: resourceLoader.Strings.ReceiverDescription),
                    parser.CreateOption<string>(SubjectOptionNames, isRequired: true, description: resourceLoader.Strings.SubjectDescription),
                ];
            }

            public static string GetSubjectValue(IAsyncCommand cmd)
            {
                return GetRequiredOptionValue<string>(cmd, SubjectOptionNames.First());
            }

            public static string GetSenderValue(IAsyncCommand cmd)
            {
                return GetRequiredOptionValue<string>(cmd, SenderOptionNames.First());
            }

            public static string GetReceiverValue(IAsyncCommand cmd)
            {
                return GetRequiredOptionValue<string>(cmd, ReceiverOptionNames.First());
            }
        }

        public static class CommandImportOptions
        {
            public static readonly IReadOnlyCollection<string> FileOptionNames = ["-f", "--file", "/File"];

            public static IReadOnlyCollection<IOption> GetOptions(IAsyncParser parser, ResourceLoader resourceLoader)
            {
                Debug.Assert(parser is not null);
                Debug.Assert(resourceLoader is not null);

                return
                [
                    parser.CreateOption<FileInfo>(FileOptionNames, isRequired: true, description: resourceLoader.Strings.KeyBundleFileDescription)
                ];
            }

            public static FileInfo GetFileValue(IAsyncCommand cmd)
            {
                return GetRequiredOptionValue<FileInfo>(cmd, FileOptionNames.First());
            }
        }

        public static class CommandShowMessageOptions
        {
            public static readonly IReadOnlyCollection<string> AccountOptionNames = ["-a", "--account", "/Account"];
            public static readonly IReadOnlyCollection<string> FolderOptionNames = ["-f", "--folder", "/Folder"];
            public static readonly IReadOnlyCollection<string> IdOptionNames = ["-i", "--message-id", "/MessageId"];
            public static readonly IReadOnlyCollection<string> PrimaryKeyOptionNames = ["-k", "--primary-key", "/PrimaryKey"];

            public static IReadOnlyCollection<IOption> GetOptions(IAsyncParser parser, ResourceLoader resourceLoader)
            {
                Debug.Assert(parser is not null);
                Debug.Assert(resourceLoader is not null);

                return
                [
                    parser.CreateOption<string>(AccountOptionNames, isRequired: true, description: resourceLoader.Strings.AccountAddressDescription),
                    parser.CreateOption<string>(FolderOptionNames, isRequired: true, description: resourceLoader.Strings.AccountFolderDescription),
                    parser.CreateOption<uint>(IdOptionNames, isRequired: true, description: resourceLoader.Strings.MessageIDDescription),
                    parser.CreateOption<int>(PrimaryKeyOptionNames, isRequired: true, description: resourceLoader.Strings.MessagePKDescription)
                ];
            }

            public static string GetAccountValue(IAsyncCommand cmd)
            {
                return GetRequiredOptionValue<string>(cmd, AccountOptionNames.First());
            }

            public static string GetFolderNameValue(IAsyncCommand cmd)
            {
                return GetRequiredOptionValue<string>(cmd, FolderOptionNames.First());
            }

            public static uint GetIdValue(IAsyncCommand cmd)
            {
                return GetRequiredOptionValue<uint>(cmd, IdOptionNames.First());
            }

            public static int GetPKValue(IAsyncCommand cmd)
            {
                return GetRequiredOptionValue<int>(cmd, PrimaryKeyOptionNames.First());
            }
        }

        public static class CommandSyncFolderOptions
        {
            public static readonly IReadOnlyCollection<string> AccountOptionNames = ["-a", "--account", "/Account"];
            public static readonly IReadOnlyCollection<string> FolderOptionNames = ["-f", "--folder", "/Folder"];

            public static IReadOnlyCollection<IOption> GetSyncFolderOptions(IAsyncParser parser, ResourceLoader resourceLoader)
            {
                Debug.Assert(parser is not null);
                Debug.Assert(resourceLoader is not null);

                return
                [
                    parser.CreateOption<string>(AccountOptionNames, isRequired: true, description: resourceLoader.Strings.AccountAddressDescription),
                    parser.CreateOption<string>(FolderOptionNames, isRequired: true, description: resourceLoader.Strings.AccountFolderDescription)
                ];
            }

            public static string GetAccountValue(IAsyncCommand cmd)
            {
                return GetRequiredOptionValue<string>(cmd, AccountOptionNames.First());
            }

            public static string GetFolderNameValue(IAsyncCommand cmd)
            {
                return GetRequiredOptionValue<string>(cmd, FolderOptionNames.First());
            }
        }

        public static class CommandShowMessagesOptions
        {
            private const int DefaultPageSize = 20;

            public static readonly IReadOnlyCollection<string> PageSizeOptionNames = ["-s", "--page-size", "/PageSize"];
            public static readonly IReadOnlyCollection<string> AccountOptionNames = ["-a", "--account", "/Account"];
            public static readonly IReadOnlyCollection<string> FolderOptionNames = ["-f", "--folder", "/Folder"];
            public static readonly IReadOnlyCollection<string> ContactOptionNames = ["-c", "--contact-address", "/ContactAddress"];

            public static IReadOnlyCollection<IOption> GetShowMessagesOptions(IAsyncParser parser, ResourceLoader resourceLoader)
            {
                Debug.Assert(parser is not null);
                Debug.Assert(resourceLoader is not null);

                return
                [
                    parser.CreateOption<int>(PageSizeOptionNames, getDefaultValue: () => DefaultPageSize, description: resourceLoader.Strings.PageSizeDescription),
                ];
            }

            public static IReadOnlyCollection<IOption> GetShowFolderMessagesOptions(IAsyncParser parser, ResourceLoader resourceLoader)
            {
                Debug.Assert(parser is not null);
                Debug.Assert(resourceLoader is not null);

                return
                [
                    parser.CreateOption<string>(AccountOptionNames, isRequired: true, description: resourceLoader.Strings.AccountAddressDescription),
                    parser.CreateOption<string>(FolderOptionNames, isRequired: true, description: resourceLoader.Strings.AccountFolderDescription),
                    parser.CreateOption<int>(PageSizeOptionNames, getDefaultValue: () => DefaultPageSize, description: resourceLoader.Strings.PageSizeDescription),
                ];
            }

            public static IReadOnlyCollection<IOption> GetShowContactMessagesOptions(IAsyncParser parser, ResourceLoader resourceLoader)
            {
                Debug.Assert(parser is not null);
                Debug.Assert(resourceLoader is not null);

                return
                [
                    parser.CreateOption<string>(ContactOptionNames, isRequired: true, description: resourceLoader.Strings.ContactAddressDescription),
                    parser.CreateOption<int>(PageSizeOptionNames, getDefaultValue: () => DefaultPageSize, description: resourceLoader.Strings.PageSizeDescription),
                ];
            }

            public static int GetPageSizeValue(IAsyncCommand cmd)
            {
                return GetRequiredOptionValue<int>(cmd, PageSizeOptionNames.First());
            }

            public static string GetAccountValue(IAsyncCommand cmd)
            {
                return GetRequiredOptionValue<string>(cmd, AccountOptionNames.First());
            }

            public static string GetFolderNameValue(IAsyncCommand cmd)
            {
                return GetRequiredOptionValue<string>(cmd, FolderOptionNames.First());
            }

            public static string GetContactAddressValue(IAsyncCommand cmd)
            {
                return GetRequiredOptionValue<string>(cmd, ContactOptionNames.First());
            }
        }

        public static class CommandListContactsOptions
        {
            private const int DefaultPageSize = 20;

            public static readonly IReadOnlyCollection<string> PageSizeOptionNames = ["-s", "--page-size", "/PageSize"];

            public static IReadOnlyCollection<IOption> GetOptions(IAsyncParser parser, ResourceLoader resourceLoader)
            {
                Debug.Assert(parser is not null);
                Debug.Assert(resourceLoader is not null);

                return
                [
                    parser.CreateOption<int>(PageSizeOptionNames, getDefaultValue: () => DefaultPageSize, description: resourceLoader.Strings.PageSizeDescription),
                ];
            }

            public static int GetPageSizeValue(IAsyncCommand cmd)
            {
                return GetRequiredOptionValue<int>(cmd, PageSizeOptionNames.First());
            }
        }

        private static T GetRequiredOptionValue<T>(IAsyncCommand cmd, string name)
        {
            ArgumentNullException.ThrowIfNull(cmd);
            return cmd.GetRequiredValue<T>(name) ?? throw new InvalidOperationException($"The required '{name}' option is missing.");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "For future use.")]
        private static T? GetOptionValue<T>(IAsyncCommand cmd, string name)
        {
            ArgumentNullException.ThrowIfNull(cmd);
            return cmd.GetRequiredValue<T>(name);
        }
    }
}
