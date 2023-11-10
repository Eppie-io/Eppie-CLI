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

using Tuvi.Core;

namespace Eppie.CLI.Menu
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal class Actions
    {
        private readonly ILogger<Actions> _logger;
        private readonly Application _application;
        private readonly CoreProvider _coreProvider;

        public Actions(
            ILogger<Actions> logger,
            Application application,
            CoreProvider coreProvider)
        {
            _logger = logger;
            _application = application;
            _coreProvider = coreProvider;
        }

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
            string password = _application.AskPassword();

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

            await _coreProvider.ResetAsync().ConfigureAwait(false);
            _application.WriteApplicationResetMessage();
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

        internal void ListAccountsAction()
        {
            _logger.LogMethodCall();
            throw new NotImplementedException();
        }

        internal void AddAccountAction()
        {
            _logger.LogMethodCall();
            throw new NotImplementedException();
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
    }
}
