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

using Microsoft.Extensions.Logging;

using Tuvi.Core;

namespace Eppie.CLI.UserInteraction
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal class MenuCommand
    {
        private readonly ILogger<MenuCommand> _logger;
        //private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly CoreProvider _coreProvider;

        public MenuCommand(
            ILogger<MenuCommand> logger,
            CoreProvider coreProvider)
        {
            _logger = logger;
            //_hostApplicationLifetime = hostApplicationLifetime;
            _coreProvider = coreProvider;
        }

        internal void ExitAction()
        {
            _logger.LogDebug("MenuCommand.ExitAction has been called.");
            _logger.LogInformation("Exit.");
            //_hostApplicationLifetime.StopApplication();
        }

        // ToDo: CA1303:Do not pass literals as localized parameters - The output strings must be placed into resources.
        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "This is temporary. It should be placed in resources.")]
        internal async Task InitActionAsync()
        {
            _logger.LogTrace("MenuCommand.InitActionAsync has been called.");

            bool isFirstTime = await _coreProvider.TuviMailCore.IsFirstApplicationStartAsync().ConfigureAwait(false);

            if (!isFirstTime)
            {
                _logger.LogWarning("Eppie is already initialized.");
                return;
            }

            ISecurityManager sm = _coreProvider.TuviMailCore.GetSecurityManager();
            string[] seedPhrase = await sm.CreateSeedPhraseAsync().ConfigureAwait(false);
            string password = ConsoleElement.ReadSecretValue("Password: ") ?? string.Empty;

            if (password.Length == 0 || password != ConsoleElement.ReadSecretValue("Confirm password: "))
            {
                _logger.LogWarning("Invalid password.");
            }

            bool success = await _coreProvider.TuviMailCore.InitializeApplicationAsync(password).ConfigureAwait(false);

            if (success)
            {
                _logger.LogInformation("Eppie is initialized.");
                Console.WriteLine("Your seed phrase is \n");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{string.Join(' ', seedPhrase)}\n");
                Console.ResetColor();
                Console.WriteLine("IMPORTANT, copy and keep it in secret");
            }
            else
            {
                _logger.LogError("Eppie could not be initialized.");
            }
        }

        internal async Task ResetActionAsync()
        {
            _logger.LogTrace("MenuCommand.ResetActionAsync has been called.");

            try
            {
                await _coreProvider.ResetAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                throw;
            }

            _logger.LogInformation("Eppie was reset.");
        }

        internal async Task OpenActionAsync()
        {
            _logger.LogTrace("MenuCommand.OpenActionAsync has been called.");

            bool isFirstTime = await _coreProvider.TuviMailCore.IsFirstApplicationStartAsync().ConfigureAwait(false);

            if (isFirstTime)
            {
                _logger.LogWarning("Eppie hasn't been initialized yet.");
                return;
            }

            string password = ConsoleElement.ReadSecretValue("Password: ") ?? string.Empty;

            bool success = await _coreProvider.TuviMailCore.InitializeApplicationAsync(password).ConfigureAwait(false);

            if (success)
            {
                _logger.LogInformation("The instance was opened successfully.");
            }
            else
            {
                _logger.LogWarning("Invalid password.");
            }
        }

        internal void ListAccountsAction()
        {
            _logger.LogTrace("MenuCommand.ListAccountsAction has been called.");
        }

        internal void AddAccountAction()
        {
            _logger.LogTrace("MenuCommand.AddAccountAction has been called.");
        }

        internal void RestoreAction()
        {
            _logger.LogTrace("MenuCommand.RestoreAction has been called.");
        }

        internal void SendAction()
        {
            _logger.LogTrace("MenuCommand.SendAction has been called.");
        }

        internal void ListContactsAction()
        {
            _logger.LogTrace("MenuCommand.ListContactsAction has been called.");
        }

        internal void ShowMessageAction()
        {
            _logger.LogTrace("MenuCommand.ShowMessageAction has been called.");
        }

        internal void ShowMessagesAction()
        {
            _logger.LogTrace("MenuCommand.ShowMessagesAction has been called.");
        }

        internal void ImportAction()
        {
            _logger.LogTrace("MenuCommand.ImportAction has been called.");
        }
    }
}
