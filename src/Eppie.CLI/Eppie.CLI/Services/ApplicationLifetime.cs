// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2024 Eppie (https://eppie.io)                                    //
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
using Eppie.CLI.Tools;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal class ApplicationLifetime : IHostLifetime, IDisposable
    {
        private readonly ILogger<ApplicationLifetime> _logger;
        private readonly IHostEnvironment _environment;
        private readonly ResourceLoader _resourceLoader;
        private readonly ConsoleOptions _consoleOptions;

        private bool _disposedValue;

        public ApplicationLifetime(
           ILogger<ApplicationLifetime> logger,
           IHostEnvironment environment,
           ResourceLoader resourceLoader,
           IOptions<ConsoleOptions> consoleOptions)
        {
            Debug.Assert(consoleOptions is not null);

            _logger = logger;

            _environment = environment;
            _resourceLoader = resourceLoader;

            _consoleOptions = consoleOptions.Value;

            Console.CancelKeyPress += OnCancelKeyPressed;
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            InitializeConsole();
            WriteGreetingMessage();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            WriteGoodbyeMessage();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Console.CancelKeyPress -= OnCancelKeyPressed;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        private void InitializeConsole()
        {
            Console.InputEncoding = _consoleOptions.Encoding;
            Console.OutputEncoding = _consoleOptions.Encoding;
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = _consoleOptions.CultureInfo;
            Console.Title = _resourceLoader.AssemblyStrings.Title;

            _logger.LogDebug(
                "OutputEncoding is {OutputEncoding}; CurrentCulture is {CurrentCulture}",
                Console.OutputEncoding,
                CultureInfo.CurrentCulture);
        }

        private void WriteGreetingMessage()
        {
            _logger.LogDebug("Application title is '{ApplicationTitle}'; version is {ApplicationVersion}",
                            _resourceLoader.AssemblyStrings.Title,
                            _resourceLoader.AssemblyStrings.Version);
            Console.WriteLine(_resourceLoader.Strings.GetLogo(_resourceLoader.AssemblyStrings.Title,
                                                              _resourceLoader.AssemblyStrings.Version));

            {
                using IDisposable? consoleLogScope = _logger.BeginConsoleScope();
                _logger.LogInformation("Hosting environment: {EnvironmentName}", _environment.EnvironmentName);
                _logger.LogInformation("Content root path: {ContentRootPath}", _environment.ContentRootPath);
            }

            Console.WriteLine(_resourceLoader.Strings.Description);
        }

        private void WriteGoodbyeMessage()
        {
            _logger.LogMethodCall();
            Console.WriteLine(_resourceLoader.Strings.Goodbye);
        }

        private void OnCancelKeyPressed(object? sender, ConsoleCancelEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(e);
            e.Cancel = true;
        }
    }
}
