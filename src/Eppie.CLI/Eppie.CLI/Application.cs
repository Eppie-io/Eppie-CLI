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
using Eppie.CLI.Services;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Eppie.CLI
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal class Application : BackgroundService
    {
        public ProgramConfiguration Configuration { get; }
        public IHostEnvironment Environment { get; }
        public ResourceLoader ResourceLoader { get; }

        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _lifetime;

        private readonly CancellationTokenSource _waitProcess = new();

        public Application(
            ILogger<Application> logger,
            IHostApplicationLifetime lifetime,
            IHostEnvironment environment,
            ProgramConfiguration programConfiguration,
            ResourceLoader resourceLoader)
        {
            _logger = logger;
            _lifetime = lifetime;
            Configuration = programConfiguration;
            Environment = environment;
            ResourceLoader = resourceLoader;
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

            _waitProcess.Dispose();
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

                using CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _lifetime.ApplicationStopping);

                while (!linkedTokenSource.IsCancellationRequested)
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
                using CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _waitProcess.Token);

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
            _logger.LogTrace("Application.WriteApplicationHeader has been called.");

            _logger.LogInformation(ResourceLoader.Strings.LogoFormat,
                                   ResourceLoader.AssemblyStrings.Title,
                                   ResourceLoader.AssemblyStrings.Version);
            _logger.LogInformation(ResourceLoader.Strings.Description);
            _logger.LogInformation(ResourceLoader.Strings.EnvironmentNameFormat, Environment.EnvironmentName);
            _logger.LogInformation(ResourceLoader.Strings.ContentRootPathFormat, Environment.ContentRootPath);
        }

        private void ExecuteApplicationProcess()
        {
            _waitProcess.Cancel();
        }
    }
}
