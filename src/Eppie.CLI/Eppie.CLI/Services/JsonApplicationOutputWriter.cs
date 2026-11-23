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

using Eppie.CLI.Json;

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal sealed class JsonApplicationOutputWriter(ResourceLoader resourceLoader) : IApplicationOutputWriter
    {
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
                    Write(AccountsStructuredResultOutput.Create(accountsOutput));
                    return true;

                case ContactsOutput contactsOutput:
                    Write(ContactsStructuredResultOutput.Create(contactsOutput));
                    return true;

                case FoldersOutput foldersOutput:
                    Write(FoldersStructuredResultOutput.Create(foldersOutput));
                    return true;

                case MessageOutput messageOutput:
                    StructuredOutput msgOutput = messageOutput.Compact
                        ? CompactMessageStructuredResultOutput.Create(messageOutput)
                        : MessageStructuredResultOutput.Create(messageOutput);
                    Write(msgOutput);
                    return true;

                case MessagesOutput messagesOutput:
                    Write(MessagesStructuredResultOutput.Create(messagesOutput));
                    return true;

                case AccountAddedOutput accountAddedOutput:
                    Write(AccountAddedStructuredResultOutput.Create(accountAddedOutput));
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
                    Write(InitializedStructuredStatusOutput.Create(initializedOutput));
                    return true;
                case ApplicationResetOutput:
                    Write(ResetStructuredStatusOutput.Create());
                    return true;
                case ApplicationOpenedOutput:
                    Write(OpenedStructuredStatusOutput.Create());
                    return true;
                case ApplicationRestoredOutput:
                    Write(RestoredStructuredStatusOutput.Create());
                    return true;
                case AuthorizationToServiceOutput authorizationToServiceOutput:
                    Write(AuthorizationStartedStructuredStatusOutput.Create(authorizationToServiceOutput));
                    return true;
                case AuthorizationCompletedOutput:
                    Write(AuthorizationCompletedStructuredStatusOutput.Create());
                    return true;
                case MessageSentOutput messageSentOutput:
                    Write(MessageSentStructuredStatusOutput.Create(messageSentOutput));
                    return true;
                case MessageDeletedOutput messageDeletedOutput:
                    Write(MessageDeletedStructuredStatusOutput.Create(messageDeletedOutput));
                    return true;
                case FolderSyncedOutput folderSyncedOutput:
                    Write(FolderSyncedStructuredStatusOutput.Create(folderSyncedOutput));
                    return true;
                case InputRequiredOutput inputRequiredOutput:
                    Write(InputRequiredStructuredStatusOutput.Create(inputRequiredOutput));
                    return true;
                case ProtonHumanVerificationRequiredOutput humanVerificationRequiredOutput:
                    Write(HumanVerificationRequiredStructuredStatusOutput.Create(humanVerificationRequiredOutput));
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
                    string invalidPasswordMessage = _resourceLoader.Strings.InvalidPassword;
                    Write(InvalidPasswordStructuredWarningOutput.Create(invalidPasswordMessage));
                    return true;
                case SecondInitializationWarningOutput:
                    string secondInitializationMessage = _resourceLoader.Strings.SecondInitialization;
                    Write(SecondInitializationStructuredWarningOutput.Create(secondInitializationMessage));
                    return true;
                case UninitializedAppWarningOutput:
                    string uninitializedAppMessage = _resourceLoader.Strings.Uninitialized;
                    Write(UninitializedAppStructuredWarningOutput.Create(uninitializedAppMessage));
                    return true;
                case UnsuccessfulAttemptWarningOutput:
                    string unsuccessfulAttemptMessage = _resourceLoader.Strings.UnsuccessfulAttempt;
                    Write(UnsuccessfulAttemptStructuredWarningOutput.Create(unsuccessfulAttemptMessage));
                    return true;
                case UnknownFolderWarningOutput unknownFolderWarningOutput:
                    string unknownFolderMessage = _resourceLoader.Strings.GetUnknownFolderWarning(unknownFolderWarningOutput.Address,
                                                                                                  unknownFolderWarningOutput.Folder);
                    Write(UnknownFolderStructuredWarningOutput.Create(unknownFolderMessage,
                                                                      unknownFolderWarningOutput));
                    return true;
                case CommandRequiresAssumeYesInNonInteractiveModeWarningOutput commandRequiresAssumeYesWarningOutput:
                    string commandName = commandRequiresAssumeYesWarningOutput.CommandName;
                    string commandMessage = _resourceLoader.Strings.GetCommandRequiresAssumeYesInNonInteractiveModeWarning(commandName);
                    Write(CommandStructuredWarningOutput.Create(commandMessage, commandName));
                    return true;
                case StartupCommandRequiresUnlockPasswordFromStandardInputWarningOutput startupWarningOutput:
                    string startupCommandName = startupWarningOutput.CommandName;
                    string startupCommandMessage = _resourceLoader.Strings.GetStartupCommandRequiresUnlockPasswordFromStandardInputWarning(startupCommandName);
                    Write(UnlockPasswordStructuredWarningOutput.Create(startupCommandMessage, startupCommandName));
                    return true;
                default:
                    return false;
            }
        }

        private bool TryWriteError(ApplicationOutput output)
        {
            switch (output)
            {
                case InteractiveInputNotSupportedErrorOutput:
                    string interactiveMessage = _resourceLoader.Strings.InteractiveInputNotSupported;
                    Write(InteractiveStructuredErrorOutput.Create(interactiveMessage));
                    return true;
                case StandardInputEndedErrorOutput:
                    string standardInputEndedMessage = _resourceLoader.Strings.StandardInputEnded;
                    Write(StandardInputEndedStructuredErrorOutput.Create(standardInputEndedMessage));
                    return true;
                case ImpossibleInitializationErrorOutput:
                    string impossibleInitializationMessage = _resourceLoader.Strings.ImpossibleInitialization;
                    Write(ImpossibleInitializationStructuredErrorOutput.Create(impossibleInitializationMessage));
                    return true;
                case AuthorizationCanceledOutput:
                    string authorizationCanceledMessage = _resourceLoader.Strings.AuthorizationCanceled;
                    Write(AuthorizationCanceledStructuredErrorOutput.Create(authorizationCanceledMessage));
                    return true;
                case NonInteractiveOperationNotSupportedErrorOutput nonInteractiveOperationNotSupportedOutput:
                    string operationMessage = _resourceLoader.Strings.GetNonInteractiveOperationNotSupportedError(nonInteractiveOperationNotSupportedOutput.Operation);
                    Write(OperationStructuredErrorOutput.Create(operationMessage, nonInteractiveOperationNotSupportedOutput));
                    return true;
                case StructuredStandardInputInvalidJsonErrorOutput structuredInputInvalidJsonOutput:
                    string invalidJsonMessage = _resourceLoader.Strings.GetStructuredStandardInputInvalidJsonError(structuredInputInvalidJsonOutput.CommandName);
                    Write(InputInvalidStructuredErrorOutput.Create(invalidJsonMessage, structuredInputInvalidJsonOutput));
                    return true;
                case StructuredStandardInputMissingPropertyErrorOutput structuredInputMissingPropertyOutput:
                    string missingPropertyMessage = _resourceLoader.Strings.GetStructuredStandardInputMissingPropertyError(structuredInputMissingPropertyOutput.CommandName,
                                                                                                                           structuredInputMissingPropertyOutput.PropertyName);
                    Write(MissingPropertyStructuredErrorOutput.Create(missingPropertyMessage,
                                                                      structuredInputMissingPropertyOutput));
                    return true;
                case UnhandledExceptionOutput unhandledExceptionOutput:
                    string unhandledExceptionMessage = _resourceLoader.Strings.GetUnhandledException(GetUnhandledExceptionMessage(unhandledExceptionOutput.Exception));
                    Write(UnhandledExceptionStructuredErrorOutput.Create(unhandledExceptionMessage, unhandledExceptionOutput));
                    return true;
                default:
                    return false;
            }
        }

        private static string GetUnhandledExceptionMessage(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return string.IsNullOrWhiteSpace(exception.Message)
                ? exception.GetType().Name
                : exception.Message;
        }

        private static void Write(StructuredOutput output)
        {
            ArgumentNullException.ThrowIfNull(output);
            Console.WriteLine(output.ToJson());
        }
    }
}
