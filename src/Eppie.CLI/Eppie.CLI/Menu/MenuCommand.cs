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

using System.Diagnostics;

using Eppie.CLI.Services;

using Tuvi.Toolkit.Cli.CommandLine;

namespace Eppie.CLI.Menu
{
    internal static class MenuCommand
    {
        internal readonly record struct Command(string Name, bool RequiresUnlockedApplication)
        {
            public static implicit operator string(Command command)
            {
                return command.Name;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public static readonly Command Exit = new("exit", RequiresUnlockedApplication: false);
        public static readonly Command Initialize = new("init", RequiresUnlockedApplication: false);
        public static readonly Command Open = new("open", RequiresUnlockedApplication: false);
        public static readonly Command Reset = new("reset", RequiresUnlockedApplication: false);
        public static readonly Command Restore = new("restore", RequiresUnlockedApplication: false);
        public static readonly Command Send = new("send", RequiresUnlockedApplication: true);
        public static readonly Command Import = new("import", RequiresUnlockedApplication: true);
        public static readonly Command ListAccounts = new("list-accounts", RequiresUnlockedApplication: true);
        public static readonly Command ListFolders = new("list-folders", RequiresUnlockedApplication: true);
        public static readonly Command AddAccount = new("add-account", RequiresUnlockedApplication: true);
        public static readonly Command ListContacts = new("list-contacts", RequiresUnlockedApplication: true);
        public static readonly Command ShowMessage = new("show-message", RequiresUnlockedApplication: true);
        public static readonly Command DeleteMessage = new("delete-message", RequiresUnlockedApplication: true);
        public static readonly Command ShowAllMessages = new("show-all-messages", RequiresUnlockedApplication: true);
        public static readonly Command SyncFolder = new("sync-folder", RequiresUnlockedApplication: true);
        public static readonly Command ShowFolderMessages = new("show-folder-messages", RequiresUnlockedApplication: true);
        public static readonly Command ShowContactMessages = new("show-contact-messages", RequiresUnlockedApplication: true);

        private static readonly IReadOnlyList<Command> StartupCommands =
        [
            Exit,
            Initialize,
            Open,
            Reset,
            Restore,
            Send,
            Import,
            ListAccounts,
            ListFolders,
            AddAccount,
            ListContacts,
            ShowMessage,
            DeleteMessage,
            ShowAllMessages,
            SyncFolder,
            ShowFolderMessages,
            ShowContactMessages,
        ];

        private static readonly IReadOnlyDictionary<string, Command> StartupCommandsByName = StartupCommands
            .ToDictionary(static command => command.Name, StringComparer.Ordinal);

        internal static bool RequiresUnlockedApplication(string commandName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(commandName);

            return StartupCommandsByName.TryGetValue(commandName, out Command command)
                && command.RequiresUnlockedApplication;
        }

        public static class CommandListFoldersOptions
        {
            public static readonly IReadOnlyCollection<string> AccountOptionNames = ["-a", "--account", "/Account"];

            public static IReadOnlyCollection<IOption> GetOptions(IAsyncParser parser, ResourceLoader resourceLoader)
            {
                Debug.Assert(parser is not null);
                Debug.Assert(resourceLoader is not null);

                return
                [
                    parser.CreateOption<string>(AccountOptionNames, isRequired: true, description: resourceLoader.Strings.AccountAddressDescription)
                ];
            }

            public static string GetAccountValue(IAsyncCommand cmd)
            {
                return GetRequiredOptionValue<string>(cmd, AccountOptionNames.First());
            }
        }

        public static class CommandAddAccountOptions
        {
            public readonly record struct Options(AccountType Type, bool InputJsonFromStandardInput);

            public enum AccountType
            {
                Email,
                Dec,
                Proton,
            }

            public static readonly IReadOnlyCollection<string> TypeOptionNames = ["-t", "--type", "/Type"];
            public static readonly IReadOnlyCollection<string> InputJsonFromStandardInputOptionNames = ["--input-json-stdin"];

            public static IReadOnlyCollection<IOption> GetOptions(IAsyncParser parser, ResourceLoader resourceLoader)
            {
                Debug.Assert(parser is not null);
                Debug.Assert(resourceLoader is not null);

                return
                [
                    parser.CreateOption<AccountType>(TypeOptionNames, isRequired: true, description: resourceLoader.Strings.AccountTypeDescription),
                    parser.CreateOption<bool>(InputJsonFromStandardInputOptionNames, description: resourceLoader.Strings.InputJsonFromStandardInputDescription),
                ];
            }

            public static Options GetValues(IAsyncCommand cmd)
            {
                ArgumentNullException.ThrowIfNull(cmd);

                return new Options(GetRequiredOptionValue<AccountType>(cmd, TypeOptionNames.First()),
                                   GetOptionValue<bool>(cmd, InputJsonFromStandardInputOptionNames.First()));
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
            public static readonly IReadOnlyCollection<string> LimitOptionNames = ["-l", "--limit", "/Limit"];
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
                    parser.CreateOption<int?>(LimitOptionNames, description: resourceLoader.Strings.LimitDescription),
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
                    parser.CreateOption<int?>(LimitOptionNames, description: resourceLoader.Strings.LimitDescription),
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
                    parser.CreateOption<int?>(LimitOptionNames, description: resourceLoader.Strings.LimitDescription),
                ];
            }

            public static int GetPageSizeValue(IAsyncCommand cmd)
            {
                return GetRequiredOptionValue<int>(cmd, PageSizeOptionNames.First());
            }

            public static ApplicationListingOptions GetListingOptions(IAsyncCommand cmd)
            {
                ArgumentNullException.ThrowIfNull(cmd);

                return new ApplicationListingOptions(GetPageSizeValue(cmd), GetLimitValue(cmd));
            }

            public static int? GetLimitValue(IAsyncCommand cmd)
            {
                return GetOptionValue<int?>(cmd, LimitOptionNames.First());
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
            public static readonly IReadOnlyCollection<string> LimitOptionNames = ["-l", "--limit", "/Limit"];

            public static IReadOnlyCollection<IOption> GetOptions(IAsyncParser parser, ResourceLoader resourceLoader)
            {
                Debug.Assert(parser is not null);
                Debug.Assert(resourceLoader is not null);

                return
                [
                    parser.CreateOption<int>(PageSizeOptionNames, getDefaultValue: () => DefaultPageSize, description: resourceLoader.Strings.PageSizeDescription),
                    parser.CreateOption<int?>(LimitOptionNames, description: resourceLoader.Strings.LimitDescription),
                ];
            }

            public static int GetPageSizeValue(IAsyncCommand cmd)
            {
                return GetRequiredOptionValue<int>(cmd, PageSizeOptionNames.First());
            }

            public static ApplicationListingOptions GetListingOptions(IAsyncCommand cmd)
            {
                ArgumentNullException.ThrowIfNull(cmd);

                return new ApplicationListingOptions(GetPageSizeValue(cmd), GetLimitValue(cmd));
            }

            public static int? GetLimitValue(IAsyncCommand cmd)
            {
                return GetOptionValue<int?>(cmd, LimitOptionNames.First());
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
