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

using Microsoft.Extensions.Logging;

using Tuvi.Toolkit.Cli.CommandLine;

namespace Eppie.CLI.Menu
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal class MainMenu
    {
        private const string CommandMark = ">>>";

        private static class MenuCommandName
        {
            public const string Exit = "exit";
            public const string Initialize = "init";
            public const string Open = "open";
            public const string Reset = "reset";
            public const string Restore = "restore";
            public const string Send = "send";
            public const string Import = "import";
            public const string ListAccounts = "list-accounts";
            public const string ListContacts = "list-contacts";
            public const string AddContact = "add-contact";
            public const string ShowMessage = "show-message";
            public const string ShowMessages = "show-messages";
        }

        private readonly ILogger<MainMenu> _logger;
        private readonly Actions _actions;
        private readonly Application _application;
        private readonly ResourceLoader _resourceLoader;

        public MainMenu(
            ILoggerFactory loggerFactory,
            Application application,
            CoreProvider coreProvider,
            ResourceLoader resourceLoader)
        {
            _logger = loggerFactory.CreateLogger<MainMenu>();
            _application = application;
            _resourceLoader = resourceLoader;
            _actions = new Actions(loggerFactory.CreateLogger<Actions>(), _application, coreProvider);
        }

        public async Task LoopAsync(CancellationToken stoppingToken)
        {
            _logger.LogTrace("MainMenu.LoopAsync has been called.");
            IAsyncParser commandParser = Create();

            while (!stoppingToken.IsCancellationRequested)
            {
                await InvokeCommandAsync(commandParser, _application.ReadCommandMenu(CommandMark)).ConfigureAwait(false);
            }
        }

        private IAsyncParser Create()
        {
            _logger.LogTrace("MainMenu.Create has been called.");

            IAsyncParser parser = BaseParser.Default();

            ICommand root = parser.CreateRoot(
                subcommands: new[]
                {
                    CreateCommand(parser, MenuCommandName.Exit, _resourceLoader.Strings.ExitDescription, action: (cmd) => _actions.ExitAction()),
                    CreateAsyncCommand(parser, MenuCommandName.Initialize, _resourceLoader.Strings.InitDescription, action: (cmd) => _actions.InitActionAsync()),
                    CreateAsyncCommand(parser, MenuCommandName.Reset, _resourceLoader.Strings.ResetDescription, action: (cmd) => _actions.ResetActionAsync()),
                    CreateAsyncCommand(parser, MenuCommandName.Open, _resourceLoader.Strings.OpenDescription, action: (cmd) => _actions.OpenActionAsync()),
                    CreateCommand(parser, MenuCommandName.ListAccounts, string.Empty, action: (cmd) => _actions.ListAccountsAction()),
                    CreateCommand(parser, MenuCommandName.AddContact, string.Empty, action: (cmd) => _actions.AddAccountAction()),
                    CreateCommand(parser, MenuCommandName.Restore, string.Empty, action: (cmd) => _actions.RestoreAction()),
                    CreateCommand(parser, MenuCommandName.Send, string.Empty, action: (cmd) => _actions.SendAction()),
                    CreateCommand(parser, MenuCommandName.ListContacts, string.Empty, action: (cmd) => _actions.ListContactsAction()),
                    CreateCommand(parser, MenuCommandName.ShowMessage, string.Empty, action: (cmd) => _actions.ShowMessageAction()),
                    CreateCommand(parser, MenuCommandName.ShowMessages, string.Empty, action: (cmd) => _actions.ShowMessagesAction()),
                    CreateCommand(parser, MenuCommandName.Import, string.Empty, action: (cmd) => _actions.ImportAction()),
                }
            );

            parser.Bind(root);

            return parser;
        }

        private async Task InvokeCommandAsync(IAsyncParser commandParser, string? cmd)
        {
            ArgumentNullException.ThrowIfNull(commandParser);

            try
            {
                if (cmd is null)
                {
                    _actions.ExitAction();
                    return;
                }

                int result = await commandParser.InvokeAsync(cmd).ConfigureAwait(false);

                _logger.LogDebug("Command {cmd} is completed with code: {result}", cmd, result);
            }
            catch (InvalidOperationException ex)
            {
                _application.Error(ex);
            }
        }

        private ICommand CreateCommand(IAsyncParser parser, string name, string description, Action<ICommand>? action)
        {
            Debug.Assert(parser is not null);

            [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "to log exceptions")]
            void HandleException(ICommand cmd)
            {
                try
                {
                    action?.Invoke(cmd);
                }
                catch (Exception ex)
                {
                    _application.Error(ex);
                }
            }

            return parser.CreateCommand(name, description, action: HandleException);
        }

        private ICommand CreateAsyncCommand(IAsyncParser parser, string name, string description, Func<IAsyncCommand, Task>? action)
        {
            Debug.Assert(parser is not null);

            [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "to log exceptions")]
            async Task HandleException(IAsyncCommand cmd)
            {
                try
                {
                    if (action is not null)
                    {
                        await action.Invoke(cmd).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    _application.Error(ex);
                }
            }

            return parser.CreateAsyncCommand(name, description, action: HandleException);
        }
    }
}
