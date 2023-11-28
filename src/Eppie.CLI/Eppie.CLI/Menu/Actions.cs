// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2023 Eppie (https://eppie.io)                                    //
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

using Eppie.CLI.Services;
using Eppie.CLI.Tools;

using Microsoft.Extensions.Logging;

using Tuvi;
using Tuvi.Core;
using Tuvi.Core.Entities;

namespace Eppie.CLI.Menu
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal class Actions(
        ILogger<Actions> logger,
        Application application,
        CoreProvider coreProvider)
    {
        private readonly ILogger<Actions> _logger = logger;
        private readonly Application _application = application;
        private readonly CoreProvider _coreProvider = coreProvider;

        internal void ExitAction()
        {
            _logger.LogMethodCall();
            _application.StopApplication();
        }

        internal async Task InitActionAsync()
        {
            _logger.LogMethodCall();

            bool isFirstTime = await _coreProvider.TuviMailCore.IsFirstApplicationStartAsync().ConfigureAwait(false);

            if (!isFirstTime)
            {
                _application.WriteSecondInitializationWarning();
                return;
            }

            ISecurityManager sm = _coreProvider.TuviMailCore.GetSecurityManager();
            string[] seedPhrase = await sm.CreateSeedPhraseAsync().ConfigureAwait(false);
            string password = _application.AskNewPassword();

            if (password.Length == 0 || password != _application.ConfirmPassword())
            {
                _application.WriteInvalidPasswordWarning();
                return;
            }

            bool success = await _coreProvider.TuviMailCore.InitializeApplicationAsync(password).ConfigureAwait(false);

            if (success)
            {
                _application.WriteApplicationInitializationMessage(seedPhrase);
            }
            else
            {
                _application.WriteImpossibleInitializationError();
            }
        }

        internal async Task ResetActionAsync()
        {
            _logger.LogMethodCall();

            if (_application.ConfirmReset())
            {
                await _coreProvider.ResetAsync().ConfigureAwait(false);
                _application.WriteApplicationResetMessage();
            }
        }

        internal async Task OpenActionAsync()
        {
            _logger.LogMethodCall();

            bool isFirstTime = await _coreProvider.TuviMailCore.IsFirstApplicationStartAsync().ConfigureAwait(false);

            if (isFirstTime)
            {
                _application.WriteUninitializedAppWarning();
                return;
            }

            bool success = await _coreProvider.TuviMailCore.InitializeApplicationAsync(_application.AskPassword()).ConfigureAwait(false);

            if (success)
            {
                _application.WriteApplicationOpenedMessage();
            }
            else
            {
                _application.WriteInvalidPasswordWarning();
            }
        }

        internal async Task ListAccountsActionAsync()
        {
            _logger.LogMethodCall();

            List<Account> accounts = await _coreProvider.TuviMailCore.GetAccountsAsync().ConfigureAwait(true);
            accounts.Sort((a, b) => a.Id.CompareTo(b.Id));

            _application.PrintAccounts(accounts);
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

        internal void RestoreAction()
        {
            _logger.LogMethodCall();
            throw new NotImplementedException();
        }

        internal void SendAction()
        {
            _logger.LogMethodCall();
            throw new NotImplementedException();
        }

        internal void ListContactsAction()
        {
            _logger.LogMethodCall();
            throw new NotImplementedException();
        }

        internal void ShowMessageAction()
        {
            _logger.LogMethodCall();
            throw new NotImplementedException();
        }

        internal void ShowMessagesAction()
        {
            _logger.LogMethodCall();
            throw new NotImplementedException();
        }

        internal void ImportAction()
        {
            _logger.LogMethodCall();
            throw new NotImplementedException();
        }

        private async Task AddEmailAccountAsync()
        {
            _logger.LogMethodCall();

            Account account = await GetDefaultAccountAsync().ConfigureAwait(false);
            await _coreProvider.TuviMailCore.AddAccountAsync(account).ConfigureAwait(false);

            Task<Account> GetDefaultAccountAsync()
            {
                // TODO:
                // 1) add email provider selection;
                // 2) add OAuth2 authorization;

                Account account = Account.Default;
                account.Email = new EmailAddress(_application.AskAccountAddress());

                BasicAuthData basicData = new()
                {
                    Password = _application.AskAccountPassword()
                };

                account.AuthData = basicData;

                account.IncomingServerAddress = _application.AskIMAPServer();
                account.OutgoingServerAddress = _application.AskSMTPServer();

                return Task.FromResult(account);
            }
        }

        private async Task AddProtonAccountAsync()
        {
            _logger.LogMethodCall();

            //ToDo: This old code. ProtonAuthData must be use.

            Account account = new()
            {
                Email = new EmailAddress(_application.AskAccountAddress())
            };

            BasicAuthData basicData = new()
            {
                Password = _application.AskAccountPassword()
            };

            account.AuthData = basicData;
            account.Type = (int)MailBoxType.Proton;

            await _coreProvider.TuviMailCore.AddAccountAsync(account).ConfigureAwait(false);
        }

        private async Task AddDecAccountAsync()
        {
            _logger.LogMethodCall();

            Account account = await _coreProvider.TuviMailCore.NewDecentralizedAccountAsync().ConfigureAwait(false);
            account.Type = (int)MailBoxType.Dec;
            await _coreProvider.TuviMailCore.AddAccountAsync(account).ConfigureAwait(false);
        }
    }
}
