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

using Eppie.CLI.CommandMenu;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Eppie.CLI
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal class Application : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _lifetime;
        private CancellationTokenSource _startProcess = new();

        public Application(
            ILogger<Application> logger,
            IHostApplicationLifetime lifetime)
        {
            _logger = logger;
            _lifetime = lifetime;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("ApplicationLifetimeService.StartAsync has been called.");

            _lifetime.ApplicationStarted.Register(OnStarted);
            _lifetime.ApplicationStopping.Register(OnStopping);
            _lifetime.ApplicationStopped.Register(OnStopped);

            return base.StartAsync(cancellationToken);
        }

        public override void Dispose()
        {
            base.Dispose();

            _startProcess.Cancel();
            _startProcess.Dispose();
            _startProcess = null!;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("ApplicationLifetimeService.StopAsync has been called.");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            await WaitAsync(stoppingToken).ConfigureAwait(false);
            await ProcessAsync(stoppingToken).ConfigureAwait(false);
        }

        private Task ProcessAsync(CancellationToken stoppingToken)
        {
            _logger.LogTrace("ApplicationLifetimeService.ProcessAsync has been called.");

            if (stoppingToken.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            Task.Run(async () =>
            {
                MainMenu mainMenu = new(_logger);

                while (Continue(stoppingToken))
                {
                    string? cmd = MainMenu.ReadCommand();
                    await mainMenu.InvokeCommandAsync(cmd).ConfigureAwait(false);
                }
            }, stoppingToken);

            return Task.CompletedTask;
        }

        private async Task WaitAsync(CancellationToken stoppingToken)
        {
            _logger.LogTrace("ApplicationLifetimeService.WaitAsync has been called.");

            try
            {
                using CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _startProcess.Token);

                await Task.Delay(Timeout.Infinite, linkedTokenSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            { }
        }

        private void OnStopped()
        {
            _logger.LogDebug("Application has been stopped.");
        }

        private void OnStopping()
        {
            _logger.LogInformation("Application is shutting down...");
        }

        private void OnStarted()
        {
            WriteApplicationHeader();
            ExecuteApplicationProcess();
        }

        private void WriteApplicationHeader()
        {
            _logger.LogTrace("ApplicationLifetimeService.WriteApplicationHeader has been called.");

            _logger.LogInformation(Program.ResourceLoader.Strings.LogoFormat,
                                   Program.ResourceLoader.AssemblyStrings.Title,
                                   Program.ResourceLoader.AssemblyStrings.Version);
            _logger.LogInformation(Program.ResourceLoader.Strings.Description);
            _logger.LogInformation(Program.ResourceLoader.Strings.EnvironmentNameFormat, Program.Environment.EnvironmentName);
            _logger.LogInformation(Program.ResourceLoader.Strings.ContentRootPathFormat, Program.Environment.ContentRootPath);
        }

        private void ExecuteApplicationProcess()
        {
            _startProcess.Cancel();
        }

        private bool Continue(CancellationToken stoppingToken)
        {
            return !(stoppingToken.IsCancellationRequested || _lifetime.ApplicationStopping.IsCancellationRequested);
        }
    }
}
