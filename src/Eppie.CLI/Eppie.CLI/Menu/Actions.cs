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

using Eppie.CLI.Common;
using Eppie.CLI.Exceptions;
using Eppie.CLI.Services;
using Eppie.CLI.Tools;

using Finebits.Authorization.OAuth2.Abstractions;
using Finebits.Authorization.OAuth2.Types;

using Microsoft.Extensions.Logging;

using Tuvi.Core;
using Tuvi.Core.Entities;

namespace Eppie.CLI.Menu
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal class Actions(
        ILogger<Actions> logger,
        Application application,
        ApplicationLaunchOptions launchOptions,
        IApplicationOutputWriter outputWriter,
        IApplicationFailureHandler failureHandler,
        IApplicationOutputCoordinator outputCoordinator,
        IEmailAccountInputResolver emailAccountInputResolver,
        IProtonAccountInputResolver protonAccountInputResolver,
        AuthorizationProvider authProvider,
        CoreProvider coreProvider)
    {
        private readonly ILogger<Actions> _logger = logger;
        private readonly Application _application = application;
        private readonly ApplicationLaunchOptions _launchOptions = launchOptions;
        private readonly IApplicationOutputWriter _outputWriter = outputWriter;
        private readonly IApplicationFailureHandler _failureHandler = failureHandler;
        private readonly IApplicationOutputCoordinator _outputCoordinator = outputCoordinator;
        private readonly IEmailAccountInputResolver _emailAccountInputResolver = emailAccountInputResolver;
        private readonly IProtonAccountInputResolver _protonAccountInputResolver = protonAccountInputResolver;
        private readonly CoreProvider _coreProvider = coreProvider;
        private readonly AuthorizationProvider _authProvider = authProvider;

        internal void ExitAction()
        {
            UnsubscribeFromCoreExceptions();
            _logger.LogMethodCall();
            _application.StopApplication();
        }

        internal async Task InitActionAsync()
        {
            _logger.LogMethodCall();

            bool isFirstTime = await _coreProvider.TuviMailCore.IsFirstApplicationStartAsync().ConfigureAwait(false);

            if (!isFirstTime)
            {
                ThrowSecondInitializationWarning();
                return;
            }

            ISecurityManager sm = _coreProvider.TuviMailCore.GetSecurityManager();
            string[] seedPhrase = await sm.CreateSeedPhraseAsync().ConfigureAwait(false);
            string password = _application.AskNewPassword();

            if (password.Length == 0 || (!_launchOptions.NonInteractive && password != _application.ConfirmPassword()))
            {
                ThrowInvalidPasswordWarning();
                return;
            }

            bool success = await _coreProvider.TuviMailCore.InitializeApplicationAsync(password).ConfigureAwait(false);

            if (success)
            {
                SubscribeToCoreExceptions();
                WriteApplicationInitializationMessage(seedPhrase);
            }
            else
            {
                ThrowImpossibleInitializationError();
            }
        }

        internal async Task ResetActionAsync()
        {
            _logger.LogMethodCall();

            if (ConfirmResetOrWarn(MenuCommand.Reset))
            {
                await _coreProvider.ResetAsync().ConfigureAwait(false);
                WriteApplicationResetMessage();
            }
        }

        internal async Task OpenActionAsync()
        {
            _logger.LogMethodCall();

            bool isFirstTime = await _coreProvider.TuviMailCore.IsFirstApplicationStartAsync().ConfigureAwait(false);

            if (isFirstTime)
            {
                ThrowUninitializedAppWarning();
                return;
            }

            bool success = await _coreProvider.TuviMailCore.InitializeApplicationAsync(_application.AskPassword()).ConfigureAwait(false);

            if (success)
            {
                SubscribeToCoreExceptions();
                WriteApplicationOpenedMessage();
            }
            else
            {
                ThrowInvalidPasswordWarning();
            }
        }

        internal async Task ListAccountsActionAsync()
        {
            _logger.LogMethodCall();

            List<Account> accounts = await _coreProvider.TuviMailCore.GetAccountsAsync().ConfigureAwait(true);
            accounts.Sort((a, b) => a.Id.CompareTo(b.Id));

            _outputWriter.Write(new AccountsOutput(accounts));
        }

        internal async Task ListFoldersActionAsync(string accountAddress)
        {
            _logger.LogMethodCall();

            EmailAddress email = new(accountAddress);
            Account account = await _coreProvider.TuviMailCore.GetAccountAsync(email).ConfigureAwait(false);

            List<Folder> folders = [.. account.FoldersStructure.OrderBy(static folder => folder.FullName, StringComparer.OrdinalIgnoreCase)];
            _outputWriter.Write(new FoldersOutput(accountAddress, folders));
        }

        internal Task AddAccountActionAsync(MenuCommand.CommandAddAccountOptions.Options options)
        {
            _logger.LogMethodCall();

            return options.Type switch
            {
                MenuCommand.CommandAddAccountOptions.AccountType.Email => AddEmailAccountAsync(options),
                MenuCommand.CommandAddAccountOptions.AccountType.Dec => AddDecAccountAsync(),
                MenuCommand.CommandAddAccountOptions.AccountType.Proton => AddProtonAccountAsync(options),
                _ => throw new ArgumentException($"Account type '{options.Type}' is not supported.", nameof(options)),
            };
        }

        internal async Task RestoreActionAsync()
        {
            _logger.LogMethodCall();

            bool isFirstTime = await _coreProvider.TuviMailCore.IsFirstApplicationStartAsync().ConfigureAwait(false);

            if (!isFirstTime)
            {
                if (!ConfirmResetOrWarn(MenuCommand.Restore))
                {
                    return;
                }

                await _coreProvider.ResetAsync().ConfigureAwait(false);
            }

            string[] seedPhrase = _application.AskSeedPhrase().Split([' ', ';']);
            string password = _application.AskNewPassword();
            if (password.Length == 0 || (!_launchOptions.NonInteractive && password != _application.ConfirmPassword()))
            {
                ThrowInvalidPasswordWarning();
                return;
            }

            await _coreProvider.TuviMailCore.GetSecurityManager().RestoreSeedPhraseAsync(seedPhrase).ConfigureAwait(false);
            bool success = await _coreProvider.TuviMailCore.InitializeApplicationAsync(password).ConfigureAwait(false);

            if (success)
            {
                //ToDo: temporary solution
                Uri restoreUri = new(_application.AskRestorePath());

                await _coreProvider.TuviMailCore.RestoreFromBackupIfNeededAsync(restoreUri).ConfigureAwait(false);
                SubscribeToCoreExceptions();
                WriteSuccessfulRestoredMessage();
            }
            else
            {
                ThrowImpossibleInitializationError();
            }
        }

        internal async Task SendActionAsync(string sender, string receiver, string subject)
        {
            _logger.LogMethodCall();

            EmailAddress senderAddress = new(sender);
            EmailAddress receiverAddress = new(receiver);

            IAccountService accountService = await _coreProvider.TuviMailCore.GetAccountServiceAsync(senderAddress).ConfigureAwait(false);

            string body = _application.AskMessageBody();

            Message message = new()
            {
                Subject = subject,
                TextBody = body,
            };
            message.From.Add(senderAddress);
            message.To.Add(receiverAddress);

            await accountService.SendMessageAsync(message, false, false).ConfigureAwait(false);
            _outputWriter.Write(new MessageSentOutput(message.Subject ?? string.Empty, receiverAddress.Address, senderAddress.Address));
        }

        internal async Task ListContactsActionAsync(ApplicationListingOptions options)
        {
            _logger.LogMethodCall();

            List<Contact> contacts = [.. await _coreProvider.TuviMailCore.GetContactsAsync().ConfigureAwait(true)];
            _outputCoordinator.WriteContacts(options, contacts, _application.ConfirmAskMoreContacts);
        }

        internal async Task ShowMessageActionAsync(string address, string folderName, uint id, int pk)
        {
            _logger.LogMethodCall();

            MessageCommandContext? context = await TryResolveMessageCommandContextAsync(address, folderName, id, pk).ConfigureAwait(false);
            if (context is null)
            {
                return;
            }

            Message message = await context.Value.AccountService.GetMessageBodyAsync(context.Value.Message).ConfigureAwait(false);
            _outputWriter.Write(new MessageOutput(message, Compact: false));
        }

        internal async Task DeleteMessageActionAsync(string address, string folderName, uint id, int pk)
        {
            _logger.LogMethodCall();

            MessageCommandContext? context = await TryResolveMessageCommandContextAsync(address, folderName, id, pk).ConfigureAwait(false);
            if (context is null)
            {
                return;
            }

            await context.Value.AccountService.DeleteMessageAsync(context.Value.Message).ConfigureAwait(false);
            _outputWriter.Write(new MessageDeletedOutput(address, context.Value.Folder.FullName, id, pk));
        }

        internal async Task SyncFolderActionAsync(string accountAddress, string folderName)
        {
            _logger.LogMethodCall();

            EmailAddress email = new(accountAddress);
            Account account = await _coreProvider.TuviMailCore.GetAccountAsync(email).ConfigureAwait(false);

            // TODO: Need to simplify the retrieval of CompositeFolder.
            IReadOnlyList<CompositeAccount> accounts = await _coreProvider.TuviMailCore.GetCompositeAccountsAsync().ConfigureAwait(false);
            CompositeAccount compositeAccount = accounts.First(a => a.Email == account.Email);
            CompositeFolder? folder = compositeAccount.FoldersStructure.FirstOrDefault(x => x.HasSameName(folderName));

            if (folder is null)
            {
                ThrowUnknownFolderWarning(accountAddress, folderName);
                return;
            }

            await _coreProvider.TuviMailCore.CheckForNewMessagesInFolderAsync(folder, CancellationToken.None).ConfigureAwait(false);
            _outputWriter.Write(new FolderSyncedOutput(accountAddress, folderName));
        }

        internal async Task ShowAllMessagesActionAsync(ApplicationListingOptions options)
        {
            _logger.LogMethodCall();

            await _outputCoordinator.WriteMessagesAsync(_application.GetPrintAllMessagesHeader(), options, (count, lastMsg) => GetMessages(count, lastMsg, _coreProvider.TuviMailCore), _application.ConfirmAskMoreMessages).ConfigureAwait(false);

            static async Task<IEnumerable<Message>> GetMessages(int count, Message lastMessage, ITuviMail tuviMail)
            {
                return await tuviMail.GetAllEarlierMessagesAsync(count, lastMessage).ConfigureAwait(false);
            }
        }

        internal async Task ShowFolderMessagesActionAsync(string accountAddress, string folderName, ApplicationListingOptions options)
        {
            _logger.LogMethodCall();

            EmailAddress email = new(accountAddress);
            Account account = await _coreProvider.TuviMailCore.GetAccountAsync(email).ConfigureAwait(false);
            Folder? folder = account.FoldersStructure.Find(x => x.HasSameName(folderName));

            if (folder is null)
            {
                ThrowUnknownFolderWarning(accountAddress, folderName);
                return;
            }

            await _outputCoordinator.WriteMessagesAsync(_application.GetPrintFolderMessagesHeader(accountAddress, folderName), options, (count, lastMsg) => GetMessages(count, lastMsg, folder, _coreProvider.TuviMailCore), _application.ConfirmAskMoreMessages).ConfigureAwait(false);

            static async Task<IEnumerable<Message>> GetMessages(int count, Message lastMessage, Folder folder, ITuviMail tuviMail)
            {
                return await tuviMail.GetFolderEarlierMessagesAsync(folder, count, lastMessage).ConfigureAwait(false);
            }
        }

        internal async Task ShowContactMessagesActionAsync(string contactAddress, ApplicationListingOptions options)
        {
            _logger.LogMethodCall();

            EmailAddress contact = new(contactAddress);

            await _outputCoordinator.WriteMessagesAsync(_application.GetPrintContactMessagesHeader(contactAddress), options, (count, lastMsg) => GetMessages(count, lastMsg, contact, _coreProvider.TuviMailCore), _application.ConfirmAskMoreMessages).ConfigureAwait(false);

            static async Task<IEnumerable<Message>> GetMessages(int count, Message lastMessage, EmailAddress email, ITuviMail tuviMail)
            {
                return await tuviMail.GetContactEarlierMessagesAsync(email, count, lastMessage).ConfigureAwait(false);
            }
        }

        internal async Task ImportKeyBundleFromFileAsync(FileInfo file)
        {
            _logger.LogMethodCall();
            await Task.Run(() => ImportBundle(file.FullName)).ConfigureAwait(false);
        }

        private void ImportBundle(string fileAddress)
        {
            using MemoryStream keyIn = new(File.ReadAllBytes(fileAddress));
            _coreProvider.TuviMailCore.GetSecurityManager().ImportPgpKeyRingBundle(keyIn);
        }

        private async Task<MessageCommandContext?> TryResolveMessageCommandContextAsync(string address, string folderName, uint id, int pk)
        {
            EmailAddress accountEmail = new(address);
            Account account = await _coreProvider.TuviMailCore.GetAccountAsync(accountEmail).ConfigureAwait(false);
            Folder? folder = account.FoldersStructure.FirstOrDefault(x => x.HasSameName(folderName));

            if (folder is null)
            {
                ThrowUnknownFolderWarning(address, folderName);
                return null;
            }

            IAccountService accountService = await _coreProvider.TuviMailCore.GetAccountServiceAsync(accountEmail).ConfigureAwait(false);
            return new MessageCommandContext(accountService, folder, CreateMessageReference(folder, id, pk));
        }

        private static Message CreateMessageReference(Folder folder, uint id, int pk)
        {
            ArgumentNullException.ThrowIfNull(folder);

            return new Message()
            {
                Pk = pk,
                Id = id,
                Folder = folder,
                FolderId = folder.Id
            };
        }

        private readonly record struct MessageCommandContext(IAccountService AccountService, Folder Folder, Message Message);

        private async Task AddEmailAccountAsync(MenuCommand.CommandAddAccountOptions.Options options)
        {
            _logger.LogMethodCall();

            async Task<Account> CreateAccountAsync()
            {
                if (options.InputJsonFromStandardInput)
                {
                    return await _emailAccountInputResolver.ResolveAsync().ConfigureAwait(false);
                }

                MailServer mailServer = _application.SelectOption(MailServer.Other, true);

                return mailServer == MailServer.Other ? CreateDefaultAccount()
                                                      : await CreateOAuth2AccountAsync(mailServer).ConfigureAwait(false);
            }

            try
            {
                Account account = await CreateAccountAsync().ConfigureAwait(false);

                ICredentialsProvider outgoingCredentialsProvider = _coreProvider.TuviMailCore.CredentialsManager.CreateOutgoingCredentialsProvider(account);
                await _coreProvider.TuviMailCore.TestMailServerAsync(
                    account.OutgoingServerAddress,
                    account.OutgoingServerPort,
                    account.OutgoingMailProtocol,
                    outgoingCredentialsProvider
                    ).ConfigureAwait(false);

                ICredentialsProvider incomingCredentialsProvider = _coreProvider.TuviMailCore.CredentialsManager.CreateIncomingCredentialsProvider(account);
                await _coreProvider.TuviMailCore.TestMailServerAsync(
                    account.IncomingServerAddress,
                    account.IncomingServerPort,
                    account.IncomingMailProtocol,
                    incomingCredentialsProvider).ConfigureAwait(false);

                await _coreProvider.TuviMailCore.AddAccountAsync(account).ConfigureAwait(false);

                _outputWriter.Write(new AccountAddedOutput(account.Email.Address, account.Type.ToString()));
            }
            catch (OperationCanceledException)
            {
                WriteAuthorizationCanceledMessage();
            }
        }

        private Account CreateDefaultAccount()
        {
            Account account = Account.Default;
            account.Email = new EmailAddress(_application.AskAccountAddress());

            BasicAuthData basicData = new()
            {
                Password = _application.AskAccountPassword()
            };

            account.AuthData = basicData;

            account.OutgoingServerAddress = _application.AskSMTPServer(MailServer.Other);
            account.OutgoingServerPort = _application.AskSMTPServerPort(MailServer.Other);
            account.IncomingServerAddress = _application.AskIMAPServer(MailServer.Other);
            account.IncomingServerPort = _application.AskIMAPServerPort(MailServer.Other);

            return account;
        }

        private async Task<Account> CreateOAuth2AccountAsync(MailServer mailServer)
        {
            using CancellationTokenSource cancellationLogin = new();

            void CancelLogin(object? sender, ConsoleCancelEventArgs e)
            {
                cancellationLogin.Cancel();
            }

            try
            {
                Console.CancelKeyPress += CancelLogin;

                IAuthorizationClient authClient = _authProvider.CreateAuthorizationClient(mailServer);

                WriteAuthorizationToServiceMessage(mailServer.ToString());
                AuthCredential authCredential = await authClient.LoginAsync(cancellationLogin.Token).ConfigureAwait(false);

                string? email = await ReadEmailAddressAsync(authClient, authCredential).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(email))
                {
                    WriteAuthorizationCompletedMessage();

                    Account account = Account.Default;
                    account.Email = new EmailAddress(email);

                    OAuth2Data oauthData = new()
                    {
                        RefreshToken = authCredential.RefreshToken,
                        AuthAssistantId = mailServer.ToString()
                    };
                    account.AuthData = oauthData;

                    account.OutgoingServerAddress = _application.AskSMTPServer(mailServer);
                    account.OutgoingServerPort = _application.AskSMTPServerPort(mailServer);
                    account.IncomingServerAddress = _application.AskIMAPServer(mailServer);
                    account.IncomingServerPort = _application.AskIMAPServerPort(mailServer);

                    return account;
                }
                else
                {
                    throw new InvalidOperationException("User data cannot be read");
                }
            }
            finally
            {
                Console.CancelKeyPress -= CancelLogin;
            }
        }

        private async Task AddProtonAccountAsync(MenuCommand.CommandAddAccountOptions.Options options)
        {
            _logger.LogMethodCall();

            IProtonAccountInput input = await _protonAccountInputResolver.ResolveAsync(options.InputJsonFromStandardInput).ConfigureAwait(false);

            (string userId, string refreshToken, string saltedKeyPass) = await Tuvi.Proton.ClientAuth.LoginFullAsync(
                input.Email,
                input.AccountPassword,
                (ex, ct) => Task.FromResult((true, input.GetTwoFactorCode(ex is null))),
                (ex, ct) => Task.FromResult((true, input.GetMailboxPassword(ex is null))),
                null, // human verification does not supported in CLI
                default).ConfigureAwait(false);

            Account account = Account.Default;

            account.Email = new EmailAddress(input.Email);
            account.Type = MailBoxType.Proton;
            account.AuthData = new ProtonAuthData()
            {
                UserId = userId,
                RefreshToken = refreshToken,
                SaltedPassword = saltedKeyPass
            };

            await _coreProvider.TuviMailCore.AddAccountAsync(account).ConfigureAwait(false);
            _outputWriter.Write(new AccountAddedOutput(account.Email.Address, account.Type.ToString()));
        }

        private async Task AddDecAccountAsync()
        {
            _logger.LogMethodCall();

            const NetworkType NetworkType = NetworkType.Eppie;
            (string emailName, int index) = await _coreProvider.TuviMailCore.GetSecurityManager().GetNextDecAccountPublicKeyAsync(NetworkType, default).ConfigureAwait(false);

            EmailAddress email = EmailAddress.CreateDecentralizedAddress(NetworkType, emailName);

            Account account = new()
            {
                Email = email,
                IsBackupAccountSettingsEnabled = true,
                IsBackupAccountMessagesEnabled = true,
                Type = MailBoxType.Dec,
                DecentralizedAccountIndex = index
            };

            await _coreProvider.TuviMailCore.AddAccountAsync(account).ConfigureAwait(false);
            _outputWriter.Write(new AccountAddedOutput(account.Email.Address, account.Type.ToString()));
        }

        private void OnCoreException(object? sender, ExceptionEventArgs e)
        {
            _logger.LogMethodCall();

            ArgumentNullException.ThrowIfNull(e);

            _failureHandler.HandleUnhandledException(e.Exception);
        }

        private void WriteApplicationInitializationMessage(IReadOnlyCollection<string> seedPhrase)
        {
            _logger.LogDebug("The application has been initialized.");
            _outputWriter.Write(new ApplicationInitializedOutput(seedPhrase));
        }

        private void WriteApplicationResetMessage()
        {
            _logger.LogDebug("The application has been reset.");
            _outputWriter.Write(new ApplicationResetOutput());
        }

        private void WriteApplicationOpenedMessage()
        {
            _logger.LogDebug("The application was opened.");
            _outputWriter.Write(new ApplicationOpenedOutput());
        }

        private void WriteSuccessfulRestoredMessage()
        {
            _logger.LogDebug("Eppie account was restored.");
            _outputWriter.Write(new ApplicationRestoredOutput());
        }

        private void WriteAuthorizationCanceledMessage()
        {
            _logger.LogDebug("Authorization operation was canceled.");
            _outputWriter.Write(new AuthorizationCanceledOutput());
        }

        private void WriteAuthorizationToServiceMessage(string serviceName)
        {
            _logger.LogDebug("Authorization to {ServiceName} service starting.", serviceName);
            _outputWriter.Write(new AuthorizationToServiceOutput(serviceName));
        }

        private void WriteAuthorizationCompletedMessage()
        {
            _logger.LogDebug("Authorization completed successfully.");
            _outputWriter.Write(new AuthorizationCompletedOutput());
        }

        private static void ThrowInvalidPasswordWarning()
        {
            throw new ApplicationCommandException(new InvalidPasswordWarningOutput(), exitCode: 0);
        }

        private static void ThrowSecondInitializationWarning()
        {
            throw new ApplicationCommandException(new SecondInitializationWarningOutput(), exitCode: 0);
        }

        private static void ThrowUninitializedAppWarning()
        {
            throw new ApplicationCommandException(new UninitializedAppWarningOutput(), exitCode: 0);
        }

        private static void ThrowImpossibleInitializationError()
        {
            throw new ApplicationCommandException(new ImpossibleInitializationErrorOutput(), exitCode: 0);
        }

        private static void ThrowUnknownFolderWarning(string address, string folder)
        {
            throw new ApplicationCommandException(new UnknownFolderWarningOutput(address, folder), exitCode: 0);
        }

        private void SubscribeToCoreExceptions()
        {
            _coreProvider.TuviMailCore.ExceptionOccurred -= OnCoreException;
            _coreProvider.TuviMailCore.ExceptionOccurred += OnCoreException;
        }

        private void UnsubscribeFromCoreExceptions()
        {
            _coreProvider.TuviMailCore.ExceptionOccurred -= OnCoreException;
        }

        private bool ConfirmResetOrWarn(string commandName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(commandName);

            return _launchOptions.NonInteractive && !_launchOptions.AssumeYes
                ? throw new ApplicationCommandException(new CommandRequiresAssumeYesInNonInteractiveModeWarningOutput(commandName), exitCode: 0)
                : _application.ConfirmReset();
        }

        private static async Task<string?> ReadEmailAddressAsync(IAuthorizationClient client, AuthCredential credential)
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(credential);

            if (client is IProfileReader profileReader)
            {
                IUserProfile profile = await profileReader.ReadProfileAsync(credential).ConfigureAwait(false);
                return profile.Email;
            }

            return credential.ReadEmailAddress();
        }
    }
}
