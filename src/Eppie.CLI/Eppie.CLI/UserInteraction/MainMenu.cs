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

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Tuvi.Toolkit.Cli;
using Tuvi.Toolkit.Cli.CommandLine;

namespace Eppie.CLI.UserInteraction
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal class MainMenu : WorkService
    {
        private readonly MenuCommand _menuCommand;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public MainMenu(ILogger<MainMenu> logger, MenuCommand menuCommand, IHostApplicationLifetime hostApplicationLifetime)
            : base(logger)
        {
            _menuCommand = menuCommand;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        protected override Task DoWorkAsync(CancellationToken stoppingToken)
        {
            _cancellationTokenSource.Cancel();

            Task.Run(async () =>
            {
                using CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _hostApplicationLifetime.ApplicationStopping);

                IAsyncParser commandParser = Create();

                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    await InvokeCommandAsync(commandParser, ReadCommand()).ConfigureAwait(false);
                }
            }, stoppingToken);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            base.Dispose();

            _cancellationTokenSource.Dispose();
        }

        public static string? ReadCommand()
        {
            string? cmd = ConsoleElement.ReadValue(">>> ");

            if (cmd is null)
            {
                Console.WriteLine();
            }

            return cmd;
        }

        public async Task InvokeCommandAsync(IAsyncParser commandParser, string? cmd)
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

                Logger.LogTrace("Command {cmd} is completed with code: {result}", cmd, result);
            }
            catch (InvalidOperationException ex)
            {
                ConsoleExtension.WriteLine(ex.ToString(), ConsoleColor.Red);
            }
        }

        private IAsyncParser Create()
        {
            IAsyncParser parser = BaseParser.Default();

            ICommand root = parser.CreateRoot(
                subcommands: new[]
                {
                    _menuCommand.CreateExitCommand(parser),
                    _menuCommand.CreateInitCommand(parser),
                    _menuCommand.CreateResetCommand(parser),
                    _menuCommand.CreateOpenCommand(parser),
                    _menuCommand.CreateListAccountsCommand(parser),
                    _menuCommand.CreateAddAccountCommand(parser),
                    _menuCommand.CreateRestoreCommand(parser),
                    _menuCommand.CreateSendCommand(parser),
                    _menuCommand.CreateListContactsCommand(parser),
                    _menuCommand.CreateShowMessageCommand(parser),
                    _menuCommand.CreateShowMessagesCommand(parser),
                    _menuCommand.CreateImportCommand(parser),
                }
            );

            parser.Bind(root);

            return parser;
        }
    }
}
