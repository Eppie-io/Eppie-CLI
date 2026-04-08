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

using Eppie.CLI.Tools;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal partial class ApplicationMenuLoop(
        ILogger<ApplicationMenuLoop> logger,
        IHostApplicationLifetime lifetime,
        IOptions<ApplicationLaunchOptions> launchOptions,
        IStartupCommandRunner startupCommandRunner,
        IApplicationOutputWriter outputWriter,
        Menu.IApplicationMenu applicationMenu) : BackgroundService
    {
        private const string InteractiveMenuOperationName = "interactive menu";

        private readonly ILogger<ApplicationMenuLoop> _logger = logger;
        private readonly IHostApplicationLifetime _lifetime = lifetime;
        private readonly ApplicationLaunchOptions _launchOptions = launchOptions.Value;
        private readonly IStartupCommandRunner _startupCommandRunner = startupCommandRunner;
        private readonly IApplicationOutputWriter _outputWriter = outputWriter;
        private readonly Menu.IApplicationMenu _applicationMenu = applicationMenu;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogMethodCall();
            await Task.Yield();

            if (!stoppingToken.IsCancellationRequested && await _startupCommandRunner.TryRunAsync(stoppingToken).ConfigureAwait(false))
            {
                _lifetime.StopApplication();
                return;
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                if (_launchOptions.NonInteractive)
                {
                    _outputWriter.Write(new NonInteractiveOperationNotSupportedErrorOutput(InteractiveMenuOperationName));
                    _lifetime.StopApplication();
                    return;
                }

                using CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _lifetime.ApplicationStopping);
                await _applicationMenu.LoopAsync(cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }
    }
}
