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
using Eppie.CLI.Tools;

using Microsoft.Extensions.Logging;

using Tuvi.Toolkit.Cli.CommandLine;

namespace Eppie.CLI.Menu
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal class MainMenu
    {
        private const string CommandMark = ">>>";

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
            _logger.LogMethodCall();
            IAsyncParser commandParser = Create();

            while (!stoppingToken.IsCancellationRequested)
            {
                await InvokeCommandAsync(commandParser, _application.ReadCommandMenu(CommandMark)).ConfigureAwait(false);
            }
        }

        private IAsyncParser Create()
        {
            _logger.LogMethodCall();

            IAsyncParser parser = BaseParser.Default();

            ICommand root = parser.CreateRoot(
                description: _resourceLoader.Strings.GetMenuDescription(_resourceLoader.AssemblyStrings.Title,
                                                                        _resourceLoader.AssemblyStrings.Version),
                subcommands: new[]
                {
                    CreateCommand(parser, MenuCommand.Exit, _resourceLoader.Strings.ExitDescription,
                                  action: (cmd) => _actions.ExitAction()),
                    CreateAsyncCommand(parser, MenuCommand.Initialize, _resourceLoader.Strings.InitDescription,
                                       action: (cmd) => _actions.InitActionAsync()),
                    CreateAsyncCommand(parser, MenuCommand.Reset, _resourceLoader.Strings.ResetDescription,
                                       action: (cmd) => _actions.ResetActionAsync()),
                    CreateAsyncCommand(parser, MenuCommand.Open, _resourceLoader.Strings.OpenDescription,
                                       action: (cmd) => _actions.OpenActionAsync()),
                    CreateAsyncCommand(parser, MenuCommand.ListAccounts, _resourceLoader.Strings.ListAccountsDescription,
                                       action: (cmd) => _actions.ListAccountsActionAsync()),
                    CreateAsyncCommand(parser, MenuCommand.AddAccount, _resourceLoader.Strings.AddAccountDescription,
                                      action: (cmd) => _actions.AddAccountActionAsync(MenuCommand.CommandAddAccountOptions.GetTypeValue(cmd)),
                                      options: MenuCommand.CommandAddAccountOptions.GetOptions(parser)),
                    CreateCommand(parser, MenuCommand.Restore, string.Empty,
                                  action: (cmd) => _actions.RestoreAction()),
                    CreateCommand(parser, MenuCommand.Send, string.Empty,
                                  action: (cmd) => _actions.SendAction()),
                    CreateCommand(parser, MenuCommand.ListContacts, string.Empty,
                                  action: (cmd) => _actions.ListContactsAction()),
                    CreateCommand(parser, MenuCommand.ShowMessage, string.Empty,
                                  action: (cmd) => _actions.ShowMessageAction()),
                    CreateCommand(parser, MenuCommand.ShowMessages, string.Empty,
                                  action: (cmd) => _actions.ShowMessagesAction()),
                    CreateCommand(parser, MenuCommand.Import, string.Empty,
                                  action: (cmd) => _actions.ImportAction()),
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

                _logger.LogDebug("Command {CommandName} is completed with code: {CommandResult}", cmd, result);
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

        private ICommand CreateAsyncCommand(IAsyncParser parser, string name, string description, Func<IAsyncCommand, Task>? action, IReadOnlyCollection<IOption>? options = null)
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

            return parser.CreateAsyncCommand(name, description, options, action: HandleException);
        }
    }
}
