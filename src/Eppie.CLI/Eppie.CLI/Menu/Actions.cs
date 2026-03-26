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
        IApplicationOutputCoordinator outputCoordinator,
        AuthorizationProvider authProvider,
        CoreProvider coreProvider)
    {
        private readonly ILogger<Actions> _logger = logger;
        private readonly Application _application = application;
        private readonly ApplicationLaunchOptions _launchOptions = launchOptions;
        private readonly IApplicationOutputWriter _outputWriter = outputWriter;
        private readonly IApplicationOutputCoordinator _outputCoordinator = outputCoordinator;
        private readonly CoreProvider _coreProvider = coreProvider;
        private readonly AuthorizationProvider _authProvider = authProvider;

        internal void ExitAction()
        {
            _coreProvider.TuviMailCore.ExceptionOccurred -= OnCoreException;
            _logger.LogMethodCall();
            _application.StopApplication();
        }

        internal async Task InitActionAsync()
        {
            _logger.LogMethodCall();

            bool isFirstTime = await _coreProvider.TuviMailCore.IsFirstApplicationStartAsync().ConfigureAwait(false);

            if (!isFirstTime)
            {
                WriteSecondInitializationWarning();
                return;
            }

            ISecurityManager sm = _coreProvider.TuviMailCore.GetSecurityManager();
            string[] seedPhrase = await sm.CreateSeedPhraseAsync().ConfigureAwait(false);
            string password = _application.AskNewPassword();

            if (password.Length == 0 || (!_launchOptions.NonInteractive && password != _application.ConfirmPassword()))
            {
                WriteInvalidPasswordWarning();
                return;
            }

            bool success = await _coreProvider.TuviMailCore.InitializeApplicationAsync(password).ConfigureAwait(false);

            _coreProvider.TuviMailCore.ExceptionOccurred += OnCoreException;

            if (success)
            {
                WriteApplicationInitializationMessage(seedPhrase);
            }
            else
            {
                WriteImpossibleInitializationError();
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
                WriteUninitializedAppWarning();
                return;
            }

            bool success = await _coreProvider.TuviMailCore.InitializeApplicationAsync(_application.AskPassword()).ConfigureAwait(false);

            if (success)
            {
                WriteApplicationOpenedMessage();
            }
            else
            {
                WriteInvalidPasswordWarning();
            }
        }

        internal async Task ListAccountsActionAsync()
        {
            _logger.LogMethodCall();

            List<Account> accounts = await _coreProvider.TuviMailCore.GetAccountsAsync().ConfigureAwait(true);
            accounts.Sort((a, b) => a.Id.CompareTo(b.Id));

            _outputWriter.Write(new AccountsOutput(accounts));
        }

        internal Task AddAccountActionAsync(MenuCommand.CommandAddAccountOptions.AccountType type)
        {
            _logger.LogMethodCall();

            return type switch
            {
                MenuCommand.CommandAddAccountOptions.AccountType.Email => AddEmailAccountAsync(),
                MenuCommand.CommandAddAccountOptions.AccountType.Dec => AddDecAccountAsync(),
                MenuCommand.CommandAddAccountOptions.AccountType.Proton => AddProtonAccountAsync(),
                _ => throw new ArgumentException($"Account type '{type}' is not supported.", nameof(type)),
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
                WriteInvalidPasswordWarning();
                return;
            }

            await _coreProvider.TuviMailCore.GetSecurityManager().RestoreSeedPhraseAsync(seedPhrase).ConfigureAwait(false);
            bool success = await _coreProvider.TuviMailCore.InitializeApplicationAsync(password).ConfigureAwait(false);

            if (success)
            {
                //ToDo: temporary solution
                Uri restoreUri = new(_application.AskRestorePath());

                await _coreProvider.TuviMailCore.RestoreFromBackupIfNeededAsync(restoreUri).ConfigureAwait(false);
                WriteSuccessfulRestoredMessage();
            }
            else
            {
                WriteImpossibleInitializationError();
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

        internal async Task ListContactsActionAsync(int pageSize)
        {
            _logger.LogMethodCall();

            List<Contact> contacts = [.. await _coreProvider.TuviMailCore.GetContactsAsync().ConfigureAwait(true)];
            _outputCoordinator.WriteContacts(pageSize, contacts, _application.ConfirmAskMoreContacts);
        }

        internal async Task ShowMessageActionAsync(string address, string folderName, uint id, int pk)
        {
            _logger.LogMethodCall();

            EmailAddress accountEmail = new(address);
            Account account = await _coreProvider.TuviMailCore.GetAccountAsync(accountEmail).ConfigureAwait(true);
            Folder? folder = account.FoldersStructure.FirstOrDefault(x => x.HasSameName(folderName));

            if (folder is null)
            {
                WriteUnknownFolderWarning(address, folderName);
                return;
            }

            Message msg = new()
            {
                Pk = pk,
                Id = id,
                Folder = folder,
                FolderId = folder.Id
            };

            IAccountService accountService = await _coreProvider.TuviMailCore.GetAccountServiceAsync(accountEmail).ConfigureAwait(true);
            Message message = await accountService.GetMessageBodyAsync(msg).ConfigureAwait(true);
            _outputWriter.Write(new MessageOutput(message, Compact: false));
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
                WriteUnknownFolderWarning(accountAddress, folderName);
                return;
            }

            await _coreProvider.TuviMailCore.CheckForNewMessagesInFolderAsync(folder, CancellationToken.None).ConfigureAwait(false);
            _outputWriter.Write(new FolderSyncedOutput(accountAddress, folderName));
        }

        internal async Task ShowAllMessagesActionAsync(int pageSize)
        {
            _logger.LogMethodCall();

            await _outputCoordinator.WriteMessagesAsync(_application.GetPrintAllMessagesHeader(), pageSize, (count, lastMsg) => GetMessages(count, lastMsg, _coreProvider.TuviMailCore), _application.ConfirmAskMoreMessages).ConfigureAwait(false);

            static async Task<IEnumerable<Message>> GetMessages(int count, Message lastMessage, ITuviMail tuviMail)
            {
                return await tuviMail.GetAllEarlierMessagesAsync(count, lastMessage).ConfigureAwait(false);
            }
        }

        internal async Task ShowFolderMessagesActionAsync(string accountAddress, string folderName, int pageSize)
        {
            _logger.LogMethodCall();

            EmailAddress email = new(accountAddress);
            Account account = await _coreProvider.TuviMailCore.GetAccountAsync(email).ConfigureAwait(false);
            Folder? folder = account.FoldersStructure.Find(x => x.HasSameName(folderName));

            if (folder is null)
            {
                WriteUnknownFolderWarning(accountAddress, folderName);
                return;
            }

            await _outputCoordinator.WriteMessagesAsync(_application.GetPrintFolderMessagesHeader(accountAddress, folderName), pageSize, (count, lastMsg) => GetMessages(count, lastMsg, folder, _coreProvider.TuviMailCore), _application.ConfirmAskMoreMessages).ConfigureAwait(false);

            static async Task<IEnumerable<Message>> GetMessages(int count, Message lastMessage, Folder folder, ITuviMail tuviMail)
            {
                return await tuviMail.GetFolderEarlierMessagesAsync(folder, count, lastMessage).ConfigureAwait(false);
            }
        }

        internal async Task ShowContactMessagesActionAsync(string contactAddress, int pageSize)
        {
            _logger.LogMethodCall();

            EmailAddress contact = new(contactAddress);

            await _outputCoordinator.WriteMessagesAsync(_application.GetPrintContactMessagesHeader(contactAddress), pageSize, (count, lastMsg) => GetMessages(count, lastMsg, contact, _coreProvider.TuviMailCore), _application.ConfirmAskMoreMessages).ConfigureAwait(false);

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

        private async Task AddEmailAccountAsync()
        {
            _logger.LogMethodCall();

            async Task<Account> CreateAccountAsync()
            {
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

        private async Task AddProtonAccountAsync()
        {
            _logger.LogMethodCall();

            string email = _application.AskAccountAddress();

            (string userId, string refreshToken, string saltedKeyPass) = await Tuvi.Proton.ClientAuth.LoginFullAsync(
                email,
                _application.AskAccountPassword(),
                (ex, ct) => Task.FromResult((true, _application.AskTwoFactorCode(ex is null))),
                (ex, ct) => Task.FromResult((true, _application.AskMailboxPassword(ex is null))),
                null, // human verification does not supported in CLI
                default).ConfigureAwait(false);

            Account account = Account.Default;

            account.Email = new EmailAddress(email);
            account.Type = MailBoxType.Proton;
            account.AuthData = new ProtonAuthData()
            {
                UserId = userId,
                RefreshToken = refreshToken,
                SaltedPassword = saltedKeyPass
            };

            await _coreProvider.TuviMailCore.AddAccountAsync(account).ConfigureAwait(false);
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

            _logger.LogError("An error has occurred {Exception}", e.Exception);
            _outputWriter.Write(new UnhandledExceptionOutput(e.Exception));
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

        private void WriteInvalidPasswordWarning()
        {
            _logger.LogWarning("The command failed. (Reason: Invalid Password).");
            _outputWriter.Write(new InvalidPasswordWarningOutput());
        }

        private void WriteSecondInitializationWarning()
        {
            _logger.LogWarning("The command failed. (Reason: The application has already been initialized.).");
            _outputWriter.Write(new SecondInitializationWarningOutput());
        }

        private void WriteUninitializedAppWarning()
        {
            _logger.LogWarning("The command failed. (Reason: The application hasn't been initialized yet).");
            _outputWriter.Write(new UninitializedAppWarningOutput());
        }

        private void WriteImpossibleInitializationError()
        {
            _logger.LogError("The application could not be initialized.");
            _outputWriter.Write(new ImpossibleInitializationErrorOutput());
        }

        private void WriteUnknownFolderWarning(string address, string folder)
        {
            _logger.LogWarning("The command failed. (Reason: Unknown folder; Parameters: {@Parameters}).", new { Address = address, Folder = folder });
            _outputWriter.Write(new UnknownFolderWarningOutput(address, folder));
        }

        private bool ConfirmResetOrWarn(string commandName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(commandName);

            if (_launchOptions.NonInteractive && !_launchOptions.Yes)
            {
                _logger.LogWarning("The command failed. (Reason: The '{CommandName}' command requires '--yes' in '--non-interactive' mode to confirm data loss.).", commandName);
                _outputWriter.Write(new CommandRequiresYesInNonInteractiveModeWarningOutput(commandName));
                return false;
            }

            return _application.ConfirmReset();
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
