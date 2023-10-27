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

using Tuvi.Toolkit.Cli;
using Tuvi.Toolkit.Cli.CommandLine;

namespace Eppie.CLI.UserInteraction
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
        private readonly MenuCommand _menuCommand;
        private readonly ResourceLoader _resourceLoader;

        public MainMenu(
            ILoggerFactory loggerFactory,
            CoreProvider coreProvider,
            ResourceLoader resourceLoader)
        {
            _logger = loggerFactory.CreateLogger<MainMenu>();
            _resourceLoader = resourceLoader;
            _menuCommand = new MenuCommand(loggerFactory.CreateLogger<MenuCommand>(), coreProvider);
        }

        public async Task LoopAsync(CancellationToken stoppingToken)
        {
            IAsyncParser commandParser = Create();

            while (!stoppingToken.IsCancellationRequested)
            {
                await InvokeCommandAsync(commandParser, ReadCommand()).ConfigureAwait(false);
            }
        }

        private IAsyncParser Create()
        {
            IAsyncParser parser = BaseParser.Default();

            ICommand root = parser.CreateRoot(
                subcommands: new[]
                {
                    CreateCommand(parser, MenuCommandName.Exit, _resourceLoader.Strings.ExitDescription, action: (cmd) => _menuCommand.ExitAction()),
                    CreateAsyncCommand(parser, MenuCommandName.Initialize, _resourceLoader.Strings.InitDescription, action: (cmd) => _menuCommand.InitActionAsync()),
                    CreateAsyncCommand(parser, MenuCommandName.Reset, _resourceLoader.Strings.ResetDescription, action: (cmd) => _menuCommand.ResetActionAsync()),
                    CreateAsyncCommand(parser, MenuCommandName.Open, _resourceLoader.Strings.OpenDescription, action: (cmd) => _menuCommand.OpenActionAsync()),
                    CreateCommand(parser, MenuCommandName.ListAccounts, string.Empty, action: (cmd) => _menuCommand.ListAccountsAction()),
                    CreateCommand(parser, MenuCommandName.AddContact, string.Empty, action: (cmd) => _menuCommand.AddAccountAction()),
                    CreateCommand(parser, MenuCommandName.Restore, string.Empty, action: (cmd) => _menuCommand.RestoreAction()),
                    CreateCommand(parser, MenuCommandName.Send, string.Empty, action: (cmd) => _menuCommand.SendAction()),
                    CreateCommand(parser, MenuCommandName.ListContacts, string.Empty, action: (cmd) => _menuCommand.ListContactsAction()),
                    CreateCommand(parser, MenuCommandName.ShowMessage, string.Empty, action: (cmd) => _menuCommand.ShowMessageAction()),
                    CreateCommand(parser, MenuCommandName.ShowMessages, string.Empty, action: (cmd) => _menuCommand.ShowMessagesAction()),
                    CreateCommand(parser, MenuCommandName.Import, string.Empty, action: (cmd) => _menuCommand.ImportAction()),
                }
            );

            parser.Bind(root);

            return parser;
        }

        private static string? ReadCommand()
        {
            string? cmd = ConsoleElement.ReadValue($"{CommandMark} ");

            if (cmd is null)
            {
                Console.WriteLine();
            }

            return cmd;
        }

        private async Task InvokeCommandAsync(IAsyncParser commandParser, string? cmd)
        {
            ArgumentNullException.ThrowIfNull(commandParser);

            try
            {
                if (cmd is null)
                {
                    _menuCommand.ExitAction();
                    return;
                }

                int result = await commandParser.InvokeAsync(cmd).ConfigureAwait(false);

                _logger.LogTrace("Command {cmd} is completed with code: {result}", cmd, result);
            }
            catch (InvalidOperationException ex)
            {
                ConsoleExtension.WriteLine(ex.ToString(), ConsoleColor.Red);
            }
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
