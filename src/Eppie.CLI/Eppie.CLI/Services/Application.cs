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

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Tuvi.Toolkit.Cli;

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal class Application
    {
        private readonly IHostEnvironment _environment;
        private readonly ResourceLoader _resourceLoader;
        private readonly ILogger<Application> _logger;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ConsoleOptions _consoleOptions;

        public Application(
           ILogger<Application> logger,
           IHostApplicationLifetime lifetime,
           IHostEnvironment environment,
           ResourceLoader resourceLoader,
           IOptions<ConsoleOptions> consoleOptions)
        {
            Debug.Assert(consoleOptions is not null);

            _logger = logger;
            _lifetime = lifetime;
            _environment = environment;
            _resourceLoader = resourceLoader;

            _consoleOptions = consoleOptions.Value;
        }

        public void StopApplication()
        {
            _logger.LogDebug("Application.StopApplication has been called.");
            _lifetime.StopApplication();

            WriteGoodbye();
        }

        public string? ReadValue(string message, ConsoleColor foreground = ConsoleColor.Gray)
        {
            _logger.LogTrace("Application ReadValue has been called.");
            return ConsoleExtension.ReadValue(message, (message) => ConsoleExtension.Write(message, foreground), Console.ReadLine);
        }

        public string? ReadSecretValue(string message, ConsoleColor foreground = ConsoleColor.Gray)
        {
            _logger.LogTrace("Application ReadSecretValue has been called.");
            return ConsoleExtension.ReadValue(message, (message) => ConsoleExtension.Write(message, foreground), () => ConsoleExtension.ReadSecretLine());
        }

        public string? ReadCommandMenu(string commandMark)
        {
            string? cmd = ReadValue($"{commandMark} ");

            if (cmd is null)
            {
                Console.WriteLine();
            }

            return cmd;
        }

        public void InitializeConsole()
        {
            Console.OutputEncoding = _consoleOptions.Encoding;
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = _consoleOptions.CultureInfo;
            Console.Title = _resourceLoader.AssemblyStrings.Title;

            _logger.LogDebug("OutputEncoding is {OutputEncoding}; CurrentCulture is {CurrentCulture}",
                Console.OutputEncoding,
                CultureInfo.CurrentCulture);
        }

        public void WriteGreeting()
        {
            _logger.LogTrace("Application.WriteApplicationHeader has been called.");

            _logger.LogInformation(_resourceLoader.Strings.LogoFormat,
                                   _resourceLoader.AssemblyStrings.Title,
                                   _resourceLoader.AssemblyStrings.Version);
            _logger.LogInformation(_resourceLoader.Strings.Description);
            _logger.LogInformation(_resourceLoader.Strings.EnvironmentNameFormat, _environment.EnvironmentName);
            _logger.LogInformation(_resourceLoader.Strings.ContentRootPathFormat, _environment.ContentRootPath);
        }

        public void WriteGoodbye()
        {
            _logger.LogInformation(_resourceLoader.Strings.Goodbye);
        }

        // ToDo: CA1303:Do not pass literals as localized parameters - The output strings must be placed into resources.
        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "This is temporary. It should be placed in resources.")]
        public void WriteSeedPhrase(string[] seedPhrase)
        {
            _logger.LogTrace("Application.WriteSeedPhrase has been called.");

            Console.WriteLine("Your seed phrase is \n");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{string.Join(' ', seedPhrase)}\n");
            Console.ResetColor();
            Console.WriteLine("IMPORTANT, copy and keep it in secret");
        }

        public void Error(Exception ex)
        {
            Debug.Assert(ex is not null);
            _logger.LogTrace("Application.Error has been called.");
            _logger.LogError("An error has occurred {Exception}", ex);

            ConsoleExtension.WriteLine(ex.ToString(), ConsoleColor.Red);
        }
    }
}
