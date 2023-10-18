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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using Eppie.CLI.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Tuvi.Core;
using Tuvi.Toolkit.Cli.CommandLine;

namespace Eppie.CLI.UserInteraction
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal class MenuCommand
    {
        private readonly ILogger<MenuCommand> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly CoreProvider _coreProvider;
        private readonly ResourceLoader _resourceLoader;

        public MenuCommand(
            ILogger<MenuCommand> logger,
            IServiceProvider serviceProvider,
            CoreProvider coreProvider,
            ResourceLoader resourceLoader)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _coreProvider = coreProvider;
            _resourceLoader = resourceLoader;
        }

        public ICommand CreateExitCommand(IAsyncParser parser)
        {
            return CreateCommand(parser, "exit", _resourceLoader.Strings.ExitDescription, action: (cmd) => ExitAction());
        }

        public ICommand CreateInitCommand(IAsyncParser parser)
        {
            return CreateAsyncCommand(parser, "init", _resourceLoader.Strings.InitDescription, action: (cmd) => InitActionAsync());
        }

        public ICommand CreateResetCommand(IAsyncParser parser)
        {
            return CreateAsyncCommand(parser, "reset", _resourceLoader.Strings.ResetDescription, action: (cmd) => ResetActionAsync());
        }

        public ICommand CreateOpenCommand(IAsyncParser parser)
        {
            return CreateAsyncCommand(parser, "open", _resourceLoader.Strings.OpenDescription, action: (cmd) => OpenActionAsync());
        }

        public ICommand CreateListAccountsCommand(IAsyncParser parser)
        {
            return CreateCommand(parser, "list-accounts", string.Empty, action: (cmd) => ListAccountsAction());
        }

        public ICommand CreateAddAccountCommand(IAsyncParser parser)
        {
            return CreateCommand(parser, "add-account", string.Empty, action: (cmd) => AddAccountAction());
        }

        public ICommand CreateRestoreCommand(IAsyncParser parser)
        {
            return CreateCommand(parser, "restore", string.Empty, action: (cmd) => RestoreAction());
        }

        public ICommand CreateSendCommand(IAsyncParser parser)
        {
            return CreateCommand(parser, "send", string.Empty, action: (cmd) => SendAction());
        }

        public ICommand CreateListContactsCommand(IAsyncParser parser)
        {
            return CreateCommand(parser, "list-contacts", string.Empty, action: (cmd) => ListContactsAction());
        }

        public ICommand CreateShowMessageCommand(IAsyncParser parser)
        {
            return CreateCommand(parser, "show-message", string.Empty, action: (cmd) => ShowMessageAction());
        }

        public ICommand CreateShowMessagesCommand(IAsyncParser parser)
        {
            return CreateCommand(parser, "show-messages", string.Empty, action: (cmd) => ShowMessagesAction());
        }

        public ICommand CreateImportCommand(IAsyncParser parser)
        {
            return CreateCommand(parser, "import", string.Empty, action: (cmd) => ImportAction());
        }

        public void ExitAction()
        {
            _logger.LogDebug("MenuCommand.ExitAction has been called.");
            _serviceProvider.GetRequiredService<IHostApplicationLifetime>().StopApplication();
        }

        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        private async Task InitActionAsync()
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

        private async Task ResetActionAsync()
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

        private async Task OpenActionAsync()
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

        private void ListAccountsAction()
        {
            _logger.LogTrace("MenuCommand.ListAccountsAction has been called.");
        }

        private void AddAccountAction()
        {
            _logger.LogTrace("MenuCommand.AddAccountAction has been called.");
        }

        private void RestoreAction()
        {
            _logger.LogTrace("MenuCommand.RestoreAction has been called.");
        }

        private void SendAction()
        {
            _logger.LogTrace("MenuCommand.SendAction has been called.");
        }

        private void ListContactsAction()
        {
            _logger.LogTrace("MenuCommand.ListContactsAction has been called.");
        }

        private void ShowMessageAction()
        {
            _logger.LogTrace("MenuCommand.ShowMessageAction has been called.");
        }

        private void ShowMessagesAction()
        {
            _logger.LogTrace("MenuCommand.ShowMessagesAction has been called.");
        }

        private void ImportAction()
        {
            _logger.LogTrace("MenuCommand.ImportAction has been called.");
        }

        private static ICommand CreateCommand(IAsyncParser parser, string name, string description, Action<ICommand>? action)
        {
            Debug.Assert(parser is not null);
            return parser.CreateCommand(name, description, action: action);
        }

        private static ICommand CreateAsyncCommand(IAsyncParser parser, string name, string description, Func<IAsyncCommand, Task>? action)
        {
            Debug.Assert(parser is not null);
            return parser.CreateAsyncCommand(name, description, action: action);
        }
    }
}
