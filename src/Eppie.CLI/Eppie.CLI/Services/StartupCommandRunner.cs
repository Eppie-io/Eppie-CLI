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
        ApplicationLaunchOptions launchOptions,
        IApplicationUnlocker applicationUnlocker,
        IApplicationMenu applicationMenu) : IStartupCommandRunner
    {
        private readonly ILogger<StartupCommandRunner> _logger = logger;
        private readonly ApplicationLaunchOptions _launchOptions = launchOptions;
        private readonly IApplicationUnlocker _applicationUnlocker = applicationUnlocker;
        private readonly IApplicationMenu _applicationMenu = applicationMenu;

        public async Task<bool> TryRunAsync(CancellationToken cancellationToken)
        {
            _logger.LogMethodCall();

            if (_launchOptions.StartupCommand is not string startupCommand)
            {
                return false;
            }

            if (ShouldUnlockBeforeExecutingCommand(startupCommand)
                && !await _applicationUnlocker.UnlockAsync(cancellationToken).ConfigureAwait(false))
            {
                return true;
            }

            await _applicationMenu.InvokeCommandAsync(startupCommand).ConfigureAwait(false);
            return true;
        }

        private bool ShouldUnlockBeforeExecutingCommand(string commandText)
        {
            return _launchOptions.UnlockPasswordFromStandardInput && MenuCommand.RequiresUnlockedApplication(GetCommandName(commandText));
        }

        private static string GetCommandName(string commandText)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(commandText);

            // TODO: Remove manual command-name parsing after extending parser API to expose command parsing without execution.

            ReadOnlySpan<char> commandTextSpan = commandText.AsSpan().TrimStart();
            int separatorIndex = commandTextSpan.IndexOfAny(" \t\r\n");

            return separatorIndex >= 0
                ? commandTextSpan[..separatorIndex].ToString()
                : commandTextSpan.ToString();
        }
    }
}
