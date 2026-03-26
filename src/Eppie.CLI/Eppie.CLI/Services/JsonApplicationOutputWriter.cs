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
using System.Text.Json;

using Tuvi.Core.Entities;

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal sealed class JsonApplicationOutputWriter(ResourceLoader resourceLoader) : IApplicationOutputWriter
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

        private readonly ResourceLoader _resourceLoader = resourceLoader;

        public ApplicationOutputFormat Format => ApplicationOutputFormat.Json;

        public void Write(ApplicationOutput output)
        {
            ArgumentNullException.ThrowIfNull(output);

            if (TryWriteResult(output) || TryWriteStatus(output) || TryWriteWarning(output) || TryWriteError(output))
            {
                return;
            }

            throw new InvalidOperationException($"Unknown output type '{output.GetType().FullName}'.");
        }

        private static bool TryWriteResult(ApplicationOutput output)
        {
            switch (output)
            {
                case AccountsOutput accountsOutput:
                    WriteResult("accounts", accountsOutput.Accounts.Select(static account => new
                    {
                        id = account.Id,
                        address = account.Email.Address,
                    }));
                    return true;
                case ContactsOutput contactsOutput:
                    WriteResult("contacts", contactsOutput.Contacts.Select(static contact => new
                    {
                        id = contact.Id,
                        address = contact.Email.Address,
                        fullName = contact.FullName,
                        unreadCount = contact.UnreadCount,
                    }));
                    return true;
                case MessageOutput messageOutput:
                    WriteResult("message", ToMessageOutput(messageOutput.Message, messageOutput.Compact));
                    return true;
                case MessagesOutput messagesOutput:
                    WriteResult("messages", messagesOutput.Messages.Select(message => ToMessageOutput(message, messagesOutput.Compact)),
                                meta: string.IsNullOrWhiteSpace(messagesOutput.Header) ? null : new { header = messagesOutput.Header });
                    return true;
                default:
                    return false;
            }
        }

        private static bool TryWriteStatus(ApplicationOutput output)
        {
            switch (output)
            {
                case ApplicationInitializedOutput initializedOutput:
                    WriteStatus("initialized", new { seedPhrase = initializedOutput.SeedPhrase });
                    return true;
                case AccountAddedOutput accountAddedOutput:
                    WriteResult("accountAdded", new { address = accountAddedOutput.Address, accountType = accountAddedOutput.AccountType });
                    return true;
                case ApplicationResetOutput:
                    WriteStatus("reset");
                    return true;
                case ApplicationOpenedOutput:
                    WriteStatus("opened");
                    return true;
                case ApplicationRestoredOutput:
                    WriteStatus("restored");
                    return true;
                case AuthorizationCanceledOutput:
                    WriteStatus("authorizationCanceled");
                    return true;
                case AuthorizationToServiceOutput authorizationToServiceOutput:
                    WriteStatus("authorizationStarted", new { serviceName = authorizationToServiceOutput.ServiceName });
                    return true;
                case AuthorizationCompletedOutput:
                    WriteStatus("authorizationCompleted");
                    return true;
                case MessageSentOutput messageSentOutput:
                    WriteStatus("messageSent", new { subject = messageSentOutput.Subject, to = messageSentOutput.To, from = messageSentOutput.From });
                    return true;
                case FolderSyncedOutput folderSyncedOutput:
                    WriteStatus("folderSynced", new { account = folderSyncedOutput.AccountAddress, folder = folderSyncedOutput.FolderName });
                    return true;
                default:
                    return false;
            }
        }

        private bool TryWriteWarning(ApplicationOutput output)
        {
            switch (output)
            {
                case InvalidPasswordWarningOutput:
                    WriteWarning("invalidPassword", _resourceLoader.Strings.InvalidPassword);
                    return true;
                case SecondInitializationWarningOutput:
                    WriteWarning("alreadyInitialized", _resourceLoader.Strings.SecondInitialization);
                    return true;
                case UninitializedAppWarningOutput:
                    WriteWarning("uninitialized", _resourceLoader.Strings.Uninitialized);
                    return true;
                case UnsuccessfulAttemptWarningOutput:
                    WriteWarning("unsuccessfulAttempt", _resourceLoader.Strings.UnsuccessfulAttempt);
                    return true;
                case UnknownFolderWarningOutput unknownFolderWarningOutput:
                    WriteWarning("unknownFolder",
                                 _resourceLoader.Strings.GetUnknownFolderWarning(unknownFolderWarningOutput.Address, unknownFolderWarningOutput.Folder),
                                 new { address = unknownFolderWarningOutput.Address, folder = unknownFolderWarningOutput.Folder });
                    return true;
                case CommandRequiresYesInNonInteractiveModeWarningOutput commandRequiresYesWarningOutput:
                    WriteWarning("commandRequiresYesInNonInteractiveMode",
                                 _resourceLoader.Strings.GetCommandRequiresYesInNonInteractiveModeWarning(commandRequiresYesWarningOutput.CommandName),
                                 new { commandName = commandRequiresYesWarningOutput.CommandName });
                    return true;
                case StartupCommandRequiresUnlockPasswordFromStandardInputWarningOutput startupWarningOutput:
                    WriteWarning("startupCommandRequiresUnlockPasswordFromStandardInput",
                                 _resourceLoader.Strings.GetStartupCommandRequiresUnlockPasswordFromStandardInputWarning(startupWarningOutput.CommandName),
                                 new { commandName = startupWarningOutput.CommandName });
                    return true;
                default:
                    return false;
            }
        }

        private bool TryWriteError(ApplicationOutput output)
        {
            switch (output)
            {
                case ImpossibleInitializationErrorOutput:
                    WriteError("impossibleInitialization", _resourceLoader.Strings.ImpossibleInitialization);
                    return true;
                case UnhandledExceptionOutput unhandledExceptionOutput:
                    WriteError("unhandledException",
                               _resourceLoader.Strings.GetUnhandledException(unhandledExceptionOutput.Exception),
                               new { exceptionType = unhandledExceptionOutput.Exception.GetType().FullName });
                    return true;
                default:
                    return false;
            }
        }

        private static void WriteResult<TData>(string code, TData data, object? meta = null)
        {
            WriteJson(new
            {
                type = "result",
                code,
                data,
                meta,
            });
        }

        private static void WriteStatus(string code, object? data = null)
        {
            WriteJson(new
            {
                type = "status",
                code,
                data,
            });
        }

        private static void WriteWarning(string code, string message, object? data = null)
        {
            WriteJson(new
            {
                type = "warning",
                code,
                message,
                data,
            });
        }

        private static void WriteError(string code, string message, object? data = null)
        {
            WriteJson(new
            {
                type = "error",
                code,
                message,
                data,
            });
        }

        private static object ToMessageOutput(Message message, bool compact)
        {
            ArgumentNullException.ThrowIfNull(message);

            return compact
                ? new
                {
                    id = message.Id,
                    pk = message.Pk,
                    date = message.Date,
                    to = message.To.FirstOrDefault()?.Address ?? string.Empty,
                    from = message.From.FirstOrDefault()?.Address ?? string.Empty,
                    folder = message.Folder.FullName,
                    subject = message.Subject,
                }
                : new
                {
                    id = message.Id,
                    pk = message.Pk,
                    date = message.Date,
                    to = message.To.Select(static address => address.Address),
                    from = message.From.Select(static address => address.Address),
                    cc = message.Cc.Select(static address => address.Address),
                    bcc = message.Bcc.Select(static address => address.Address),
                    folder = message.Folder.FullName,
                    subject = message.Subject,
                    textBody = message.TextBody,
                    htmlBody = message.HtmlBody,
                    attachmentsCount = message.Attachments.Count,
                };
        }

        private static void WriteJson<T>(T value)
        {
            Console.WriteLine(JsonSerializer.Serialize(value, JsonSerializerOptions));
        }
    }
}
