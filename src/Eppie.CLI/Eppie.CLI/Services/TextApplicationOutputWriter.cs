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
using System.Text;

using Tuvi.Core.Entities;

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal sealed class TextApplicationOutputWriter(ResourceLoader resourceLoader) : IApplicationOutputWriter
    {
        private readonly ResourceLoader _resourceLoader = resourceLoader;

        public ApplicationOutputFormat Format => ApplicationOutputFormat.Text;

        public void Write(ApplicationOutput output)
        {
            ArgumentNullException.ThrowIfNull(output);

            switch (output)
            {
                case AccountsOutput accountsOutput:
                    WriteAccounts(accountsOutput.Accounts);
                    return;
                case ContactsOutput contactsOutput:
                    WriteContacts(contactsOutput.Contacts);
                    return;
                case FoldersOutput foldersOutput:
                    WriteFolders(foldersOutput.AccountAddress, foldersOutput.Folders);
                    return;
                case MessageOutput messageOutput:
                    WriteMessage(messageOutput.Message, messageOutput.Compact);
                    return;
                case MessagesOutput messagesOutput:
                    WriteMessages(messagesOutput.Header, messagesOutput.Messages);
                    return;
                case ApplicationInitializedOutput initializedOutput:
                    WriteInitialized(initializedOutput.SeedPhrase);
                    return;
                case ApplicationResetOutput:
                    Console.WriteLine(_resourceLoader.Strings.AppReset);
                    return;
                case ApplicationOpenedOutput:
                    Console.WriteLine(_resourceLoader.Strings.AppOpened);
                    return;
                case ApplicationRestoredOutput:
                    Console.WriteLine(_resourceLoader.Strings.AppRestored);
                    return;
                case AuthorizationCanceledOutput:
                    Console.WriteLine(_resourceLoader.Strings.AuthorizationCanceled);
                    return;
                case AuthorizationToServiceOutput authorizationToServiceOutput:
                    Console.WriteLine(_resourceLoader.Strings.GetAuthorizationToServiceText(authorizationToServiceOutput.ServiceName));
                    return;
                case AuthorizationCompletedOutput:
                    Console.WriteLine(_resourceLoader.Strings.AuthorizationCompleted);
                    return;
                case InvalidPasswordWarningOutput:
                    WriteWarning(_resourceLoader.Strings.InvalidPassword);
                    return;
                case SecondInitializationWarningOutput:
                    WriteWarning(_resourceLoader.Strings.SecondInitialization);
                    return;
                case UninitializedAppWarningOutput:
                    WriteWarning(_resourceLoader.Strings.Uninitialized);
                    return;
                case UnsuccessfulAttemptWarningOutput:
                    WriteWarning(_resourceLoader.Strings.UnsuccessfulAttempt);
                    return;
                case UnknownFolderWarningOutput unknownFolderWarningOutput:
                    WriteWarning(_resourceLoader.Strings.GetUnknownFolderWarning(unknownFolderWarningOutput.Address, unknownFolderWarningOutput.Folder));
                    return;
                case CommandRequiresYesInNonInteractiveModeWarningOutput commandRequiresYesWarningOutput:
                    WriteWarning(_resourceLoader.Strings.GetCommandRequiresYesInNonInteractiveModeWarning(commandRequiresYesWarningOutput.CommandName));
                    return;
                case AccountAddedOutput accountAddedOutput:
                    Console.WriteLine(_resourceLoader.Strings.GetAccountAddedText(accountAddedOutput.Address, accountAddedOutput.AccountType));
                    return;
                case StartupCommandRequiresUnlockPasswordFromStandardInputWarningOutput startupWarningOutput:
                    WriteWarning(_resourceLoader.Strings.GetStartupCommandRequiresUnlockPasswordFromStandardInputWarning(startupWarningOutput.CommandName));
                    return;
                case NonInteractiveOperationNotSupportedErrorOutput nonInteractiveOperationNotSupportedOutput:
                    WriteError(_resourceLoader.Strings.GetNonInteractiveOperationNotSupportedError(nonInteractiveOperationNotSupportedOutput.Operation));
                    return;
                case StructuredStandardInputInvalidJsonErrorOutput structuredInputInvalidJsonOutput:
                    WriteError(_resourceLoader.Strings.GetStructuredStandardInputInvalidJsonError(structuredInputInvalidJsonOutput.CommandName));
                    return;
                case StructuredStandardInputMissingPropertyErrorOutput structuredInputMissingPropertyOutput:
                    WriteError(_resourceLoader.Strings.GetStructuredStandardInputMissingPropertyError(structuredInputMissingPropertyOutput.CommandName,
                                                                                                      structuredInputMissingPropertyOutput.PropertyName));
                    return;
                case ImpossibleInitializationErrorOutput:
                    WriteError(_resourceLoader.Strings.ImpossibleInitialization);
                    return;
                case UnhandledExceptionOutput unhandledExceptionOutput:
                    WriteError(_resourceLoader.Strings.GetUnhandledException(unhandledExceptionOutput.Exception));
                    return;
                case MessageSentOutput messageSentOutput:
                    Console.WriteLine(_resourceLoader.Strings.GetMessageSentText(messageSentOutput.Subject, messageSentOutput.To, messageSentOutput.From));
                    return;
                case MessageDeletedOutput messageDeletedOutput:
                    Console.WriteLine(_resourceLoader.Strings.GetMessageDeletedText(messageDeletedOutput.AccountAddress,
                                                                                   messageDeletedOutput.FolderName,
                                                                                   messageDeletedOutput.Id,
                                                                                   messageDeletedOutput.Pk));
                    return;
                case FolderSyncedOutput folderSyncedOutput:
                    Console.WriteLine(_resourceLoader.Strings.GetFolderSyncedText(folderSyncedOutput.AccountAddress, folderSyncedOutput.FolderName));
                    return;
                default:
                    throw new InvalidOperationException($"Unknown output type '{output.GetType().FullName}'.");
            }
        }

        private void WriteFolders(string accountAddress, IReadOnlyCollection<Folder> folders)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(accountAddress);
            ArgumentNullException.ThrowIfNull(folders);

            if (folders.Count == 0)
            {
                Console.WriteLine(_resourceLoader.Strings.GetEmptyFolderList(accountAddress));
                return;
            }

            Console.WriteLine(_resourceLoader.Strings.GetHeaderFolderList(accountAddress));

            int i = 0;
            foreach (Folder folder in folders)
            {
                Console.WriteLine($"{++i}. {folder.FullName}");
            }

            Console.WriteLine();
        }

        private void WriteInitialized(IReadOnlyCollection<string> seedPhrase)
        {
            ArgumentNullException.ThrowIfNull(seedPhrase);

            Console.WriteLine();
            Console.WriteLine(_resourceLoader.Strings.SeedPhraseHeader);
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{string.Join(' ', seedPhrase)}");
            Console.ResetColor();

            Console.WriteLine();
            Console.WriteLine(_resourceLoader.Strings.SeedPhraseFooter);
        }

        private void WriteAccounts(IReadOnlyCollection<Account> accounts)
        {
            ArgumentNullException.ThrowIfNull(accounts);

            if (accounts.Count == 0)
            {
                Console.WriteLine(_resourceLoader.Strings.EmptyAccountList);
                return;
            }

            Console.WriteLine(_resourceLoader.Strings.HeaderAccountList);
            int i = 0;
            foreach (Account account in accounts)
            {
                Console.WriteLine($"{++i}. {account.Email.Address} ({account.Type})");
            }

            Console.WriteLine();
        }

        private void WriteContacts(IReadOnlyCollection<Contact> contacts)
        {
            ArgumentNullException.ThrowIfNull(contacts);

            if (contacts.Count == 0)
            {
                Console.WriteLine(_resourceLoader.Strings.EmptyContactList);
                return;
            }

            foreach (Contact contact in contacts)
            {
                Console.WriteLine(_resourceLoader.Strings.GetContactDetailsText(contact.Id, contact.Email.Address, contact.FullName, contact.UnreadCount));
            }
        }

        private void WriteMessages(string? header, IReadOnlyCollection<Message> messages)
        {
            ArgumentNullException.ThrowIfNull(messages);

            if (!string.IsNullOrEmpty(header))
            {
                Console.WriteLine(header);
            }

            foreach (Message message in messages)
            {
                WriteMessage(message, compact: true);
            }
        }

        private void WriteMessage(Message message, bool compact)
        {
            ArgumentNullException.ThrowIfNull(message);

            if (compact)
            {
                string to = message.To.FirstOrDefault()?.Address ?? string.Empty;
                string from = message.From.FirstOrDefault()?.Address ?? string.Empty;

                Console.WriteLine(_resourceLoader.Strings.GetMessageDetailsText(message.Id, message.Pk, message.Date, Truncate(to), Truncate(from),
                                                                                message.Folder.FullName, message.Subject));
                return;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(_resourceLoader.Strings.GetMessageIDPropertyText(message.Id));
            Console.WriteLine(_resourceLoader.Strings.GetMessagePKPropertyText(message.Pk));
            Console.WriteLine(_resourceLoader.Strings.GetMessageDatePropertyText(message.Date));
            Console.WriteLine(_resourceLoader.Strings.GetMessageToPropertyText(string.Join(';', message.To)));
            Console.WriteLine(_resourceLoader.Strings.GetMessageFromPropertyText(string.Join(';', message.From)));
            Console.WriteLine(_resourceLoader.Strings.GetMessageCcPropertyText(string.Join(';', message.Cc)));
            Console.WriteLine(_resourceLoader.Strings.GetMessageBccPropertyText(string.Join(';', message.Bcc)));
            Console.WriteLine(_resourceLoader.Strings.GetMessageFolderText(message.Folder.FullName));
            Console.WriteLine(_resourceLoader.Strings.GetMessageSubjectPropertyText(message.Subject));

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message.TextBody ?? message.HtmlBody ?? string.Empty);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(_resourceLoader.Strings.GetMessageAttachmentsCountText(message.Attachments.Count));

            Console.ResetColor();
        }

        private static string Truncate(string s, int maxLength = 19)
        {
            ArgumentNullException.ThrowIfNull(s);

            if (s.Length <= maxLength)
            {
                return s;
            }

            int halfLen = maxLength >> 1;
            StringBuilder sb = new(maxLength);
            sb.Append(s.AsSpan(0, halfLen));
            sb.Append('…');
            sb.Append(s.AsSpan(s.Length - halfLen, halfLen));
            return sb.ToString();
        }

        private static void WriteWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
