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

using Eppie.CLI.Common;
using Eppie.CLI.Exceptions;
using Eppie.CLI.Options;
using Eppie.CLI.Tools;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Tuvi.Toolkit.Cli;

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal class Application(
       ILogger<Application> logger,
       IHostApplicationLifetime lifetime,
       ApplicationLaunchOptions launchOptions,
       IApplicationOutputWriter outputWriter,
       IOptions<MailOptions> mailOptions,
        ResourceLoader resourceLoader) : IApplicationPasswordReader
    {
        private readonly ResourceLoader _resourceLoader = resourceLoader;
        private readonly ILogger<Application> _logger = logger;
        private readonly IHostApplicationLifetime _lifetime = lifetime;
        private readonly ApplicationLaunchOptions _launchOptions = launchOptions;
        private readonly IApplicationOutputWriter _outputWriter = outputWriter;
        private readonly IOptions<MailOptions> _mailOptions = mailOptions;

        internal void StopApplication()
        {
            _logger.LogMethodCall();
            _lifetime.StopApplication();
        }

        internal string AskPassword()
        {
            _logger.LogMethodCall();

            return _launchOptions.NonInteractive
                ? ReadPasswordFromStandardInput()
                : ReadSecretValue(_resourceLoader.Strings.AskPassword);
        }

        internal string ReadPasswordFromStandardInput()
        {
            _logger.LogMethodCall();

            return ReadValue(_resourceLoader.Strings.AskPassword, writePrompt: !_launchOptions.NonInteractive);
        }

        string IApplicationPasswordReader.AskPassword()
        {
            return AskPassword();
        }

        string IApplicationPasswordReader.ReadPasswordFromStandardInput()
        {
            return ReadPasswordFromStandardInput();
        }

        internal string AskNewPassword()
        {
            _logger.LogMethodCall();

            return _launchOptions.NonInteractive
                ? ReadValue(_resourceLoader.Strings.AskNewPassword, writePrompt: false)
                : ReadSecretValue(_resourceLoader.Strings.AskNewPassword);
        }

        internal string ConfirmPassword()
        {
            _logger.LogMethodCall();

            return _launchOptions.NonInteractive
                ? ReadValue(_resourceLoader.Strings.ConfirmPassword, writePrompt: false)
                : ReadSecretValue(_resourceLoader.Strings.ConfirmPassword);
        }

        internal string AskAccountAddress()
        {
            _logger.LogMethodCall();

            return _launchOptions.NonInteractive
                ? ReadValue(_resourceLoader.Strings.AskAccountAddress, writePrompt: false)
                : ReadValue(_resourceLoader.Strings.AskAccountAddress);
        }

        internal string AskAccountPassword()
        {
            _logger.LogMethodCall();

            return _launchOptions.NonInteractive
                ? ReadValue(_resourceLoader.Strings.AskAccountPassword, writePrompt: false)
                : ReadSecretValue(_resourceLoader.Strings.AskAccountPassword);
        }

        internal string AskTwoFactorCode(bool firstAttempt)
        {
            _logger.LogMethodCall();

            if (!firstAttempt)
            {
                _outputWriter.Write(new UnsuccessfulAttemptWarningOutput());
            }

            return ReadValue(_resourceLoader.Strings.AskTwoFactorCode);
        }

        internal string AskMailboxPassword(bool firstAttempt)
        {
            _logger.LogMethodCall();

            if (!firstAttempt)
            {
                _outputWriter.Write(new UnsuccessfulAttemptWarningOutput());
            }

            return _launchOptions.NonInteractive
                ? ReadValue(_resourceLoader.Strings.AskMailboxPassword, writePrompt: false)
                : ReadSecretValue(_resourceLoader.Strings.AskMailboxPassword);
        }

        internal string AskIMAPServer(MailServer mailServer)
        {
            _logger.LogMethodCall();

            MailServerConfiguration config = GetMailServerConfiguration(mailServer);
            return AskQuestionWithDefault(_resourceLoader.Strings.GetIMAPServerQuestionText(config.IMAP), config.IMAP);
        }

        internal string AskSMTPServer(MailServer mailServer)
        {
            _logger.LogMethodCall();

            MailServerConfiguration config = GetMailServerConfiguration(mailServer);
            return AskQuestionWithDefault(_resourceLoader.Strings.GetSMTPServerQuestionText(config.SMTP), config.SMTP);
        }

        internal int AskIMAPServerPort(MailServer mailServer)
        {
            _logger.LogMethodCall();

            MailServerConfiguration config = GetMailServerConfiguration(mailServer);
            return AskQuestionWithDefault(_resourceLoader.Strings.GetIMAPPortQuestionText(config.IMAPPort), config.IMAPPort);
        }

        internal int AskSMTPServerPort(MailServer mailServer)
        {
            _logger.LogMethodCall();

            MailServerConfiguration config = GetMailServerConfiguration(mailServer);
            return AskQuestionWithDefault(_resourceLoader.Strings.GetSMTPPortQuestionText(config.SMTPPort), config.SMTPPort);
        }

        internal int AskQuestionWithDefault(string text, int defaultValue)
        {
            _logger.LogMethodCall();

            return int.TryParse(ReadValue(text), out int port) && port > 0
                ? port
                : defaultValue;
        }

        internal string AskQuestionWithDefault(string text, string defaultValue)
        {
            _logger.LogMethodCall();

            string answer = ReadValue(text);
            return string.IsNullOrEmpty(answer) ? defaultValue : answer;
        }

        private MailServerConfiguration GetMailServerConfiguration(MailServer mailServer)
        {
            return _mailOptions.Value.Servers.GetValueOrDefault(mailServer) ?? new MailServerConfiguration();
        }

        internal string AskSeedPhrase()
        {
            _logger.LogMethodCall();

            return _launchOptions.NonInteractive
                ? ReadValue(_resourceLoader.Strings.AskSeedPhrase, writePrompt: false)
                : ReadSecretValue(_resourceLoader.Strings.AskSeedPhrase);
        }

        internal string AskRestorePath()
        {
            _logger.LogMethodCall();

            return ReadValue(_resourceLoader.Strings.AskRestorePath);
        }

        internal TEnum SelectOption<TEnum>(TEnum defaultOption, bool ignoreCase = false)
            where TEnum : struct, Enum
        {
            _logger.LogMethodCall();

            if (_launchOptions.NonInteractive)
            {
                throw new InvalidOperationException(_resourceLoader.Strings.GetNonInteractiveOperationNotSupportedError("option selection"));
            }

            Console.WriteLine(_resourceLoader.Strings.SelectOptionHeader);

            int i = 0;
            foreach (string name in Enum.GetNames<TEnum>())
            {
                Console.WriteLine($"{i}) {name}");
                ++i;
            }

            string value = ReadValue(_resourceLoader.Strings.GetAskOptionText(defaultOption.ToString()));
            return Enum.TryParse(value, ignoreCase, out TEnum option) && Enum.IsDefined(option) ? option : defaultOption;
        }

        internal bool ConfirmReset()
        {
            _logger.LogMethodCall();

            return _launchOptions.AssumeYes || ReadBoolValue(_resourceLoader.Strings.ConfirmReset);
        }

        internal bool ConfirmAskMoreContacts()
        {
            _logger.LogMethodCall();

            return ReadBoolValue(_resourceLoader.Strings.AskMoreContacts);
        }

        internal bool ConfirmAskMoreMessages()
        {
            _logger.LogMethodCall();

            return ReadBoolValue(_resourceLoader.Strings.AskMoreMessages);
        }

        internal string AskMessageBody()
        {
            _logger.LogMethodCall();

            return _launchOptions.NonInteractive
                ? ReadRemainingStandardInput()
                : ConsoleExtension.ReadMultiLine(_resourceLoader.Strings.AskMessageBody, "EOF") ?? throw new ReadValueCanceledException();
        }

        internal string GetPrintAllMessagesHeader()
        {
            _logger.LogMethodCall();

            return _resourceLoader.Strings.PrintAllMessagesHeader;
        }

        internal string GetPrintFolderMessagesHeader(string accountAddress, string folderName)
        {
            _logger.LogMethodCall();

            return _resourceLoader.Strings.GetPrintFolderMessagesHeader(accountAddress, folderName);
        }

        internal string GetPrintContactMessagesHeader(string contactAddress)
        {
            _logger.LogMethodCall();

            return _resourceLoader.Strings.GetPrintContactMessagesHeader(contactAddress);
        }

        internal string ReadValue(string message, ConsoleColor foreground = ConsoleColor.Gray)
        {
            return ReadValue(message, writePrompt: true, foreground);
        }

        internal string ReadValue(string message, bool writePrompt, ConsoleColor foreground = ConsoleColor.Gray)
        {
            _logger.LogMethodCall();

            return ConsoleExtension.ReadValue(writePrompt ? message : string.Empty,
                                              (message) =>
                                              {
                                                  if (!string.IsNullOrEmpty(message))
                                                  {
                                                      ConsoleExtension.Write(message, foreground);
                                                  }
                                              },
                                              Console.ReadLine) ?? throw new ReadValueCanceledException();
        }

        internal Task<string> ReadStandardInputToEndAsync()
        {
            _logger.LogMethodCall();
            return Console.In.ReadToEndAsync();
        }

        private string ReadRemainingStandardInput()
        {
            _logger.LogMethodCall();

            List<string> lines = [];

            while (Console.ReadLine() is string line)
            {
                lines.Add(line);
            }

            return string.Join(Environment.NewLine, lines);
        }

        private string ReadSecretValue(string message, ConsoleColor foreground = ConsoleColor.Gray)
        {
            _logger.LogMethodCall();

            try
            {
                return ConsoleExtension.ReadValue(message, (message) => ConsoleExtension.Write(message, foreground), () => ConsoleExtension.ReadSecretLine()) ?? throw new ReadValueCanceledException();
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException)
            {
                return ReadValue(message, foreground);
            }
        }

        private bool ReadBoolValue(string message, ConsoleColor foreground = ConsoleColor.Gray)
        {
            _logger.LogMethodCall();

            return !_launchOptions.NonInteractive
                ? ConsoleExtension.ReadBool(message, (message) => ConsoleExtension.Write(message, foreground))
                : throw new InvalidOperationException(_resourceLoader.Strings.GetNonInteractiveOperationNotSupportedError("confirmation prompt"));
        }
    }
}
