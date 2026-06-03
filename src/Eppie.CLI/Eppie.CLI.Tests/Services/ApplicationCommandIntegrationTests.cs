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

using System.Globalization;
using System.Text.Json;

using Eppie.CLI.Services;

using NUnit.Framework;

using JsonCommandResult = Eppie.CLI.Tests.Services.ApplicationCommandTestHarness.JsonCommandResult;
using MessageIdentity = Eppie.CLI.Tests.Services.ApplicationCommandTestHarness.MessageIdentity;
using ProcessResult = Eppie.CLI.Tests.Services.ApplicationCommandTestHarness.ProcessResult;

namespace Eppie.CLI.Tests.Services
{
    [TestFixture]
    public class ApplicationCommandIntegrationTests
    {
        [Test]
        public async Task ProcessWhenStartupCommandIsHelpPrintsCommandHelp()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-help-");

            ProcessResult result = await harness.RunApplicationAsync(TestConstants.HelpOption).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                AssertProcessSucceeded(result);
                Assert.That(result.StandardOutput, Does.Contain("Usage:"));
                Assert.That(result.StandardOutput, Does.Contain(TestConstants.NonInteractiveOption));
                Assert.That(result.StandardOutput, Does.Contain(TestConstants.OutputOption));
                Assert.That(result.StandardOutput, Does.Contain(TestConstants.AssumeYesOption));
                Assert.That(result.StandardOutput, Does.Contain(TestConstants.UnlockPasswordStdinOption));
                Assert.That(result.StandardOutput, Does.Contain(TestConstants.ListAccountsCommand));
                Assert.That(result.StandardOutput, Does.Contain(TestConstants.ListFoldersCommand));
                Assert.That(result.StandardOutput, Does.Contain(TestConstants.DeleteMessageCommand));
            });
        }

        [Test]
        public async Task ProcessWhenStartupCommandIsHelpInNonInteractiveModePrintsHelpWithoutInteractivePrompt()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-help-non-interactive-");

            ProcessResult result = await harness.RunApplicationAsync(TestConstants.HelpOption, nonInteractive: true).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                AssertProcessSucceeded(result);
                Assert.That(result.StandardOutput, Does.Contain("Usage:"));
                Assert.That(result.StandardOutput, Does.Contain(TestConstants.NonInteractiveOption));
                Assert.That(result.StandardOutput, Does.Not.Contain(TestConstants.InteractivePrompt));
            });
        }

        [Test]
        public async Task ProcessWhenNoStartupCommandIsProvidedInNonInteractiveModeOutputsClearErrorWithoutInteractiveLoop()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-no-startup-");

            ProcessResult result = await harness.RunApplicationAsync([], nonInteractive: true).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                AssertProcessSucceeded(result);
                Assert.That(result.StandardOutput, Does.Contain(TestConstants.InteractiveMenu));
                Assert.That(result.StandardOutput, Does.Contain(TestConstants.NonInteractiveOption));
                Assert.That(result.StandardOutput, Does.Not.Contain(TestConstants.InteractivePrompt));
            });
        }

        [Test]
        public async Task ProcessWhenStartupCommandIsOpenOpensInitializedApplication()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-open-");

            ProcessResult initResult = await harness.InitializeAsync().ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                AssertProcessSucceeded(initResult);
                AssertContainsAllTerms(initResult.StandardOutput, TestConstants.Application, TestConstants.InitializedCode);
            });

            ProcessResult openResult = await harness.RunApplicationAsync(TestConstants.OpenCommand, TestConstants.Password + Environment.NewLine).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                AssertProcessSucceeded(openResult);
                AssertContainsAllTerms(openResult.StandardOutput, TestConstants.Application, TestConstants.Opened);
            });
        }

        [Test]
        public async Task ProcessWhenDeleteMessageRunsInNonInteractiveJsonModeDeletesMessageFromOriginalFolder()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-delete-message-");

            ProcessResult initResult = await harness.InitializeAsync(nonInteractive: true).ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            using JsonCommandResult addAccountResult = await harness.AddDecAccountJsonAsync().ConfigureAwait(false);
            string senderAddress = ApplicationCommandTestHarness.GetAddressFromAddAccountResult(addAccountResult);

            ProcessResult sendResult = await harness.RunApplicationAsync(
                CreateSendArgs(senderAddress, senderAddress, TestConstants.DeleteMeSubject),
                TestConstants.Password + Environment.NewLine + TestConstants.HelloFromAutomation + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true,
                outputJson: true).ConfigureAwait(false);
            AssertProcessSucceeded(sendResult);

            using JsonCommandResult messagesResult = await harness.RunJsonCommandAsync(
                [TestConstants.ShowAllMessagesCommand, TestConstants.ShortPageSizeOption, TestConstants.Ten, TestConstants.ShortLimitOption, TestConstants.Ten],
                TestConstants.Password + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true).ConfigureAwait(false);

            MessageIdentity messageIdentity = ApplicationCommandTestHarness.GetFirstMessageIdentity(messagesResult);

            using JsonCommandResult deleteResult = await harness.RunJsonCommandAsync(
                CreateDeleteMessageArgs(senderAddress, messageIdentity.Folder, messageIdentity.Id, messageIdentity.Pk),
                TestConstants.Password + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true).ConfigureAwait(false);

            using JsonCommandResult folderMessagesResult = await harness.RunJsonCommandAsync(
                CreateShowFolderMessagesArgs(senderAddress, messageIdentity.Folder),
                TestConstants.Password + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true).ConfigureAwait(false);

            JsonElement deleteData = GetJsonData(deleteResult);
            JsonElement folderMessagesData = GetJsonData(folderMessagesResult);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(deleteResult, TestConstants.JsonStatusType, TestConstants.MessageDeletedCode);
                Assert.That(deleteData.GetProperty(TestConstants.JsonAccountProperty).GetString(), Is.EqualTo(senderAddress));
                Assert.That(deleteData.GetProperty(TestConstants.JsonFolderProperty).GetString(), Is.EqualTo(messageIdentity.Folder));
                Assert.That(deleteData.GetProperty(TestConstants.JsonIdProperty).GetUInt32(), Is.EqualTo(messageIdentity.Id));
                Assert.That(deleteData.GetProperty(TestConstants.JsonPkProperty).GetInt32(), Is.EqualTo(messageIdentity.Pk));
                AssertProcessSucceeded(folderMessagesResult);
                Assert.That(folderMessagesData.EnumerateArray().Any(x => x.GetProperty(TestConstants.JsonIdProperty).GetUInt32() == messageIdentity.Id && x.GetProperty(TestConstants.JsonPkProperty).GetInt32() == messageIdentity.Pk), Is.False);
            });
        }

        [Test]
        public async Task ProcessWhenListFoldersRunsInNonInteractiveJsonModeOutputsStructuredResult()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-json-folders-");

            ProcessResult initResult = await harness.InitializeAsync().ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            using JsonCommandResult addAccountResult = await harness.AddDecAccountJsonAsync().ConfigureAwait(false);
            string accountAddress = ApplicationCommandTestHarness.GetAddressFromAddAccountResult(addAccountResult);

            using JsonCommandResult listFoldersResult = await harness.RunJsonCommandAsync(
                CreateListFoldersArgs(accountAddress),
                TestConstants.Password + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true).ConfigureAwait(false);

            JsonElement listFoldersMeta = GetJsonMeta(listFoldersResult);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(listFoldersResult, TestConstants.JsonResultType, TestConstants.FoldersCode);
                Assert.That(listFoldersMeta.GetProperty(TestConstants.JsonAccountProperty).GetString(), Is.EqualTo(accountAddress));
                Assert.That(GetJsonData(listFoldersResult).ValueKind, Is.EqualTo(JsonValueKind.Array));
            });
        }

        [Test]
        public async Task ProcessWhenLockedStartupCommandRunsInNonInteractiveJsonModeWithExplicitUnlockPasswordFromStandardInputFalseOutputsStructuredWarning()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-locked-command-json-false-");

            ProcessResult initResult = await harness.InitializeAsync().ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            using JsonCommandResult listAccountsResult = await harness.RunJsonCommandAsync(
                [TestConstants.ListAccountsCommand],
                unlockPasswordFromStandardInput: false,
                nonInteractive: true).ConfigureAwait(false);

            JsonElement warningData = GetJsonData(listAccountsResult);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(listAccountsResult, TestConstants.JsonWarningType, TestConstants.StartupCommandRequiresUnlockPasswordFromStandardInputCode);
                Assert.That(warningData.GetProperty(TestConstants.JsonCommandNameProperty).GetString(), Is.EqualTo(TestConstants.ListAccountsCommand));
            });
        }

        [Test]
        public async Task ProcessWhenLockedStartupCommandRunsInNonInteractiveJsonModeWithoutUnlockPasswordFromStandardInputOutputsStructuredWarning()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-locked-command-json-");

            ProcessResult initResult = await harness.InitializeAsync().ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            using JsonCommandResult listAccountsResult = await harness.RunJsonCommandAsync(TestConstants.ListAccountsCommand, nonInteractive: true).ConfigureAwait(false);

            JsonElement warningData = GetJsonData(listAccountsResult);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(listAccountsResult, TestConstants.JsonWarningType, TestConstants.StartupCommandRequiresUnlockPasswordFromStandardInputCode);
                Assert.That(warningData.GetProperty(TestConstants.JsonCommandNameProperty).GetString(), Is.EqualTo(TestConstants.ListAccountsCommand));
            });
        }

        [Test]
        public async Task ProcessWhenLockedStartupCommandRunsInNonInteractiveJsonModeWithWrongPasswordFromStandardInputOutputsStructuredWarning()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-locked-command-invalid-password-json-");

            ProcessResult initResult = await harness.InitializeAsync().ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            using JsonCommandResult listAccountsResult = await harness.RunJsonCommandAsync(
                TestConstants.ListAccountsCommand,
                TestConstants.WrongPassword + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true).ConfigureAwait(false);

            AssertJsonTypeAndCode(listAccountsResult, TestConstants.JsonWarningType, TestConstants.InvalidPasswordCode);
        }

        [Test]
        public async Task ProcessWhenOpenRunsInNonInteractiveTextModeOnUninitializedApplicationPrintsWarningWithoutBanner()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-open-warning-text-");

            ProcessResult result = await harness.RunApplicationAsync(TestConstants.OpenCommand, nonInteractive: true).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                AssertProcessSucceeded(result);
                AssertContainsAllTerms(result.StandardOutput, TestConstants.InitializedCode);
                Assert.That(result.StandardOutput, Does.Not.Contain(TestConstants.InteractivePrompt));
            });
        }

        [Test]
        public async Task ProcessWhenOpenRunsInJsonModeAfterResetOutputsStructuredUninitializedWarning()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-open-after-reset-json-");

            ProcessResult initResult = await harness.InitializeAsync().ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            ProcessResult resetResult = await harness.RunApplicationAsync(TestConstants.ResetCommand, nonInteractive: true, assumeYes: true, outputJson: true).ConfigureAwait(false);
            AssertProcessSucceeded(resetResult);

            using JsonCommandResult openResult = await harness.RunJsonCommandAsync(TestConstants.OpenCommand, nonInteractive: true).ConfigureAwait(false);

            AssertJsonTypeAndCode(openResult, TestConstants.JsonWarningType, TestConstants.UninitializedCode);
        }

        [Test]
        public async Task ProcessWhenInitRunsInNonInteractiveJsonModeOutputsStructuredInitializedStatus()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-init-json-");

            using JsonCommandResult initResult = await harness.InitializeJsonAsync().ConfigureAwait(false);
            JsonElement initData = GetJsonData(initResult);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(initResult, TestConstants.JsonStatusType, TestConstants.InitializedCode);
                Assert.That(initData.GetProperty(TestConstants.JsonSeedPhraseProperty).ValueKind, Is.EqualTo(JsonValueKind.Array));
                Assert.That(initData.GetProperty(TestConstants.JsonSeedPhraseProperty).GetArrayLength(), Is.Positive);
            });
        }

        [Test]
        public async Task ProcessWhenInitRunsInNonInteractiveModeReadsSinglePasswordFromStandardInput()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-init-non-interactive-single-password-");

            ProcessResult initResult = await harness.InitializeAsync(nonInteractive: true).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                AssertProcessSucceeded(initResult);
                AssertContainsAllTerms(initResult.StandardOutput, TestConstants.Application, TestConstants.InitializedCode);
                Assert.That(initResult.StandardOutput, Does.Not.Contain("Confirm your vault password:"));
            });
        }

        [Test]
        public async Task ProcessWhenResetRunsInNonInteractiveModeWithoutAssumeYesShowsDomainWarningInsteadOfUnhandledException()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-reset-no-yes-");

            ProcessResult initResult = await harness.InitializeAsync().ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            ProcessResult resetResult = await harness.RunApplicationAsync(TestConstants.ResetCommand, nonInteractive: true).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                AssertProcessSucceeded(resetResult);
                Assert.That(resetResult.StandardOutput, Does.Contain(TestConstants.AssumeYesOption));
                Assert.That(resetResult.StandardOutput, Does.Not.Contain("An error has occurred:"));
                Assert.That(resetResult.StandardOutput, Does.Not.Contain("System.InvalidOperationException"));
            });
        }

        [Test]
        public async Task ProcessWhenResetRunsInNonInteractiveJsonModeWithoutAssumeYesOutputsStructuredWarning()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-reset-no-yes-json-");

            ProcessResult initResult = await harness.InitializeAsync().ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            using JsonCommandResult resetResult = await harness.RunJsonCommandAsync(TestConstants.ResetCommand, nonInteractive: true).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(resetResult, TestConstants.JsonWarningType, TestConstants.CommandRequiresAssumeYesInNonInteractiveModeCode);
                Assert.That(resetResult.RootElement.GetProperty(TestConstants.JsonMessageProperty).GetString(), Does.Contain(TestConstants.AssumeYesOption));
            });
        }

        [Test]
        public async Task ProcessWhenJsonOutputIsEnabledForListAccountsInNonInteractiveModePrintsJsonArrayOnly()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-json-accounts-");

            ProcessResult initResult = await harness.InitializeAsync().ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            using JsonCommandResult listAccountsResult = await harness.RunJsonCommandAsync(
                TestConstants.ListAccountsCommand,
                TestConstants.Password + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true).ConfigureAwait(false);

            JsonElement accountsData = GetJsonData(listAccountsResult);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(listAccountsResult, TestConstants.JsonResultType, TestConstants.AccountsCode);
                Assert.That(accountsData.ValueKind, Is.EqualTo(JsonValueKind.Array));
                Assert.That(accountsData.GetArrayLength(), Is.Zero);
            });
        }

        [Test]
        public async Task ProcessWhenResetRunsInNonInteractiveModeWithYesAndJsonOutputsStructuredStatus()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-reset-json-");

            ProcessResult initResult = await harness.InitializeAsync().ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            using JsonCommandResult resetResult = await harness.RunJsonCommandAsync(TestConstants.ResetCommand, nonInteractive: true, assumeYes: true).ConfigureAwait(false);

            AssertJsonTypeAndCode(resetResult, TestConstants.JsonStatusType, TestConstants.ResetCode);
        }

        [Test]
        public async Task ProcessWhenOpenRunsInJsonModeOnUninitializedApplicationOutputsStructuredWarning()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-open-warning-json-");

            using JsonCommandResult result = await harness.RunJsonCommandAsync(TestConstants.OpenCommand, TestConstants.Password + Environment.NewLine, nonInteractive: true).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(result, TestConstants.JsonWarningType, TestConstants.UninitializedCode);
                Assert.That(GetJsonMessage(result), Does.Contain("hasn't been initialized yet"));
            });
        }

        [Test]
        public async Task ProcessWhenLockedStartupCommandIsLaunchedInInteractiveModeWithoutUnlockPasswordFromStandardInputPromptsForPasswordAndExecutesCommand()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-locked-command-");

            ProcessResult initResult = await harness.InitializeAsync().ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            ProcessResult listAccountsResult = await harness.RunApplicationAsync(TestConstants.ListAccountsCommand, TestConstants.Password + Environment.NewLine).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                AssertProcessSucceeded(listAccountsResult);
                Assert.That(CountOccurrences(listAccountsResult.StandardOutput, TestConstants.VaultPasswordPrompt), Is.Not.Zero, listAccountsResult.StandardOutput);
                AssertContainsAllTerms(listAccountsResult.StandardOutput, TestConstants.AccountsCode);
            });
        }

        [Test]
        public async Task ProcessWhenLockedStartupCommandIsLaunchedInNonInteractiveModeWithoutUnlockPasswordFromStandardInputShowsHintAndDoesNotExecuteCommand()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-locked-command-non-interactive-");

            ProcessResult initResult = await harness.InitializeAsync().ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            ProcessResult listAccountsResult = await harness.RunApplicationAsync(TestConstants.ListAccountsCommand, nonInteractive: true).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                AssertProcessSucceeded(listAccountsResult);
                Assert.That(listAccountsResult.StandardOutput, Does.Contain(TestConstants.UnlockPasswordStdinOption));
                Assert.That(listAccountsResult.StandardOutput, Does.Not.Contain("There are no accounts yet."));
            });
        }

        [Test]
        public async Task ProcessWhenUnlockPasswordFromStandardInputIsEnabledInInteractiveModeStillPromptsForPasswordAndExecutesCommand()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-unlock-");

            ProcessResult initResult = await harness.InitializeAsync().ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            ProcessResult listAccountsResult = await harness.RunApplicationAsync(TestConstants.ListAccountsCommand, TestConstants.Password + Environment.NewLine, unlockPasswordFromStandardInput: true).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                AssertProcessSucceeded(listAccountsResult);
                Assert.That(CountOccurrences(listAccountsResult.StandardOutput, TestConstants.VaultPasswordPrompt), Is.Not.Zero, listAccountsResult.StandardOutput);
                AssertContainsAllTerms(listAccountsResult.StandardOutput, TestConstants.AccountsCode);
            });
        }

        [Test]
        public async Task ProcessWhenUnlockPasswordFromStandardInputIsEnabledInNonInteractiveModeOmitsPromptBannerAndGoodbye()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-non-interactive-");

            ProcessResult initResult = await harness.InitializeAsync().ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            ProcessResult listAccountsResult = await harness.RunApplicationAsync(TestConstants.ListAccountsCommand, TestConstants.Password + Environment.NewLine, unlockPasswordFromStandardInput: true, nonInteractive: true).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                AssertProcessSucceeded(listAccountsResult);
                AssertContainsAllTerms(listAccountsResult.StandardOutput, TestConstants.AccountsCode);
                Assert.That(listAccountsResult.StandardOutput, Does.Not.Contain(TestConstants.VaultPasswordPrompt));
                Assert.That(listAccountsResult.StandardOutput, Does.Not.Contain(TestConstants.InteractivePrompt));
            });
        }

        [Test]
        public async Task ProcessWhenAddProtonAccountRunsInNonInteractiveModeOmitsAccountAddressPrompt()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-proton-non-interactive-");

            ProcessResult initResult = await harness.InitializeAsync(nonInteractive: true).ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            ProcessResult addAccountResult = await harness.RunApplicationAsync(
                [TestConstants.AddAccountCommand, TestConstants.ShortTypeOption, TestConstants.ProtonAccountType],
                TestConstants.Password + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true,
                outputJson: true).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                AssertProcessSucceeded(addAccountResult);
                AssertOutputOmitsSetupPrompts(addAccountResult.StandardOutput);
            });
        }

        [Test]
        public async Task ProcessWhenAddProtonAccountRunsWithInputJsonStdinAndInvalidJsonOutputsStructuredError()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-proton-json-invalid-");

            ProcessResult initResult = await harness.InitializeAsync(nonInteractive: true).ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            using JsonCommandResult addAccountResult = await harness.RunJsonCommandAsync(
                [TestConstants.AddAccountCommand, TestConstants.ShortTypeOption, TestConstants.ProtonAccountType, TestConstants.InputJsonStdinOption],
                TestConstants.Password + Environment.NewLine + "{not-json}" + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(addAccountResult, TestConstants.JsonErrorType, TestConstants.StructuredStandardInputInvalidJsonCode, expectSuccess: false);
                AssertOutputOmitsSetupPrompts(addAccountResult.ProcessResult.StandardOutput);
            });
        }

        [Test]
        public async Task ProcessWhenAddProtonAccountRunsWithInputJsonStdinAndMissingEmailOutputsStructuredError()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-proton-json-missing-email-");

            ProcessResult initResult = await harness.InitializeAsync(nonInteractive: true).ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            using JsonCommandResult addAccountResult = await harness.RunJsonCommandAsync(
                [TestConstants.AddAccountCommand, TestConstants.ShortTypeOption, TestConstants.ProtonAccountType, TestConstants.InputJsonStdinOption],
                TestConstants.Password + Environment.NewLine + "{\"accountPassword\":\"secret\"}" + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true).ConfigureAwait(false);

            JsonElement errorData = GetJsonData(addAccountResult);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(addAccountResult, TestConstants.JsonErrorType, TestConstants.StructuredStandardInputMissingPropertyCode, expectSuccess: false);
                Assert.That(errorData.GetProperty(TestConstants.JsonPropertyNameProperty).GetString(), Is.EqualTo(TestConstants.EmailPropertyName));
                AssertOutputOmitsSetupPrompts(addAccountResult.ProcessResult.StandardOutput);
            });
        }

        [Test]
        public async Task ProcessWhenAddEmailAccountRunsWithInputJsonStdinAndInvalidJsonOutputsStructuredError()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-email-json-invalid-");

            ProcessResult initResult = await harness.InitializeAsync(nonInteractive: true).ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            using JsonCommandResult addAccountResult = await harness.RunJsonCommandAsync(
                [TestConstants.AddAccountCommand, TestConstants.ShortTypeOption, TestConstants.EmailAccountCommandType, TestConstants.InputJsonStdinOption],
                TestConstants.Password + Environment.NewLine + "{not-json}" + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(addAccountResult, TestConstants.JsonErrorType, TestConstants.StructuredStandardInputInvalidJsonCode, expectSuccess: false);
                AssertOutputOmitsSetupPrompts(addAccountResult.ProcessResult.StandardOutput);
            });
        }

        [Test]
        public async Task ProcessWhenAddEmailAccountRunsWithInputJsonStdinAndMissingImapServerOutputsStructuredError()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-email-json-missing-imap-server-");

            ProcessResult initResult = await harness.InitializeAsync(nonInteractive: true).ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            using JsonCommandResult addAccountResult = await harness.RunJsonCommandAsync(
                [TestConstants.AddAccountCommand, TestConstants.ShortTypeOption, TestConstants.EmailAccountCommandType, TestConstants.InputJsonStdinOption],
                TestConstants.Password + Environment.NewLine + $"{{\"email\":\"{TestConstants.UserAddress}\",\"accountPassword\":\"secret\",\"imapPort\":993,\"smtpServer\":\"smtp.example.com\",\"smtpPort\":465}}" + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true).ConfigureAwait(false);

            JsonElement errorData = GetJsonData(addAccountResult);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(addAccountResult, TestConstants.JsonErrorType, TestConstants.StructuredStandardInputMissingPropertyCode, expectSuccess: false);
                Assert.That(errorData.GetProperty(TestConstants.JsonPropertyNameProperty).GetString(), Is.EqualTo(TestConstants.ImapServerPropertyName));
                AssertOutputOmitsSetupPrompts(addAccountResult.ProcessResult.StandardOutput);
            });
        }

        [Test]
        public async Task ProcessWhenUnhandledExceptionRunsInNonInteractiveJsonModeKeepsStandardErrorEmptyAndStdoutJsonParseable()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-unhandled-json-");

            ProcessResult initResult = await harness.InitializeAsync(nonInteractive: true).ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            using JsonCommandResult sendResult = await harness.RunJsonCommandAsync(
                [TestConstants.SendCommand, TestConstants.ShortPageSizeOption, TestConstants.MissingAddress, TestConstants.ShortRecipientOption, TestConstants.MissingAddress, TestConstants.ShortTypeOption, TestConstants.MissingAccountSubject],
                TestConstants.Password + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true).ConfigureAwait(false);

            JsonElement errorData = GetJsonData(sendResult);

            Assert.Multiple(() =>
            {
                AssertProcessSucceeded(sendResult);
                Assert.That(sendResult.ProcessResult.StandardError, Is.Empty);
                Assert.That(sendResult.RootElement.GetProperty(TestConstants.JsonTypeProperty).GetString(), Is.EqualTo(TestConstants.JsonErrorType));
                Assert.That(sendResult.RootElement.GetProperty(TestConstants.JsonCodeProperty).GetString(), Is.EqualTo(TestConstants.UnhandledExceptionCode));
                Assert.That(errorData.GetProperty(TestConstants.JsonExceptionTypeProperty).GetString(), Is.EqualTo("Tuvi.Core.Entities.AccountIsNotExistInDatabaseException"));
            });
        }

        [Test]
        public async Task ProcessWhenSendRunsInNonInteractiveJsonModeWithoutBodySendsMessageWithEmptyBody()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-send-missing-eof-");

            ProcessResult initResult = await harness.InitializeAsync(nonInteractive: true).ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            using JsonCommandResult addAccountResult = await harness.AddDecAccountJsonAsync().ConfigureAwait(false);
            string senderAddress = ApplicationCommandTestHarness.GetAddressFromAddAccountResult(addAccountResult);

            using JsonCommandResult sendResult = await harness.RunJsonCommandAsync(
                CreateSendArgs(senderAddress, senderAddress, TestConstants.MissingBodySubject),
                TestConstants.Password + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true).ConfigureAwait(false);

            JsonElement sendData = GetJsonData(sendResult);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(sendResult, TestConstants.JsonStatusType, TestConstants.MessageSentCode);
                Assert.That(sendData.GetProperty(TestConstants.JsonSubjectProperty).GetString(), Is.EqualTo(TestConstants.MissingBodySubject));
            });
        }

        [Test]
        public async Task ProcessWhenSendRunsInNonInteractiveJsonModeWithoutExplicitEofButWithBodySendsMessage()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-send-closed-stdin-");

            ProcessResult initResult = await harness.InitializeAsync(nonInteractive: true).ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            using JsonCommandResult addAccountResult = await harness.AddDecAccountJsonAsync().ConfigureAwait(false);
            string senderAddress = ApplicationCommandTestHarness.GetAddressFromAddAccountResult(addAccountResult);

            using JsonCommandResult sendResult = await harness.RunJsonCommandAsync(
                CreateSendArgs(senderAddress, senderAddress, TestConstants.ClosedStdinSubject),
                TestConstants.Password + Environment.NewLine + TestConstants.HelloFromAutomation + Environment.NewLine + TestConstants.SecondBodyLine + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true).ConfigureAwait(false);

            JsonElement sendData = GetJsonData(sendResult);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(sendResult, TestConstants.JsonStatusType, TestConstants.MessageSentCode);
                Assert.That(sendData.GetProperty(TestConstants.JsonSubjectProperty).GetString(), Is.EqualTo(TestConstants.ClosedStdinSubject));
            });
        }

        [Test]
        public async Task ProcessWhenDeleteMessageRunsInNonInteractiveJsonModeMovesMessageToTrashThenDeletesItPermanently()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-delete-message-");

            ProcessResult initResult = await harness.InitializeAsync(nonInteractive: true).ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            using JsonCommandResult addAccountResult = await harness.AddDecAccountJsonAsync().ConfigureAwait(false);
            string senderAddress = ApplicationCommandTestHarness.GetAddressFromAddAccountResult(addAccountResult);

            using JsonCommandResult foldersResult = await harness.RunJsonCommandAsync(
                CreateListFoldersArgs(senderAddress),
                TestConstants.Password + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true).ConfigureAwait(false);

            string trashFolder = ApplicationCommandTestHarness.GetTrashFolderName(foldersResult);

            ProcessResult sendResult = await harness.RunApplicationAsync(
                CreateSendArgs(senderAddress, senderAddress, TestConstants.DeleteMeSubject),
                TestConstants.Password + Environment.NewLine + TestConstants.HelloFromAutomation + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true,
                outputJson: true).ConfigureAwait(false);
            AssertProcessSucceeded(sendResult);

            using JsonCommandResult messagesResult = await harness.RunJsonCommandAsync(
                [TestConstants.ShowAllMessagesCommand, TestConstants.ShortPageSizeOption, TestConstants.Ten, TestConstants.ShortLimitOption, TestConstants.Ten],
                TestConstants.Password + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true).ConfigureAwait(false);

            MessageIdentity messageIdentity = ApplicationCommandTestHarness.GetFirstMessageIdentity(messagesResult);

            using JsonCommandResult deleteFromSourceResult = await harness.RunJsonCommandAsync(
                CreateDeleteMessageArgs(senderAddress, messageIdentity.Folder, messageIdentity.Id, messageIdentity.Pk),
                TestConstants.Password + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true).ConfigureAwait(false);

            using JsonCommandResult sourceFolderMessagesResult = await harness.RunJsonCommandAsync(
                CreateShowFolderMessagesArgs(senderAddress, messageIdentity.Folder),
                TestConstants.Password + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true).ConfigureAwait(false);

            using JsonCommandResult trashFolderMessagesResult = await harness.RunJsonCommandAsync(
                CreateShowFolderMessagesArgs(senderAddress, trashFolder),
                TestConstants.Password + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true).ConfigureAwait(false);

            JsonElement movedMessage = trashFolderMessagesResult.RootElement.GetProperty(TestConstants.JsonDataProperty)
                .EnumerateArray()
                .First(x => string.Equals(x.GetProperty(TestConstants.JsonSubjectProperty).GetString(), TestConstants.DeleteMeSubject, StringComparison.Ordinal));

            uint trashId = movedMessage.GetProperty(TestConstants.JsonIdProperty).GetUInt32();
            int trashPk = movedMessage.GetProperty(TestConstants.JsonPkProperty).GetInt32();

            using JsonCommandResult deleteFromTrashResult = await harness.RunJsonCommandAsync(
                CreateDeleteMessageArgs(senderAddress, trashFolder, trashId, trashPk),
                TestConstants.Password + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true).ConfigureAwait(false);

            using JsonCommandResult trashFolderAfterPermanentDeleteResult = await harness.RunJsonCommandAsync(
                CreateShowFolderMessagesArgs(senderAddress, trashFolder),
                TestConstants.Password + Environment.NewLine,
                unlockPasswordFromStandardInput: true,
                nonInteractive: true).ConfigureAwait(false);

            JsonElement deleteFromSourceData = GetJsonData(deleteFromSourceResult);
            JsonElement sourceFolderMessagesData = GetJsonData(sourceFolderMessagesResult);
            JsonElement trashFolderAfterDeleteData = GetJsonData(trashFolderAfterPermanentDeleteResult);
            JsonElement deleteFromTrashData = GetJsonData(deleteFromTrashResult);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(deleteFromSourceResult, TestConstants.JsonStatusType, TestConstants.MessageDeletedCode);
                Assert.That(deleteFromSourceData.GetProperty(TestConstants.JsonFolderProperty).GetString(), Is.EqualTo(messageIdentity.Folder));

                AssertProcessSucceeded(sourceFolderMessagesResult);
                Assert.That(sourceFolderMessagesData.EnumerateArray().Any(x => x.GetProperty(TestConstants.JsonIdProperty).GetUInt32() == messageIdentity.Id && x.GetProperty(TestConstants.JsonPkProperty).GetInt32() == messageIdentity.Pk), Is.False);

                AssertProcessSucceeded(trashFolderMessagesResult);
                Assert.That(string.Equals(movedMessage.GetProperty(TestConstants.JsonFolderProperty).GetString(), trashFolder, StringComparison.Ordinal), Is.True);
                Assert.That(string.Equals(movedMessage.GetProperty(TestConstants.JsonSubjectProperty).GetString(), TestConstants.DeleteMeSubject, StringComparison.Ordinal), Is.True);

                AssertJsonTypeAndCode(deleteFromTrashResult, TestConstants.JsonStatusType, TestConstants.MessageDeletedCode);
                Assert.That(deleteFromTrashData.GetProperty(TestConstants.JsonFolderProperty).GetString(), Is.EqualTo(trashFolder));

                AssertProcessSucceeded(trashFolderAfterPermanentDeleteResult);
                Assert.That(trashFolderAfterDeleteData.EnumerateArray().Any(x => string.Equals(x.GetProperty(TestConstants.JsonSubjectProperty).GetString(), TestConstants.DeleteMeSubject, StringComparison.Ordinal)), Is.False);
            });
        }

        [Test]
        public async Task ProcessWhenResetRunsInNonInteractiveModeWithYesResetsWithoutPromptBannerOrGoodbye()
        {
            using ApplicationCommandTestHarness harness = ApplicationCommandTestHarness.Create("eppie-cli-reset-non-interactive-");

            ProcessResult initResult = await harness.InitializeAsync().ConfigureAwait(false);
            AssertProcessSucceeded(initResult);

            ProcessResult resetResult = await harness.RunApplicationAsync(TestConstants.ResetCommand, nonInteractive: true, assumeYes: true).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                AssertProcessSucceeded(resetResult);
                AssertContainsAllTerms(resetResult.StandardOutput, TestConstants.Application, TestConstants.ResetCommand);
                Assert.That(resetResult.StandardOutput, Does.Not.Contain(TestConstants.InteractivePrompt));
            });
        }

        private static int CountOccurrences(string text, string value)
        {
            ArgumentNullException.ThrowIfNull(text);
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            int count = 0;
            int index = 0;

            while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += value.Length;
            }

            return count;
        }

        private static string[] CreateDeleteMessageArgs(string account, string folder, uint id, int pk)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(account);
            ArgumentException.ThrowIfNullOrWhiteSpace(folder);

            return
            [
                TestConstants.DeleteMessageCommand,
                TestConstants.ShortAccountOption, account,
                TestConstants.ShortFolderOption, folder,
                TestConstants.ShortIdOption, id.ToString(CultureInfo.InvariantCulture),
                TestConstants.ShortPrimaryKeyOption, pk.ToString(CultureInfo.InvariantCulture)
            ];
        }

        private static string[] CreateListFoldersArgs(string account)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(account);

            return [TestConstants.ListFoldersCommand, TestConstants.ShortAccountOption, account];
        }

        private static string[] CreateSendArgs(string sender, string recipient, string subject)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sender);
            ArgumentException.ThrowIfNullOrWhiteSpace(recipient);
            ArgumentException.ThrowIfNullOrWhiteSpace(subject);

            return
            [
                TestConstants.SendCommand,
                TestConstants.ShortPageSizeOption, sender,
                TestConstants.ShortRecipientOption, recipient,
                TestConstants.ShortTypeOption, subject
            ];
        }

        private static string[] CreateShowFolderMessagesArgs(string account, string folder)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(account);
            ArgumentException.ThrowIfNullOrWhiteSpace(folder);

            return
            [
                TestConstants.ShowFolderMessagesCommand,
                TestConstants.ShortAccountOption, account,
                TestConstants.ShortFolderOption, folder,
                TestConstants.ShortPageSizeOption, TestConstants.Ten,
                TestConstants.ShortLimitOption, TestConstants.Ten
            ];
        }

        private static void AssertProcessSucceeded(ProcessResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            Assert.That(result.ExitCode, Is.Zero, result.StandardError);
        }

        private static void AssertProcessSucceeded(JsonCommandResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            AssertProcessSucceeded(result.ProcessResult);
        }

        private static void AssertProcessFailed(ProcessResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            Assert.That(result.ExitCode, Is.Not.Zero, result.StandardError);
        }

        private static void AssertProcessFailed(JsonCommandResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            AssertProcessFailed(result.ProcessResult);
        }

        private static void AssertJsonTypeAndCode(JsonCommandResult result, string expectedType, string expectedCode)
        {
            AssertJsonTypeAndCode(result, expectedType, expectedCode, expectSuccess: true);
        }

        private static void AssertJsonTypeAndCode(JsonCommandResult result, string expectedType, string expectedCode, bool expectSuccess)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentException.ThrowIfNullOrWhiteSpace(expectedType);
            ArgumentException.ThrowIfNullOrWhiteSpace(expectedCode);

            Assert.Multiple(() =>
            {
                if (expectSuccess)
                {
                    AssertProcessSucceeded(result);
                }
                else
                {
                    AssertProcessFailed(result);
                }

                Assert.That(result.RootElement.GetProperty(TestConstants.JsonTypeProperty).GetString(), Is.EqualTo(expectedType));
                Assert.That(result.RootElement.GetProperty(TestConstants.JsonCodeProperty).GetString(), Is.EqualTo(expectedCode));
            });
        }

        private static JsonElement GetJsonData(JsonCommandResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            return result.RootElement.GetProperty(TestConstants.JsonDataProperty);
        }

        private static JsonElement GetJsonMeta(JsonCommandResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            return result.RootElement.GetProperty(TestConstants.JsonMetaProperty);
        }

        private static string? GetJsonMessage(JsonCommandResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            return result.RootElement.GetProperty(TestConstants.JsonMessageProperty).GetString();
        }

        private static void AssertOutputOmitsSetupPrompts(string output)
        {
            ArgumentNullException.ThrowIfNull(output);

            Assert.Multiple(() =>
            {
                Assert.That(output, Does.Not.Contain(TestConstants.EnterAccountAddressPrompt));
                Assert.That(output, Does.Not.Contain(TestConstants.VaultPasswordPrompt));
            });
        }

        private static void AssertContainsAllTerms(string text, params string[] terms)
        {
            ArgumentNullException.ThrowIfNull(text);
            ArgumentNullException.ThrowIfNull(terms);

            foreach (string term in terms)
            {
                Assert.That(text, Does.Contain(term).IgnoreCase, $"Expected output to contain '{term}'. Actual output:{Environment.NewLine}{text}");
            }
        }
    }
}
