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

using Eppie.CLI.Menu;
using Eppie.CLI.Tools;

using Microsoft.Extensions.Logging;

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal sealed class StartupCommandRunner(
        ILogger<StartupCommandRunner> logger,
        RawCommandLineArguments commandLineArguments,
        ApplicationLaunchOptions launchOptions,
        IApplicationOutputWriter outputWriter,
        IApplicationUnlocker applicationUnlocker,
        IApplicationMenu applicationMenu) : IStartupCommandRunner
    {
        private readonly ILogger<StartupCommandRunner> _logger = logger;
        private readonly RawCommandLineArguments _commandLineArguments = commandLineArguments;
        private readonly ApplicationLaunchOptions _launchOptions = launchOptions;
        private readonly IApplicationOutputWriter _outputWriter = outputWriter;
        private readonly IApplicationUnlocker _applicationUnlocker = applicationUnlocker;
        private readonly IApplicationMenu _applicationMenu = applicationMenu;

        public async Task<bool> TryRunAsync(CancellationToken cancellationToken)
        {
            _logger.LogMethodCall();

            string[] startupCommandArguments = StartupCommandArguments.GetStartupCommandArguments(_commandLineArguments);

            if (startupCommandArguments.Length == 0)
            {
                return false;
            }

            string commandName = startupCommandArguments[0];

            if (MenuCommand.RequiresUnlockedApplication(commandName))
            {
                if (_launchOptions.NonInteractive && !_launchOptions.UnlockPasswordFromStandardInput)
                {
                    WriteUnlockPasswordFromStandardInputHint(commandName);
                    return true;
                }

                if (!await _applicationUnlocker.UnlockAsync(cancellationToken, readPasswordFromStandardInput: _launchOptions.NonInteractive).ConfigureAwait(false))
                {
                    return true;
                }
            }

            await _applicationMenu.InvokeCommandAsync(startupCommandArguments).ConfigureAwait(false);
            return true;
        }

        private void WriteUnlockPasswordFromStandardInputHint(string commandName)
        {
            _outputWriter.Write(new StartupCommandRequiresUnlockPasswordFromStandardInputWarningOutput(commandName));
        }
    }
}
