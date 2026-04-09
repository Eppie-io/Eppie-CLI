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

namespace Eppie.CLI.Tests.Services
{
    internal static class TestConstants
    {
        internal const string Password = "password";
        internal const string WrongPassword = "wrong-password";
        internal const string VaultPassword = "vault-password";
        internal const string Json = "json";
        internal const string Xml = "xml";
        internal const string True = "true";
        internal const string TruePascal = "True";
        internal const string TrueUpper = "TRUE";
        internal const string False = "false";
        internal const string One = "1";
        internal const string Two = "2";
        internal const string Ten = "10";
        internal const string Boom = "boom";
        internal const string Application = "application";
        internal const string Opened = "opened";
        internal const string Restored = "restored";
        internal const string SentRole = "sent";
        internal const string AllRole = "all";
        internal const string Contact1FullName = "Contact 1";
        internal const string EmailPropertyName = "email";
        internal const string ImapServerPropertyName = "imapServer";
        internal const string AccountAddress = "account@eppie.io";
        internal const string UserAddress = "user@example.com";
        internal const string ToAddress = "to@example.com";
        internal const string FromAddress = "from@example.com";
        internal const string MissingAddress = "missing@eppie";
        internal const string Inbox = "Inbox";
        internal const string Archive = "Archive";
        internal const string AllSent = "All Sent";
        internal const string TrashRole = "trash";
        internal const string InboxHeader = "Inbox header";
        internal const string InteractiveMenu = "interactive menu";
        internal const string HelpOption = "-h";
        internal const string ListAccountsCommand = "list-accounts";
        internal const string ListFoldersCommand = "list-folders";
        internal const string ShowAllMessagesCommand = "show-all-messages";
        internal const string ShowFolderMessagesCommand = "show-folder-messages";
        internal const string ShowMessageCommand = "show-message";
        internal const string AddAccountCommand = "add-account";
        internal const string ResetCommand = "reset";
        internal const string OpenCommand = "open";
        internal const string SendCommand = "send";
        internal const string DeleteMessageCommand = "delete-message";
        internal const string DecAccountType = "Dec";
        internal const string EmailAccountType = "Email";
        internal const string ProtonServiceName = "Proton";
        internal const string ProtonAccountType = "proton";
        internal const string EmailAccountCommandType = "email";
        internal const string DecAccountCommandType = "dec";
        internal const string Subject1 = "Subject 1";
        internal const string Subject2 = "Subject 2";
        internal const string Subject3 = "Subject 3";
        internal const string Subject4 = "Subject 4";
        internal const string ReplyTestSubject = "Re: test";
        internal const string MissingAccountSubject = "Missing account";
        internal const string DeleteMeSubject = "Delete me";
        internal const string MissingBodySubject = "Missing body";
        internal const string ClosedStdinSubject = "Closed stdin";
        internal const string HelloFromAutomation = "Hello from automation";
        internal const string SecondBodyLine = "Second body line";
        internal const string TextBody = "Text body";
        internal const string ContactAddressPrefix = "contact";
        internal const string ContactAddressDomain = "@eppie.io";
        internal const string VaultPasswordPrompt = "Enter the vault password:";
        internal const string EnterAccountAddressPrompt = "Enter account address:";
        internal const string InteractivePrompt = ">>>";
        internal const string UnlockPasswordStdinOption = "--unlock-password-stdin";
        internal const string NonInteractiveOption = "--non-interactive";
        internal const string AssumeYesOption = "--assume-yes";
        internal const string OutputOption = "--output";
        internal const string InputJsonStdinOption = "--input-json-stdin";
        internal const string AccountOption = "--account";
        internal const string FolderOption = "--folder";
        internal const string MessageIdOption = "--message-id";
        internal const string PrimaryKeyOption = "--primary-key";
        internal const string ShortAccountOption = "-a";
        internal const string ShortFolderOption = "-f";
        internal const string ShortIdOption = "-i";
        internal const string ShortPrimaryKeyOption = "-k";
        internal const string ShortLimitOption = "-l";
        internal const string ShortRecipientOption = "-r";
        internal const string ShortPageSizeOption = "-s";
        internal const string ShortTypeOption = "-t";

        internal const string JsonTypeProperty = "type";
        internal const string JsonCodeProperty = "code";
        internal const string JsonDataProperty = "data";
        internal const string JsonMetaProperty = "meta";
        internal const string JsonMessageProperty = "message";
        internal const string JsonAddressProperty = "address";
        internal const string JsonAccountProperty = "account";
        internal const string JsonFolderProperty = "folder";
        internal const string JsonIdProperty = "id";
        internal const string JsonPkProperty = "pk";
        internal const string JsonSubjectProperty = "subject";
        internal const string JsonCommandNameProperty = "commandName";
        internal const string JsonPropertyNameProperty = "propertyName";
        internal const string JsonSeedPhraseProperty = "seedPhrase";
        internal const string JsonFullNameProperty = "fullName";
        internal const string JsonRolesProperty = "roles";
        internal const string JsonHeaderProperty = "header";
        internal const string JsonReturnedProperty = "returned";
        internal const string JsonHasMoreProperty = "hasMore";
        internal const string JsonToProperty = "to";
        internal const string JsonFromProperty = "from";
        internal const string JsonAccountTypeProperty = "accountType";
        internal const string JsonOperationProperty = "operation";
        internal const string JsonServiceNameProperty = "serviceName";
        internal const string JsonExceptionTypeProperty = "exceptionType";
        internal const string JsonTextBodyProperty = "textBody";
        internal const string JsonUnreadCountProperty = "unreadCount";
        internal const string JsonTotalCountProperty = "totalCount";

        internal const string JsonResultType = "result";
        internal const string JsonStatusType = "status";
        internal const string JsonWarningType = "warning";
        internal const string JsonErrorType = "error";
        internal const string InitializedCode = "initialized";
        internal const string UninitializedCode = "uninitialized";
        internal const string AccountsCode = "accounts";
        internal const string FoldersCode = "folders";
        internal const string MessagesCode = "messages";
        internal const string MessageSentCode = "messageSent";
        internal const string MessageDeletedCode = "messageDeleted";
        internal const string ResetCode = "reset";
        internal const string InvalidPasswordCode = "invalidPassword";
        internal const string StartupCommandRequiresUnlockPasswordFromStandardInputCode = "startupCommandRequiresUnlockPasswordFromStandardInput";
        internal const string UnhandledExceptionCode = "unhandledException";
        internal const string StructuredStandardInputInvalidJsonCode = "structuredStandardInputInvalidJson";
        internal const string StructuredStandardInputMissingPropertyCode = "structuredStandardInputMissingProperty";
        internal const string CommandRequiresAssumeYesInNonInteractiveModeCode = "commandRequiresAssumeYesInNonInteractiveMode";
        internal const string AlreadyInitializedCode = "alreadyInitialized";
        internal const string UnsuccessfulAttemptCode = "unsuccessfulAttempt";
        internal const string UnknownFolderCode = "unknownFolder";
        internal const string ImpossibleInitializationCode = "impossibleInitialization";
        internal const string NonInteractiveOperationNotSupportedCode = "nonInteractiveOperationNotSupported";
        internal const string AccountAddedCode = "accountAdded";
        internal const string ContactsCode = "contacts";
        internal const string FolderSyncedCode = "folderSynced";
        internal const string MessageCode = "message";
        internal const string AuthorizationCanceledCode = "authorizationCanceled";
        internal const string AuthorizationStartedCode = "authorizationStarted";
        internal const string AuthorizationCompletedCode = "authorizationCompleted";
    }
}
