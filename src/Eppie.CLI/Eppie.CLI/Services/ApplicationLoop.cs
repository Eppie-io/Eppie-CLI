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

using Eppie.CLI.Menu;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal partial class ApplicationLoop : BackgroundService
    {
        private readonly ILogger<ApplicationLoop> _logger;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly Application _application;
        private readonly MainMenu _mainMenu;

        public ApplicationLoop(
            ILogger<ApplicationLoop> logger,
            IHostApplicationLifetime lifetime,
            Application application,
            MainMenu mainMenu)
        {
            _logger = logger;
            _mainMenu = mainMenu;
            _application = application;

            _lifetime = lifetime;
            _lifetime.ApplicationStarted.Register(OnStarted);
            _lifetime.ApplicationStopping.Register(OnStopping);
            _lifetime.ApplicationStopped.Register(OnStopped);
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("ApplicationLoop.StartAsync has been called.");
            _application.InitializeConsole();
            _application.WriteGreetingMessage();

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogTrace("ApplicationLoop.ExecuteAsync has been called.");
            await Task.Yield();

            if (!stoppingToken.IsCancellationRequested)
            {
                using CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _lifetime.ApplicationStopping);
                await _mainMenu.LoopAsync(cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        private void OnStopped()
        {
            _logger.LogDebug("Application has been stopped.");
        }

        private void OnStopping()
        {
            _logger.LogDebug("Application is stopping.");
        }

        private void OnStarted()
        {
            _logger.LogDebug("Application has been started.");
        }
    }
}
