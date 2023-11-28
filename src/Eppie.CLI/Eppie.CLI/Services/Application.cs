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
using Eppie.CLI.Tools;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Tuvi.Core.Entities;
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

        internal void InitializeConsole()
        {
            Console.OutputEncoding = _consoleOptions.Encoding;
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = _consoleOptions.CultureInfo;
            Console.Title = _resourceLoader.AssemblyStrings.Title;

            _logger.LogDebug(
                "OutputEncoding is {OutputEncoding}; CurrentCulture is {CurrentCulture}",
                Console.OutputEncoding,
                CultureInfo.CurrentCulture);
        }

        internal void StopApplication()
        {
            _logger.LogMethodCall();
            _lifetime.StopApplication();

            WriteGoodbyeMessage();
        }

        internal string? ReadCommandMenu(string commandMark)
        {
            _logger.LogMethodCall();
            string? cmd = ReadValue($"{commandMark} ");

            if (cmd is null)
            {
                Console.WriteLine();
            }

            return cmd;
        }

        internal string AskPassword()
        {
            _logger.LogMethodCall();

            return ReadSecretValue(_resourceLoader.Strings.AskPassword);
        }

        internal string AskNewPassword()
        {
            _logger.LogMethodCall();

            return ReadSecretValue(_resourceLoader.Strings.AskNewPassword);
        }

        internal string ConfirmPassword()
        {
            _logger.LogMethodCall();

            return ReadSecretValue(_resourceLoader.Strings.ConfirmPassword);
        }

        internal string AskAccountAddress()
        {
            _logger.LogMethodCall();

            return ReadValue(_resourceLoader.Strings.AskAccountAddress);
        }

        internal string AskAccountPassword()
        {
            _logger.LogMethodCall();

            return ReadSecretValue(_resourceLoader.Strings.AskAccountPassword);
        }

        internal string AskIMAPServer()
        {
            _logger.LogMethodCall();

            return ReadValue(_resourceLoader.Strings.AskIMAPServer);
        }

        internal string AskSMTPServer()
        {
            _logger.LogMethodCall();

            return ReadValue(_resourceLoader.Strings.AskSMTPServer);
        }

        internal string AskSeedPhrase()
        {
            _logger.LogMethodCall();

            return ReadSecretValue(_resourceLoader.Strings.AskSeedPhrase);
        }

        internal string AskRestorePath()
        {
            _logger.LogMethodCall();

            return ReadValue(_resourceLoader.Strings.AskRestorePath);
        }

        internal bool ConfirmReset()
        {
            _logger.LogMethodCall();

            return ReadBoolValue(_resourceLoader.Strings.ConfirmReset);
        }

        internal void PrintAccounts(IReadOnlyCollection<Account> accounts)
        {
            _logger.LogMethodCall();

            if (accounts is null || accounts.Count == 0)
            {
                Console.WriteLine(_resourceLoader.Strings.EmptyAccountList);
                return;
            }

            Console.WriteLine(_resourceLoader.Strings.HeaderAccountList);
            foreach (Account account in accounts)
            {
                Console.WriteLine($"{account.Id}. {account.Email.Address}");
            }

            Console.WriteLine();
        }

        internal void WriteGreetingMessage()
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

        internal void WriteGoodbyeMessage()
        {
            _logger.LogMethodCall();
            Console.WriteLine(_resourceLoader.Strings.Goodbye);
        }

        internal void WriteApplicationInitializationMessage(string[] seedPhrase)
        {
            _logger.LogDebug("The application has been initialized.");

            Console.WriteLine();
            Console.WriteLine(_resourceLoader.Strings.SeedPhraseHeader);
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{string.Join(' ', seedPhrase)}");
            Console.ResetColor();

            Console.WriteLine();
            Console.WriteLine(_resourceLoader.Strings.SeedPhraseFooter);
        }

        internal void WriteApplicationResetMessage()
        {
            _logger.LogDebug("The application has been reset.");
            Console.WriteLine(_resourceLoader.Strings.AppReset);
        }

        internal void WriteApplicationOpenedMessage()
        {
            _logger.LogDebug("The application was opened.");
            Console.WriteLine(_resourceLoader.Strings.AppOpened);
        }

        internal void WriteSuccessfulRestoredMessage()
        {
            _logger.LogDebug("Eppie account was restored.");
            Console.WriteLine(_resourceLoader.Strings.AppRestored);
        }

        internal void WriteInvalidPasswordWarning()
        {
            LogCommandWarning("Invalid Password");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(_resourceLoader.Strings.InvalidPassword);
            Console.ResetColor();
        }

        internal void WriteSecondInitializationWarning()
        {
            LogCommandWarning("The application has already been initialized.");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(_resourceLoader.Strings.SecondInitialization);
            Console.ResetColor();
        }

        internal void WriteUninitializedAppWarning()
        {
            LogCommandWarning("The application hasn't been initialized yet.");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(_resourceLoader.Strings.Uninitialized);
            Console.ResetColor();
        }

        internal void WriteImpossibleInitializationError()
        {
            _logger.LogError("The application could not be initialized.");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(_resourceLoader.Strings.ImpossibleInitialization);
            Console.ResetColor();
        }

        internal void Error(Exception ex)
        {
            _logger.LogError("An error has occurred {Exception}", ex);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(_resourceLoader.Strings.GetUnhandledException(ex));
            Console.ResetColor();
        }

        private string ReadValue(string message, ConsoleColor foreground = ConsoleColor.Gray)
        {
            _logger.LogMethodCall();
            return ConsoleExtension.ReadValue(message, (message) => ConsoleExtension.Write(message, foreground), Console.ReadLine) ?? string.Empty;
        }

        private string ReadSecretValue(string message, ConsoleColor foreground = ConsoleColor.Gray)
        {
            _logger.LogMethodCall();
            return ConsoleExtension.ReadValue(message, (message) => ConsoleExtension.Write(message, foreground), () => ConsoleExtension.ReadSecretLine()) ?? string.Empty;
        }

        private bool ReadBoolValue(string message, ConsoleColor foreground = ConsoleColor.Gray)
        {
            _logger.LogMethodCall();
            return ConsoleExtension.ReadBool(message, (message) => ConsoleExtension.Write(message, foreground));
        }

        private void LogCommandWarning(string reason)
        {
            _logger.LogWarning("The command failed. (Reason: {WarningReason}).", reason);
        }
    }
}
