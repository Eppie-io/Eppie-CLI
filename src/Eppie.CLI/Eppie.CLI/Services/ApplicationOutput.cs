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

using Tuvi.Core.Entities;

namespace Eppie.CLI.Services
{
    internal abstract record ApplicationOutput;

    internal sealed record AccountsOutput(IReadOnlyCollection<Account> Accounts) : ApplicationOutput;

    internal sealed record ContactsOutput(IReadOnlyCollection<Contact> Contacts) : ApplicationOutput;

    internal sealed record FoldersOutput(string AccountAddress, IReadOnlyCollection<Folder> Folders) : ApplicationOutput;

    internal sealed record MessageOutput(Message Message, bool Compact) : ApplicationOutput;

    internal readonly record struct PagingInfo(int Returned, bool HasMore);

    internal sealed record MessagesOutput(string? Header, IReadOnlyCollection<Message> Messages, PagingInfo? Paging) : ApplicationOutput;

    internal sealed record ApplicationInitializedOutput(IReadOnlyCollection<string> SeedPhrase) : ApplicationOutput;

    internal sealed record ApplicationResetOutput() : ApplicationOutput;

    internal sealed record ApplicationOpenedOutput() : ApplicationOutput;

    internal sealed record ApplicationRestoredOutput() : ApplicationOutput;

    internal sealed record AuthorizationCanceledOutput() : ApplicationOutput;

    internal sealed record AuthorizationToServiceOutput(string ServiceName) : ApplicationOutput;

    internal sealed record AuthorizationCompletedOutput() : ApplicationOutput;

    internal sealed record InvalidPasswordWarningOutput() : ApplicationOutput;

    internal sealed record SecondInitializationWarningOutput() : ApplicationOutput;

    internal sealed record UninitializedAppWarningOutput() : ApplicationOutput;

    internal sealed record UnsuccessfulAttemptWarningOutput() : ApplicationOutput;

    internal sealed record UnknownFolderWarningOutput(string Address, string Folder) : ApplicationOutput;

    internal sealed record CommandRequiresAssumeYesInNonInteractiveModeWarningOutput(string CommandName) : ApplicationOutput;

    internal sealed record StartupCommandRequiresUnlockPasswordFromStandardInputWarningOutput(string CommandName) : ApplicationOutput;

    internal sealed record StructuredStandardInputInvalidJsonErrorOutput(string CommandName) : ApplicationOutput;

    internal sealed record StructuredStandardInputMissingPropertyErrorOutput(string CommandName, string PropertyName) : ApplicationOutput;

    internal sealed record AccountAddedOutput(string Address, string AccountType) : ApplicationOutput;

    internal sealed record MessageSentOutput(string Subject, string To, string From) : ApplicationOutput;

    internal sealed record MessageDeletedOutput(string AccountAddress, string FolderName, uint Id, int Pk) : ApplicationOutput;

    internal sealed record FolderSyncedOutput(string AccountAddress, string FolderName) : ApplicationOutput;

    internal sealed record NonInteractiveOperationNotSupportedErrorOutput(string Operation) : ApplicationOutput;

    internal sealed record ImpossibleInitializationErrorOutput() : ApplicationOutput;

    internal sealed record UnhandledExceptionOutput(Exception Exception) : ApplicationOutput;
}
