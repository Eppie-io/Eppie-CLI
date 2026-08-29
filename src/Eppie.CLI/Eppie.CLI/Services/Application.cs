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
       IOptions<ApplicationLaunchOptions> launchOptions,
       IApplicationOutputWriter outputWriter,
       IOptions<MailOptions> mailOptions,
       ResourceLoader resourceLoader) : IApplicationPasswordReader
    {
        private const string MessageBodyTerminator = "EOF";

        private readonly ResourceLoader _resourceLoader = resourceLoader;
        private readonly ILogger<Application> _logger = logger;
        private readonly IHostApplicationLifetime _lifetime = lifetime;
        private readonly ApplicationLaunchOptions _launchOptions = launchOptions.Value;
        private readonly IApplicationOutputWriter _outputWriter = outputWriter;
        private readonly IOptions<MailOptions> _mailOptions = mailOptions;

        internal bool CanAskForNewValues => IsPromptVisible && !Console.IsInputRedirected;

        private bool IsPromptVisible => !_launchOptions.NonInteractive;

        private void EnsureInteractiveInputIsAvailable()
        {
            if (!_launchOptions.NonInteractive && _outputWriter.Format == ApplicationOutputFormat.Json)
            {
                throw new ApplicationCommandException(new InteractiveInputNotSupportedErrorOutput());
            }
        }

        private string ReadNamedValue(string message, string inputName, ConsoleColor foreground = ConsoleColor.Gray)
        {
            return IsPromptVisible ? ReadValue(message, foreground) : ReadAnnouncedValue(message, inputName, foreground);
        }

        private string ReadNamedSecret(string message, string inputName, ConsoleColor foreground = ConsoleColor.Gray)
        {
            return IsPromptVisible ? ReadSecretValue(message, foreground) : ReadAnnouncedValue(message, inputName, foreground);
        }

        private string ReadAnnouncedValue(string message, string inputName, ConsoleColor foreground)
        {
            if (_outputWriter.Format == ApplicationOutputFormat.Json)
            {
                _outputWriter.Write(new InputRequiredOutput(inputName));
            }

            return ReadValue(message, writePrompt: false, foreground);
        }

        internal void StopApplication()
        {
            _logger.LogMethodCall();
            _lifetime.StopApplication();
        }

        internal string AskPassword()
        {
            _logger.LogMethodCall();

            return ReadNamedSecret(_resourceLoader.Strings.AskPassword, InputName.VaultPassword);
        }

        internal string ReadPasswordFromStandardInput()
        {
            _logger.LogMethodCall();

            return ReadNamedValue(_resourceLoader.Strings.AskPassword, InputName.VaultPassword);
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

            return ReadNamedSecret(_resourceLoader.Strings.AskNewPassword, InputName.NewVaultPassword);
        }

        internal string ConfirmPassword()
        {
            _logger.LogMethodCall();

            return ReadNamedSecret(_resourceLoader.Strings.ConfirmPassword, InputName.VaultPasswordConfirmation);
        }

        internal string AskAccountAddress()
        {
            _logger.LogMethodCall();

            return ReadNamedValue(_resourceLoader.Strings.AskAccountAddress, InputName.AccountAddress);
        }

        internal string AskAccountPassword()
        {
            _logger.LogMethodCall();

            return ReadNamedSecret(_resourceLoader.Strings.AskAccountPassword, InputName.AccountPassword);
        }

        internal string AskTwoFactorCode(bool firstAttempt)
        {
            _logger.LogMethodCall();

            if (!firstAttempt)
            {
                _outputWriter.Write(new UnsuccessfulAttemptWarningOutput());
            }

            return ReadNamedValue(_resourceLoader.Strings.AskTwoFactorCode, InputName.TwoFactorCode);
        }

        internal string AskMailboxPassword(bool firstAttempt)
        {
            _logger.LogMethodCall();

            if (!firstAttempt)
            {
                _outputWriter.Write(new UnsuccessfulAttemptWarningOutput());
            }

            return ReadNamedSecret(_resourceLoader.Strings.AskMailboxPassword, InputName.MailboxPassword);
        }

        internal string AskHumanVerificationToken()
        {
            _logger.LogMethodCall();

            return ReadNamedValue(_resourceLoader.Strings.AskHumanVerificationToken, InputName.HumanVerificationToken);
        }

        internal string AskIMAPServer(MailServer mailServer)
        {
            _logger.LogMethodCall();

            MailServerConfiguration config = GetMailServerConfiguration(mailServer);
            return AskQuestionWithDefault(_resourceLoader.Strings.GetIMAPServerQuestionText(config.IMAP), InputName.ImapServer, config.IMAP);
        }

        internal string AskSMTPServer(MailServer mailServer)
        {
            _logger.LogMethodCall();

            MailServerConfiguration config = GetMailServerConfiguration(mailServer);
            return AskQuestionWithDefault(_resourceLoader.Strings.GetSMTPServerQuestionText(config.SMTP), InputName.SmtpServer, config.SMTP);
        }

        internal int AskIMAPServerPort(MailServer mailServer)
        {
            _logger.LogMethodCall();

            MailServerConfiguration config = GetMailServerConfiguration(mailServer);
            return AskQuestionWithDefault(_resourceLoader.Strings.GetIMAPPortQuestionText(config.IMAPPort), InputName.ImapPort, config.IMAPPort);
        }

        internal int AskSMTPServerPort(MailServer mailServer)
        {
            _logger.LogMethodCall();

            MailServerConfiguration config = GetMailServerConfiguration(mailServer);
            return AskQuestionWithDefault(_resourceLoader.Strings.GetSMTPPortQuestionText(config.SMTPPort), InputName.SmtpPort, config.SMTPPort);
        }

        internal int AskQuestionWithDefault(string text, string inputName, int defaultValue)
        {
            _logger.LogMethodCall();

            return int.TryParse(ReadNamedValue(text, inputName), out int port) && port > 0
                ? port
                : defaultValue;
        }

        internal string AskQuestionWithDefault(string text, string inputName, string defaultValue)
        {
            _logger.LogMethodCall();

            string answer = ReadNamedValue(text, inputName);
            return string.IsNullOrEmpty(answer) ? defaultValue : answer;
        }

        private MailServerConfiguration GetMailServerConfiguration(MailServer mailServer)
        {
            return _mailOptions.Value.Servers.GetValueOrDefault(mailServer) ?? new MailServerConfiguration();
        }

        internal string AskSeedPhrase()
        {
            _logger.LogMethodCall();

            return ReadNamedSecret(_resourceLoader.Strings.AskSeedPhrase, InputName.SeedPhrase);
        }

        internal string AskRestorePath()
        {
            _logger.LogMethodCall();

            return ReadNamedValue(_resourceLoader.Strings.AskRestorePath, InputName.RestorePath);
        }

        internal TEnum SelectOption<TEnum>(TEnum defaultOption, bool ignoreCase = false)
            where TEnum : struct, Enum
        {
            _logger.LogMethodCall();

            if (_launchOptions.NonInteractive)
            {
                throw new ApplicationCommandException(new NonInteractiveOperationNotSupportedErrorOutput("option selection"));
            }

            EnsureInteractiveInputIsAvailable();

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

            if (_launchOptions.NonInteractive)
            {
                return ReadRemainingStandardInput();
            }

            EnsureInteractiveInputIsAvailable();

            return ConsoleExtension.ReadMultiLine(_resourceLoader.Strings.AskMessageBody, MessageBodyTerminator) ?? throw new ReadValueCanceledException();
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
            return ReadValue(message, writePrompt: IsPromptVisible, foreground);
        }

        internal string ReadValue(string message, bool writePrompt, ConsoleColor foreground = ConsoleColor.Gray)
        {
            _logger.LogMethodCall();

            if (writePrompt)
            {
                EnsureInteractiveInputIsAvailable();
            }

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

            EnsureInteractiveInputIsAvailable();

            try
            {
                return ConsoleExtension.ReadValue(message,
                                                  (message) => ConsoleExtension.Write(message, foreground),
                                                  () => ConsoleExtension.ReadSecretLine()) ?? throw new InputCanceledByUserException();
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException)
            {
                return ReadValue(message, foreground);
            }
        }

        private bool ReadBoolValue(string message, ConsoleColor foreground = ConsoleColor.Gray)
        {
            _logger.LogMethodCall();

            if (_launchOptions.NonInteractive)
            {
                throw new ApplicationCommandException(new NonInteractiveOperationNotSupportedErrorOutput("confirmation prompt"));
            }

            EnsureInteractiveInputIsAvailable();

            return ConsoleExtension.ReadBool(message, (message) => ConsoleExtension.Write(message, foreground));
        }
    }
}
