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
using System.Globalization;

using Eppie.CLI.Options;
using Eppie.CLI.Services;
using Eppie.CLI.UserInteraction;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eppie.CLI
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal class Application : BackgroundService
    {
        private readonly ILogger<Application> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IHostEnvironment _environment;
        private readonly ResourceLoader _resourceLoader;
        private readonly ConsoleOptions _consoleOptions;
        public Application(
            ILoggerFactory loggerFactory,
            IHostApplicationLifetime lifetime,
            IHostEnvironment environment,
            ResourceLoader resourceLoader,
            IOptions<ConsoleOptions> consoleOptions)
        {
            Debug.Assert(consoleOptions is not null);

            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<Application>();
            _environment = environment;
            _resourceLoader = resourceLoader;

            _consoleOptions = consoleOptions.Value;

            _lifetime = lifetime;
            _lifetime.ApplicationStarted.Register(OnStarted);
            _lifetime.ApplicationStopping.Register(OnStopping);
            _lifetime.ApplicationStopped.Register(OnStopped);
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            InitializeConsole();
            WriteApplicationHeader();

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            if (!stoppingToken.IsCancellationRequested)
            {
                await ProcessAsync(stoppingToken).ConfigureAwait(false);
            }
        }

        private Task ProcessAsync(CancellationToken stoppingToken)
        {
            _logger.LogTrace("Application.ProcessAsync has been called.");

            using CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _lifetime.ApplicationStopping);
            MainMenu mainMenu = new(_loggerFactory, Program.Host.Services.GetRequiredService<CoreProvider>(), _resourceLoader);

            return mainMenu.LoopAsync(cancellationTokenSource.Token);
        }

        private void OnStopped()
        {
            _logger.LogDebug("Application has been stopped.");
        }

        private void OnStopping()
        {
            _logger.LogInformation(_resourceLoader.Strings.Goodbye);
        }

        private void OnStarted()
        {
            _logger.LogDebug("Application has been started.");
        }

        private void WriteApplicationHeader()
        {
            _logger.LogTrace("Application.WriteApplicationHeader has been called.");

            _logger.LogInformation(_resourceLoader.Strings.LogoFormat,
                                   _resourceLoader.AssemblyStrings.Title,
                                   _resourceLoader.AssemblyStrings.Version);
            _logger.LogInformation(_resourceLoader.Strings.Description);
            _logger.LogInformation(_resourceLoader.Strings.EnvironmentNameFormat, _environment.EnvironmentName);
            _logger.LogInformation(_resourceLoader.Strings.ContentRootPathFormat, _environment.ContentRootPath);
        }

        private void InitializeConsole()
        {
            Console.OutputEncoding = _consoleOptions.Encoding;
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = _consoleOptions.CultureInfo;
            _logger.LogDebug("OutputEncoding is {OutputEncoding}; CurrentCulture is {CurrentCulture}", Console.OutputEncoding, CultureInfo.CurrentCulture);

            Console.Title = _resourceLoader.AssemblyStrings.Title;
        }
    }
}
