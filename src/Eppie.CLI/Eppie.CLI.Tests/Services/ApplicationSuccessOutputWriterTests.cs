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

using System.Text.Json;

using Eppie.CLI.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

using Tuvi.Core.Entities;

namespace Eppie.CLI.Tests.Services
{
    [TestFixture]
    public class ApplicationSuccessOutputWriterTests
    {
        private ServiceProvider _serviceProvider = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _serviceProvider = new ServiceCollection()
                .AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance)
                .AddLocalization()
                .BuildServiceProvider();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _serviceProvider.Dispose();
        }

        [Test]
        public void JsonWriterWhenAccountAddedOutputsStructuredResult()
        {
            using JsonDocument document = CaptureJsonDocument(writer => writer.Write(new AccountAddedOutput(TestConstants.AccountAddress, TestConstants.DecAccountType)));
            JsonElement data = GetData(document);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(document, TestConstants.JsonResultType, TestConstants.AccountAddedCode);
                Assert.That(data.GetProperty(TestConstants.JsonAddressProperty).GetString(), Is.EqualTo(TestConstants.AccountAddress));
                Assert.That(data.GetProperty(TestConstants.JsonAccountTypeProperty).GetString(), Is.EqualTo(TestConstants.DecAccountType));
            });
        }

        [Test]
        public void JsonWriterWhenAccountsOutputOutputsStructuredResult()
        {
            Account account = new() { Id = 1, Email = new EmailAddress(TestConstants.AccountAddress), Type = MailBoxType.Email };
            using JsonDocument document = CaptureJsonDocument(writer => writer.Write(new AccountsOutput([account])));
            JsonElement firstAccount = GetFirstDataItem(document);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(document, TestConstants.JsonResultType, TestConstants.AccountsCode);
                Assert.That(firstAccount.GetProperty(TestConstants.JsonIdProperty).GetInt32(), Is.EqualTo(1));
                Assert.That(firstAccount.GetProperty(TestConstants.JsonAddressProperty).GetString(), Is.EqualTo(TestConstants.AccountAddress));
                Assert.That(firstAccount.GetProperty(TestConstants.JsonAccountTypeProperty).GetString(), Is.EqualTo(TestConstants.EmailAccountType));
            });
        }

        [Test]
        public void JsonWriterWhenContactsOutputOutputsStructuredResult()
        {
            Contact contact = CreateContact(1);
            using JsonDocument document = CaptureJsonDocument(writer => writer.Write(new ContactsOutput([contact])));
            JsonElement firstContact = GetFirstDataItem(document);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(document, TestConstants.JsonResultType, TestConstants.ContactsCode);
                Assert.That(firstContact.GetProperty(TestConstants.JsonIdProperty).GetInt32(), Is.EqualTo(1));
                Assert.That(firstContact.GetProperty(TestConstants.JsonAddressProperty).GetString(), Is.EqualTo($"{TestConstants.ContactAddressPrefix}{TestConstants.One}{TestConstants.ContactAddressDomain}"));
                Assert.That(firstContact.GetProperty(TestConstants.JsonFullNameProperty).GetString(), Is.EqualTo(TestConstants.Contact1FullName));
                Assert.That(firstContact.GetProperty(TestConstants.JsonUnreadCountProperty).GetInt32(), Is.EqualTo(1));
            });
        }

        [Test]
        public void JsonWriterWhenFoldersOutputOutputsStructuredResult()
        {
            Folder folder = CreateFolder(TestConstants.AllSent, FolderAttributes.Sent | FolderAttributes.All, unreadCount: 2, totalCount: 5);
            using JsonDocument document = CaptureJsonDocument(writer => writer.Write(new FoldersOutput(TestConstants.AccountAddress, [folder])));
            JsonElement meta = GetMeta(document);
            JsonElement firstFolder = GetFirstDataItem(document);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(document, TestConstants.JsonResultType, TestConstants.FoldersCode);
                Assert.That(meta.GetProperty(TestConstants.JsonAccountProperty).GetString(), Is.EqualTo(TestConstants.AccountAddress));
                Assert.That(firstFolder.GetProperty(TestConstants.JsonFullNameProperty).GetString(), Is.EqualTo(TestConstants.AllSent));
                Assert.That(firstFolder.GetProperty(TestConstants.JsonUnreadCountProperty).GetInt32(), Is.EqualTo(2));
                Assert.That(firstFolder.GetProperty(TestConstants.JsonTotalCountProperty).GetInt32(), Is.EqualTo(5));
                Assert.That(firstFolder.GetProperty(TestConstants.JsonRolesProperty)[0].GetString(), Is.EqualTo(TestConstants.SentRole));
                Assert.That(firstFolder.GetProperty(TestConstants.JsonRolesProperty)[1].GetString(), Is.EqualTo(TestConstants.AllRole));
            });
        }

        [Test]
        public void JsonWriterWhenMessageOutputIsNonCompactOutputsStructuredResult()
        {
            Message message = CreateMessage(subject: TestConstants.Subject1);
            using JsonDocument document = CaptureJsonDocument(writer => writer.Write(new MessageOutput(message, Compact: false)));
            JsonElement data = GetData(document);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(document, TestConstants.JsonResultType, TestConstants.MessageCode);
                Assert.That(data.GetProperty(TestConstants.JsonSubjectProperty).GetString(), Is.EqualTo(TestConstants.Subject1));
                Assert.That(data.GetProperty(TestConstants.JsonTextBodyProperty).GetString(), Is.EqualTo(TestConstants.TextBody));
                Assert.That(data.GetProperty(TestConstants.JsonToProperty)[0].GetString(), Is.EqualTo(TestConstants.ToAddress));
                Assert.That(data.GetProperty(TestConstants.JsonFromProperty)[0].GetString(), Is.EqualTo(TestConstants.FromAddress));
            });
        }

        [Test]
        public void JsonWriterWhenMessageSentOutputsStructuredStatus()
        {
            using JsonDocument document = CaptureJsonDocument(writer => writer.Write(new MessageSentOutput(TestConstants.ReplyTestSubject, TestConstants.ToAddress, TestConstants.FromAddress)));
            JsonElement data = GetData(document);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(document, TestConstants.JsonStatusType, TestConstants.MessageSentCode);
                Assert.That(data.GetProperty(TestConstants.JsonSubjectProperty).GetString(), Is.EqualTo(TestConstants.ReplyTestSubject));
                Assert.That(data.GetProperty(TestConstants.JsonToProperty).GetString(), Is.EqualTo(TestConstants.ToAddress));
                Assert.That(data.GetProperty(TestConstants.JsonFromProperty).GetString(), Is.EqualTo(TestConstants.FromAddress));
            });
        }

        [Test]
        public void JsonWriterWhenFolderSyncedOutputsStructuredStatus()
        {
            using JsonDocument document = CaptureJsonDocument(writer => writer.Write(new FolderSyncedOutput(TestConstants.AccountAddress, TestConstants.Inbox)));
            JsonElement data = GetData(document);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(document, TestConstants.JsonStatusType, TestConstants.FolderSyncedCode);
                Assert.That(data.GetProperty(TestConstants.JsonAccountProperty).GetString(), Is.EqualTo(TestConstants.AccountAddress));
                Assert.That(data.GetProperty(TestConstants.JsonFolderProperty).GetString(), Is.EqualTo(TestConstants.Inbox));
            });
        }

        [Test]
        public void JsonWriterWhenMessageDeletedOutputsStructuredStatus()
        {
            using JsonDocument document = CaptureJsonDocument(writer => writer.Write(new MessageDeletedOutput(TestConstants.AccountAddress, TestConstants.Inbox, 42, 7)));
            JsonElement data = GetData(document);

            Assert.Multiple(() =>
            {
                AssertJsonTypeAndCode(document, TestConstants.JsonStatusType, TestConstants.MessageDeletedCode);
                Assert.That(data.GetProperty(TestConstants.JsonAccountProperty).GetString(), Is.EqualTo(TestConstants.AccountAddress));
                Assert.That(data.GetProperty(TestConstants.JsonFolderProperty).GetString(), Is.EqualTo(TestConstants.Inbox));
                Assert.That(data.GetProperty(TestConstants.JsonIdProperty).GetUInt32(), Is.EqualTo(42));
                Assert.That(data.GetProperty(TestConstants.JsonPkProperty).GetInt32(), Is.EqualTo(7));
            });
        }

        [Test]
        public void JsonWriterWhenStartupCommandRequiresUnlockPasswordFromStandardInputOutputsStructuredWarning()
        {
            string json = CaptureConsoleOutput(() => CreateJsonWriter().Write(new StartupCommandRequiresUnlockPasswordFromStandardInputWarningOutput(TestConstants.ListAccountsCommand)));

            using JsonDocument document = JsonDocument.Parse(json);

            Assert.Multiple(() =>
            {
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonTypeProperty).GetString(), Is.EqualTo(TestConstants.JsonWarningType));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonCodeProperty).GetString(), Is.EqualTo(TestConstants.StartupCommandRequiresUnlockPasswordFromStandardInputCode));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonMessageProperty).GetString(), Does.Contain(TestConstants.UnlockPasswordStdinOption));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonDataProperty).GetProperty(TestConstants.JsonCommandNameProperty).GetString(), Is.EqualTo(TestConstants.ListAccountsCommand));
            });
        }

        [Test]
        public void JsonWriterWhenNonInteractiveOperationIsNotSupportedOutputsStructuredError()
        {
            string json = CaptureConsoleOutput(() => CreateJsonWriter().Write(new NonInteractiveOperationNotSupportedErrorOutput(TestConstants.InteractiveMenu)));

            using JsonDocument document = JsonDocument.Parse(json);

            Assert.Multiple(() =>
            {
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonTypeProperty).GetString(), Is.EqualTo(TestConstants.JsonErrorType));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonCodeProperty).GetString(), Is.EqualTo(TestConstants.NonInteractiveOperationNotSupportedCode));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonMessageProperty).GetString(), Does.Contain(TestConstants.InteractiveMenu));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonDataProperty).GetProperty(TestConstants.JsonOperationProperty).GetString(), Is.EqualTo(TestConstants.InteractiveMenu));
            });
        }

        [Test]
        public void JsonWriterWhenApplicationInitializedOutputsSeedPhraseStatus()
        {
            string json = CaptureConsoleOutput(() => CreateJsonWriter().Write(new ApplicationInitializedOutput(["alpha", "beta", "gamma"])));

            using JsonDocument document = JsonDocument.Parse(json);

            Assert.Multiple(() =>
            {
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonTypeProperty).GetString(), Is.EqualTo(TestConstants.JsonStatusType));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonCodeProperty).GetString(), Is.EqualTo(TestConstants.InitializedCode));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonDataProperty).GetProperty(TestConstants.JsonSeedPhraseProperty)[0].GetString(), Is.EqualTo("alpha"));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonDataProperty).GetProperty(TestConstants.JsonSeedPhraseProperty)[1].GetString(), Is.EqualTo("beta"));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonDataProperty).GetProperty(TestConstants.JsonSeedPhraseProperty)[2].GetString(), Is.EqualTo("gamma"));
            });
        }

        [Test]
        public void JsonWriterWhenApplicationOpenedOutputsStructuredStatus()
        {
            string json = CaptureConsoleOutput(() => CreateJsonWriter().Write(new ApplicationOpenedOutput()));

            using JsonDocument document = JsonDocument.Parse(json);

            Assert.That(document.RootElement.GetProperty(TestConstants.JsonTypeProperty).GetString(), Is.EqualTo(TestConstants.JsonStatusType));
            Assert.That(document.RootElement.GetProperty(TestConstants.JsonCodeProperty).GetString(), Is.EqualTo(TestConstants.Opened));
        }

        [Test]
        public void JsonWriterWhenApplicationRestoredOutputsStructuredStatus()
        {
            string json = CaptureConsoleOutput(() => CreateJsonWriter().Write(new ApplicationRestoredOutput()));

            using JsonDocument document = JsonDocument.Parse(json);

            Assert.That(document.RootElement.GetProperty(TestConstants.JsonTypeProperty).GetString(), Is.EqualTo(TestConstants.JsonStatusType));
            Assert.That(document.RootElement.GetProperty(TestConstants.JsonCodeProperty).GetString(), Is.EqualTo(TestConstants.Restored));
        }

        [Test]
        public void JsonWriterWhenAuthorizationCanceledOutputsStructuredStatus()
        {
            string json = CaptureConsoleOutput(() => CreateJsonWriter().Write(new AuthorizationCanceledOutput()));

            using JsonDocument document = JsonDocument.Parse(json);

            Assert.That(document.RootElement.GetProperty(TestConstants.JsonTypeProperty).GetString(), Is.EqualTo(TestConstants.JsonStatusType));
            Assert.That(document.RootElement.GetProperty(TestConstants.JsonCodeProperty).GetString(), Is.EqualTo(TestConstants.AuthorizationCanceledCode));
        }

        [Test]
        public void JsonWriterWhenAuthorizationToServiceOutputsStructuredStatus()
        {
            string json = CaptureConsoleOutput(() => CreateJsonWriter().Write(new AuthorizationToServiceOutput(TestConstants.ProtonServiceName)));

            using JsonDocument document = JsonDocument.Parse(json);

            Assert.Multiple(() =>
            {
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonTypeProperty).GetString(), Is.EqualTo(TestConstants.JsonStatusType));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonCodeProperty).GetString(), Is.EqualTo(TestConstants.AuthorizationStartedCode));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonDataProperty).GetProperty(TestConstants.JsonServiceNameProperty).GetString(), Is.EqualTo(TestConstants.ProtonServiceName));
            });
        }

        [Test]
        public void JsonWriterWhenAuthorizationCompletedOutputsStructuredStatus()
        {
            string json = CaptureConsoleOutput(() => CreateJsonWriter().Write(new AuthorizationCompletedOutput()));

            using JsonDocument document = JsonDocument.Parse(json);

            Assert.That(document.RootElement.GetProperty(TestConstants.JsonTypeProperty).GetString(), Is.EqualTo(TestConstants.JsonStatusType));
            Assert.That(document.RootElement.GetProperty(TestConstants.JsonCodeProperty).GetString(), Is.EqualTo(TestConstants.AuthorizationCompletedCode));
        }

        [Test]
        public void JsonWriterWhenApplicationResetOutputsStructuredStatus()
        {
            string json = CaptureConsoleOutput(() => CreateJsonWriter().Write(new ApplicationResetOutput()));

            using JsonDocument document = JsonDocument.Parse(json);

            Assert.That(document.RootElement.GetProperty(TestConstants.JsonTypeProperty).GetString(), Is.EqualTo(TestConstants.JsonStatusType));
            Assert.That(document.RootElement.GetProperty(TestConstants.JsonCodeProperty).GetString(), Is.EqualTo(TestConstants.ResetCode));
        }

        [Test]
        public void JsonWriterWhenInvalidPasswordOutputsStructuredWarning()
        {
            string json = CaptureConsoleOutput(() => CreateJsonWriter().Write(new InvalidPasswordWarningOutput()));

            using JsonDocument document = JsonDocument.Parse(json);

            Assert.That(document.RootElement.GetProperty(TestConstants.JsonTypeProperty).GetString(), Is.EqualTo(TestConstants.JsonWarningType));
            Assert.That(document.RootElement.GetProperty(TestConstants.JsonCodeProperty).GetString(), Is.EqualTo(TestConstants.InvalidPasswordCode));
            Assert.That(document.RootElement.GetProperty(TestConstants.JsonMessageProperty).GetString(), Is.EqualTo(CreateResourceLoader().Strings.InvalidPassword));
        }

        [Test]
        public void JsonWriterWhenCommandRequiresAssumeYesInNonInteractiveModeOutputsStructuredWarning()
        {
            string json = CaptureConsoleOutput(() => CreateJsonWriter().Write(new CommandRequiresAssumeYesInNonInteractiveModeWarningOutput(TestConstants.ResetCommand)));

            using JsonDocument document = JsonDocument.Parse(json);

            Assert.Multiple(() =>
            {
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonTypeProperty).GetString(), Is.EqualTo(TestConstants.JsonWarningType));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonCodeProperty).GetString(), Is.EqualTo(TestConstants.CommandRequiresAssumeYesInNonInteractiveModeCode));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonDataProperty).GetProperty(TestConstants.JsonCommandNameProperty).GetString(), Is.EqualTo(TestConstants.ResetCommand));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonMessageProperty).GetString(), Does.Contain(TestConstants.AssumeYesOption));
            });
        }

        [Test]
        public void JsonWriterWhenSecondInitializationOutputsStructuredWarning()
        {
            string json = CaptureConsoleOutput(() => CreateJsonWriter().Write(new SecondInitializationWarningOutput()));

            using JsonDocument document = JsonDocument.Parse(json);

            Assert.That(document.RootElement.GetProperty(TestConstants.JsonTypeProperty).GetString(), Is.EqualTo(TestConstants.JsonWarningType));
            Assert.That(document.RootElement.GetProperty(TestConstants.JsonCodeProperty).GetString(), Is.EqualTo(TestConstants.AlreadyInitializedCode));
            Assert.That(document.RootElement.GetProperty(TestConstants.JsonMessageProperty).GetString(), Is.EqualTo(CreateResourceLoader().Strings.SecondInitialization));
        }

        [Test]
        public void JsonWriterWhenUnsuccessfulAttemptOutputsStructuredWarning()
        {
            string json = CaptureConsoleOutput(() => CreateJsonWriter().Write(new UnsuccessfulAttemptWarningOutput()));

            using JsonDocument document = JsonDocument.Parse(json);

            Assert.That(document.RootElement.GetProperty(TestConstants.JsonTypeProperty).GetString(), Is.EqualTo(TestConstants.JsonWarningType));
            Assert.That(document.RootElement.GetProperty(TestConstants.JsonCodeProperty).GetString(), Is.EqualTo(TestConstants.UnsuccessfulAttemptCode));
            Assert.That(document.RootElement.GetProperty(TestConstants.JsonMessageProperty).GetString(), Is.EqualTo(CreateResourceLoader().Strings.UnsuccessfulAttempt));
        }

        [Test]
        public void JsonWriterWhenUnknownFolderOutputsStructuredWarning()
        {
            string json = CaptureConsoleOutput(() => CreateJsonWriter().Write(new UnknownFolderWarningOutput(TestConstants.AccountAddress, TestConstants.Archive)));

            using JsonDocument document = JsonDocument.Parse(json);

            Assert.Multiple(() =>
            {
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonTypeProperty).GetString(), Is.EqualTo(TestConstants.JsonWarningType));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonCodeProperty).GetString(), Is.EqualTo(TestConstants.UnknownFolderCode));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonDataProperty).GetProperty(TestConstants.JsonAddressProperty).GetString(), Is.EqualTo(TestConstants.AccountAddress));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonDataProperty).GetProperty(TestConstants.JsonFolderProperty).GetString(), Is.EqualTo(TestConstants.Archive));
            });
        }

        [Test]
        public void JsonWriterWhenUnhandledExceptionOutputsStructuredError()
        {
            InvalidOperationException exception = CreateUnhandledException(TestConstants.Boom);
            string json = CaptureConsoleOutput(() => CreateJsonWriter().Write(new UnhandledExceptionOutput(exception)));

            using JsonDocument document = JsonDocument.Parse(json);

            Assert.Multiple(() =>
            {
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonTypeProperty).GetString(), Is.EqualTo(TestConstants.JsonErrorType));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonCodeProperty).GetString(), Is.EqualTo(TestConstants.UnhandledExceptionCode));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonDataProperty).GetProperty(TestConstants.JsonExceptionTypeProperty).GetString(), Is.EqualTo(typeof(InvalidOperationException).FullName));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonMessageProperty).GetString(), Is.EqualTo(CreateResourceLoader().Strings.GetUnhandledException(TestConstants.Boom)));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonMessageProperty).GetString(), Does.Not.Contain(nameof(CreateUnhandledException)));
            });
        }

        [Test]
        public void JsonWriterWhenImpossibleInitializationOutputsStructuredError()
        {
            string json = CaptureConsoleOutput(() => CreateJsonWriter().Write(new ImpossibleInitializationErrorOutput()));

            using JsonDocument document = JsonDocument.Parse(json);

            Assert.That(document.RootElement.GetProperty(TestConstants.JsonTypeProperty).GetString(), Is.EqualTo(TestConstants.JsonErrorType));
            Assert.That(document.RootElement.GetProperty(TestConstants.JsonCodeProperty).GetString(), Is.EqualTo(TestConstants.ImpossibleInitializationCode));
            Assert.That(document.RootElement.GetProperty(TestConstants.JsonMessageProperty).GetString(), Is.EqualTo(CreateResourceLoader().Strings.ImpossibleInitialization));
        }

        [Test]
        public void JsonWriterWhenMessagesOutputWithHeaderWritesMetaHeader()
        {
            Message message = CreateMessage(subject: TestConstants.Subject1);
            string json = CaptureConsoleOutput(() => CreateJsonWriter().Write(new MessagesOutput(TestConstants.InboxHeader, [message], new PagingInfo(1, false))));

            using JsonDocument document = JsonDocument.Parse(json);

            Assert.Multiple(() =>
            {
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonTypeProperty).GetString(), Is.EqualTo(TestConstants.JsonResultType));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonCodeProperty).GetString(), Is.EqualTo(TestConstants.MessagesCode));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonMetaProperty).GetProperty(TestConstants.JsonHeaderProperty).GetString(), Is.EqualTo(TestConstants.InboxHeader));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonMetaProperty).GetProperty(TestConstants.JsonReturnedProperty).GetInt32(), Is.EqualTo(1));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonMetaProperty).GetProperty(TestConstants.JsonHasMoreProperty).GetBoolean(), Is.False);
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonDataProperty)[0].GetProperty(TestConstants.JsonSubjectProperty).GetString(), Is.EqualTo(TestConstants.Subject1));
            });
        }

        [Test]
        public void JsonWriterWhenMessagesOutputWithHasMoreTrueWritesMetaHasMoreTrue()
        {
            Message message = CreateMessage(subject: TestConstants.Subject1);
            string json = CaptureConsoleOutput(() => CreateJsonWriter().Write(new MessagesOutput(TestConstants.InboxHeader, [message], new PagingInfo(1, true))));

            using JsonDocument document = JsonDocument.Parse(json);

            Assert.That(document.RootElement.GetProperty(TestConstants.JsonMetaProperty).GetProperty(TestConstants.JsonHasMoreProperty).GetBoolean(), Is.True);
        }

        [Test]
        public void JsonWriterWhenCompactMessageOutputUsesArraysForToAndFrom()
        {
            Message message = CreateMessage(subject: TestConstants.Subject1);
            string json = CaptureConsoleOutput(() => CreateJsonWriter().Write(new MessagesOutput(TestConstants.InboxHeader, [message], new PagingInfo(1, false))));

            using JsonDocument document = JsonDocument.Parse(json);

            Assert.Multiple(() =>
            {
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonDataProperty)[0].GetProperty(TestConstants.JsonToProperty)[0].GetString(), Is.EqualTo(TestConstants.ToAddress));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonDataProperty)[0].GetProperty(TestConstants.JsonFromProperty)[0].GetString(), Is.EqualTo(TestConstants.FromAddress));
            });
        }

        [Test]
        public void TextWriterWhenAccountAddedOutputsConfirmationText()
        {
            string output = CaptureTextOutput(writer => writer.Write(new AccountAddedOutput(TestConstants.AccountAddress, TestConstants.DecAccountType)));

            Assert.That(output, Does.Contain($"Account {TestConstants.AccountAddress} added ({TestConstants.DecAccountType})."));
        }

        [Test]
        public void TextWriterWhenAccountsOutputPrintsAccountAddresses()
        {
            Account account = new() { Id = 1, Email = new EmailAddress(TestConstants.AccountAddress), Type = MailBoxType.Email };
            string output = CaptureTextOutput(writer => writer.Write(new AccountsOutput([account])));

            Assert.Multiple(() =>
            {
                Assert.That(output, Does.Contain(TestConstants.AccountAddress));
                Assert.That(output, Does.Contain(TestConstants.EmailAccountType));
            });
        }

        [Test]
        public void TextWriterWhenContactsOutputPrintsContactDetails()
        {
            Contact contact = CreateContact(1);
            string output = CaptureTextOutput(writer => writer.Write(new ContactsOutput([contact])));

            Assert.Multiple(() =>
            {
                Assert.That(output, Does.Contain($"{TestConstants.ContactAddressPrefix}{TestConstants.One}{TestConstants.ContactAddressDomain}"));
                Assert.That(output, Does.Contain(TestConstants.Contact1FullName));
            });
        }

        [Test]
        public void TextWriterWhenFoldersOutputPrintsFolderNames()
        {
            Folder folder = CreateFolder(TestConstants.Inbox, FolderAttributes.Inbox, unreadCount: 1, totalCount: 3);
            string output = CaptureTextOutput(writer => writer.Write(new FoldersOutput(TestConstants.AccountAddress, [folder])));

            Assert.Multiple(() =>
            {
                Assert.That(output, Does.Contain($"Folders for account {TestConstants.AccountAddress}:"));
                Assert.That(output, Does.Contain(TestConstants.Inbox));
            });
        }

        [Test]
        public void TextWriterWhenMessageOutputIsNonCompactPrintsFullMessageDetails()
        {
            Message message = CreateMessage(subject: TestConstants.Subject1);
            string output = CaptureTextOutput(writer => writer.Write(new MessageOutput(message, Compact: false)));

            Assert.Multiple(() =>
            {
                Assert.That(output, Does.Contain(TestConstants.Subject1));
                Assert.That(output, Does.Contain(TestConstants.TextBody));
                Assert.That(output, Does.Contain(TestConstants.ToAddress));
                Assert.That(output, Does.Contain(TestConstants.FromAddress));
                Assert.That(output, Does.Contain(TestConstants.Inbox));
            });
        }

        [Test]
        public void TextWriterWhenMessageSentOutputsConfirmationText()
        {
            string output = CaptureConsoleOutput(() => CreateTextWriter().Write(new MessageSentOutput(TestConstants.ReplyTestSubject, TestConstants.ToAddress, TestConstants.FromAddress)));

            Assert.That(output, Does.Contain($"Message sent from {TestConstants.FromAddress} to {TestConstants.ToAddress}. Subject: {TestConstants.ReplyTestSubject}"));
        }

        [Test]
        public void TextWriterWhenFolderSyncedOutputsConfirmationText()
        {
            string output = CaptureConsoleOutput(() => CreateTextWriter().Write(new FolderSyncedOutput(TestConstants.AccountAddress, TestConstants.Inbox)));

            Assert.That(output, Does.Contain($"Folder '{TestConstants.Inbox}' for account {TestConstants.AccountAddress} synchronized."));
        }

        [Test]
        public void TextWriterWhenMessageDeletedOutputsConfirmationText()
        {
            string output = CaptureConsoleOutput(() => CreateTextWriter().Write(new MessageDeletedOutput(TestConstants.AccountAddress, TestConstants.Inbox, 42, 7)));

            Assert.That(output, Does.Contain($"Message 42 (PK: 7) deleted from folder '{TestConstants.Inbox}' for account {TestConstants.AccountAddress}."));
        }

        [Test]
        public void TextWriterWhenApplicationOpenedOutputsStatusText()
        {
            string output = CaptureConsoleOutput(() => CreateTextWriter().Write(new ApplicationOpenedOutput()));

            Assert.That(output, Does.Contain(CreateResourceLoader().Strings.AppOpened));
        }

        [Test]
        public void TextWriterWhenApplicationResetOutputsStatusText()
        {
            string output = CaptureConsoleOutput(() => CreateTextWriter().Write(new ApplicationResetOutput()));

            Assert.That(output, Does.Contain(CreateResourceLoader().Strings.AppReset));
        }

        [Test]
        public void TextWriterWhenApplicationRestoredOutputsStatusText()
        {
            string output = CaptureConsoleOutput(() => CreateTextWriter().Write(new ApplicationRestoredOutput()));

            Assert.That(output, Does.Contain(CreateResourceLoader().Strings.AppRestored));
        }

        [Test]
        public void TextWriterWhenAuthorizationCanceledOutputsStatusText()
        {
            string output = CaptureConsoleOutput(() => CreateTextWriter().Write(new AuthorizationCanceledOutput()));

            Assert.That(output, Does.Contain(CreateResourceLoader().Strings.AuthorizationCanceled));
        }

        [Test]
        public void TextWriterWhenAuthorizationToServiceOutputsStatusText()
        {
            string output = CaptureConsoleOutput(() => CreateTextWriter().Write(new AuthorizationToServiceOutput(TestConstants.ProtonServiceName)));

            Assert.That(output, Does.Contain(CreateResourceLoader().Strings.GetAuthorizationToServiceText(TestConstants.ProtonServiceName)));
        }

        [Test]
        public void TextWriterWhenAuthorizationCompletedOutputsStatusText()
        {
            string output = CaptureConsoleOutput(() => CreateTextWriter().Write(new AuthorizationCompletedOutput()));

            Assert.That(output, Does.Contain(CreateResourceLoader().Strings.AuthorizationCompleted));
        }

        [Test]
        public void TextWriterWhenInvalidPasswordOutputsWarningText()
        {
            string output = CaptureConsoleOutput(() => CreateTextWriter().Write(new InvalidPasswordWarningOutput()));

            Assert.That(output, Does.Contain(CreateResourceLoader().Strings.InvalidPassword));
        }

        [Test]
        public void TextWriterWhenSecondInitializationOutputsWarningText()
        {
            string output = CaptureConsoleOutput(() => CreateTextWriter().Write(new SecondInitializationWarningOutput()));

            Assert.That(output, Does.Contain(CreateResourceLoader().Strings.SecondInitialization));
        }

        [Test]
        public void TextWriterWhenUnsuccessfulAttemptOutputsWarningText()
        {
            string output = CaptureConsoleOutput(() => CreateTextWriter().Write(new UnsuccessfulAttemptWarningOutput()));

            Assert.That(output, Does.Contain(CreateResourceLoader().Strings.UnsuccessfulAttempt));
        }

        [Test]
        public void TextWriterWhenUnknownFolderOutputsWarningText()
        {
            string output = CaptureConsoleOutput(() => CreateTextWriter().Write(new UnknownFolderWarningOutput(TestConstants.AccountAddress, TestConstants.Archive)));

            Assert.That(output, Does.Contain(CreateResourceLoader().Strings.GetUnknownFolderWarning(TestConstants.AccountAddress, TestConstants.Archive)));
        }

        [Test]
        public void TextWriterWhenStartupCommandRequiresUnlockPasswordFromStandardInputOutputsWarningText()
        {
            string output = CaptureConsoleOutput(() => CreateTextWriter().Write(new StartupCommandRequiresUnlockPasswordFromStandardInputWarningOutput(TestConstants.ListAccountsCommand)));

            Assert.Multiple(() =>
            {
                Assert.That(output, Does.Contain(TestConstants.ListAccountsCommand));
                Assert.That(output, Does.Contain(TestConstants.UnlockPasswordStdinOption));
            });
        }

        [Test]
        public void TextWriterWhenNonInteractiveOperationIsNotSupportedOutputsErrorText()
        {
            string output = CaptureConsoleOutput(() => CreateTextWriter().Write(new NonInteractiveOperationNotSupportedErrorOutput(TestConstants.InteractiveMenu)));

            Assert.Multiple(() =>
            {
                Assert.That(output, Does.Contain(TestConstants.InteractiveMenu));
                Assert.That(output, Does.Contain(TestConstants.NonInteractiveOption));
            });
        }

        [Test]
        public void TextWriterWhenUnhandledExceptionOutputsErrorText()
        {
            InvalidOperationException exception = new(TestConstants.Boom);
            string output = CaptureConsoleOutput(() => CreateTextWriter().Write(new UnhandledExceptionOutput(exception)));

            Assert.That(output, Does.Contain(TestConstants.Boom));
        }

        [Test]
        public void TextWriterWhenImpossibleInitializationOutputsErrorText()
        {
            string output = CaptureConsoleOutput(() => CreateTextWriter().Write(new ImpossibleInitializationErrorOutput()));

            Assert.That(output, Does.Contain(CreateResourceLoader().Strings.ImpossibleInitialization));
        }

        [Test]
        public void TextWriterWhenMessagesOutputWithHeaderPrintsHeaderBeforeMessages()
        {
            Message message = CreateMessage(subject: TestConstants.Subject1);
            string output = CaptureConsoleOutput(() => CreateTextWriter().Write(new MessagesOutput(TestConstants.InboxHeader, [message], Paging: null)));

            Assert.Multiple(() =>
            {
                Assert.That(output, Does.Contain(TestConstants.InboxHeader));
                Assert.That(output, Does.Contain(TestConstants.Subject1));
                Assert.That(output.IndexOf(TestConstants.InboxHeader, StringComparison.Ordinal), Is.LessThan(output.IndexOf(TestConstants.Subject1, StringComparison.Ordinal)));
            });
        }

        private JsonApplicationOutputWriter CreateJsonWriter()
        {
            return new JsonApplicationOutputWriter(CreateResourceLoader());
        }

        private TextApplicationOutputWriter CreateTextWriter()
        {
            return new TextApplicationOutputWriter(CreateResourceLoader());
        }

        private ResourceLoader CreateResourceLoader()
        {
            IStringLocalizer<Resources.Program> localizer = _serviceProvider.GetRequiredService<IStringLocalizer<Resources.Program>>();
            return new ResourceLoader(localizer);
        }

        private JsonDocument CaptureJsonDocument(Action<JsonApplicationOutputWriter> writeAction)
        {
            ArgumentNullException.ThrowIfNull(writeAction);

            string json = CaptureConsoleOutput(() => writeAction(CreateJsonWriter()));
            return JsonDocument.Parse(json);
        }

        private string CaptureTextOutput(Action<TextApplicationOutputWriter> writeAction)
        {
            ArgumentNullException.ThrowIfNull(writeAction);

            return CaptureConsoleOutput(() => writeAction(CreateTextWriter()));
        }

        private static void AssertJsonTypeAndCode(JsonDocument document, string expectedType, string expectedCode)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentException.ThrowIfNullOrWhiteSpace(expectedType);
            ArgumentException.ThrowIfNullOrWhiteSpace(expectedCode);

            Assert.Multiple(() =>
            {
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonTypeProperty).GetString(), Is.EqualTo(expectedType));
                Assert.That(document.RootElement.GetProperty(TestConstants.JsonCodeProperty).GetString(), Is.EqualTo(expectedCode));
            });
        }

        private static JsonElement GetData(JsonDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);

            return document.RootElement.GetProperty(TestConstants.JsonDataProperty);
        }

        private static JsonElement GetMeta(JsonDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);

            return document.RootElement.GetProperty(TestConstants.JsonMetaProperty);
        }

        private static JsonElement GetFirstDataItem(JsonDocument document)
        {
            return GetData(document)[0];
        }

        private static string CaptureConsoleOutput(Action action)
        {
            return TestConsole.CaptureOutput(action);
        }

        private static InvalidOperationException CreateUnhandledException(string message)
        {
            try
            {
                throw new InvalidOperationException(message);
            }
            catch (InvalidOperationException exception)
            {
                return exception;
            }
        }

        private static Message CreateMessage(string subject)
        {
            return TestDataFactory.CreateMessage(subject: subject);
        }

        private static Contact CreateContact(int id)
        {
            return TestDataFactory.CreateContact(id);
        }

        private static Folder CreateFolder(string fullName, FolderAttributes attributes, int unreadCount, int totalCount)
        {
            return TestDataFactory.CreateFolder(fullName, attributes, unreadCount, totalCount);
        }
    }
}
