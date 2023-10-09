// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2023 Eppie(https://eppie.io)                                     //
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tuvi.Toolkit.Cli;
using Tuvi.Toolkit.Cli.CommandLine;

namespace Eppie.CLI.CommandMenu
{
    internal class MainMenu
    {
        private readonly IAsyncParser _commandParser;
        private readonly ILogger _logger;
        public MainMenu(ILogger logger)
        {
            _commandParser = Create();
            _logger = logger;
        }

        public static string? ReadCommand()
        {
            string? cmd = ReadValue("command: ");

            if (cmd is null)
            {
                Console.WriteLine();
            }

            return cmd;
        }

        public async Task InvokeCommandAsync(string? cmd)
        {
            try
            {
                if (cmd is null)
                {
                    ExitCommand();
                    return;
                }

                int result = await _commandParser.InvokeAsync(cmd).ConfigureAwait(false);

                _logger.LogTrace("Command {cmd} is completed with code: {result}", cmd, result);
            }
            catch (InvalidOperationException ex)
            {
                ConsoleExtension.WriteLine(ex.ToString(), ConsoleColor.Red);
            }
        }


        private IAsyncParser Create()
        {
            var parser = BaseParser.Default();

            ICommand root = parser.CreateRoot(
                subcommands: new[]
                {
                     parser.CreateCommand(
                        name: "exit",
                        description: "Exit the application.",
                        action: (cmd) => ExitCommand()
                    ),
                }
            );

            parser.Bind(root);

            return parser;
        }

        private void ExitCommand()
        {
            _logger.LogTrace("ExitCommand has been called.");
            Program.Host.Services.GetRequiredService<IHostApplicationLifetime>().StopApplication();
        }

        private static string? ReadValue(string message, ConsoleColor foreground = ConsoleColor.Gray)
        {
            return ConsoleExtension.ReadValue(message, (message) => ConsoleExtension.Write(message, foreground), Console.ReadLine);
        }
    }
}
