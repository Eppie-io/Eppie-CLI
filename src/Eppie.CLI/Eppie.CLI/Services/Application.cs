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

using System.Diagnostics.CodeAnalysis;
using System.Text;

using Eppie.CLI.Common;
using Eppie.CLI.Exceptions;
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
    internal class Application(
       ILogger<Application> logger,
       IHostApplicationLifetime lifetime,
       IOptions<MailOptions> mailOptions,
       ResourceLoader resourceLoader)
    {
        private readonly ResourceLoader _resourceLoader = resourceLoader;
        private readonly ILogger<Application> _logger = logger;
        private readonly IHostApplicationLifetime _lifetime = lifetime;
        private readonly IOptions<MailOptions> _mailOptions = mailOptions;

        internal void StopApplication()
        {
            _logger.LogMethodCall();
            _lifetime.StopApplication();
        }

        internal string AskPassword()
        {
            _logger.LogMethodCall();

            try
            {
                return ReadSecretValue(_resourceLoader.Strings.AskPassword);
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException)
            {
                return ReadValue(_resourceLoader.Strings.AskPassword);
            }
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

        internal string AskTwoFactorCode()
        {
            _logger.LogMethodCall();

            return ReadValue(_resourceLoader.Strings.AskTwoFactorCode);
        }

        internal string AskMailboxPassword()
        {
            _logger.LogMethodCall();

            return ReadSecretValue(_resourceLoader.Strings.AskMailboxPassword);
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

            return ReadSecretValue(_resourceLoader.Strings.AskSeedPhrase);
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

            return ReadBoolValue(_resourceLoader.Strings.ConfirmReset);
        }

        internal string AskMessageBody()
        {
            _logger.LogMethodCall();

            return ReadValue(_resourceLoader.Strings.AskMessageBody);
        }

        internal void PrintAccounts(IReadOnlyCollection<Account> accounts)
        {
            _logger.LogMethodCall();

            if (accounts is null || accounts.Count == 0)
            {
                _logger.LogDebug("Print Account: There are no accounts yet.");
                Console.WriteLine(_resourceLoader.Strings.EmptyAccountList);
                return;
            }

            Console.WriteLine(_resourceLoader.Strings.HeaderAccountList);
            int i = 0;
            foreach (Account account in accounts)
            {
                _logger.LogDebug("Print Account: {Id}", account.Id);
                Console.WriteLine($"{++i}. {account.Email.Address}");
            }

            Console.WriteLine();
        }

        internal void PrintMessage(Message message, bool compact)
        {
            ArgumentNullException.ThrowIfNull(message);

            _logger.LogDebug("Print message: Id - {Id}, Key - {Pk}", message.Id, message.Pk);

            if (compact)
            {
                string to = message.To.FirstOrDefault()?.Address ?? string.Empty;
                string from = message.From.FirstOrDefault()?.Address ?? string.Empty;

                Console.WriteLine(_resourceLoader.Strings.GetMessageDetailsText(message.Id, message.Pk, message.Date, Truncate(to), Truncate(from),
                                                                                message.Folder.FullName, message.Subject));
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(_resourceLoader.Strings.GetMessageIDPropertyText(message.Id));
                Console.WriteLine(_resourceLoader.Strings.GetMessagePKPropertyText(message.Pk));
                Console.WriteLine(_resourceLoader.Strings.GetMessageDatePropertyText(message.Date));
                Console.WriteLine(_resourceLoader.Strings.GetMessageToPropertyText(string.Join(';', message.To)));
                Console.WriteLine(_resourceLoader.Strings.GetMessageFromPropertyText(string.Join(';', message.From)));
                Console.WriteLine(_resourceLoader.Strings.GetMessageCcPropertyText(string.Join(';', message.Cc)));
                Console.WriteLine(_resourceLoader.Strings.GetMessageBccPropertyText(string.Join(';', message.Bcc)));
                Console.WriteLine(_resourceLoader.Strings.GetMessageFolderText(message.Folder.FullName));
                Console.WriteLine(_resourceLoader.Strings.GetMessageSubjectPropertyText(message.Subject));

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(message.TextBody ?? message.HtmlBody ?? string.Empty);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(_resourceLoader.Strings.GetMessageAttachmentsCountText(message.Attachments.Count));

                Console.ResetColor();
            }

            static string Truncate(string s, int maxLength = 19)
            {
                if (s.Length <= maxLength)
                {
                    return s;
                }

                int halfLen = maxLength >> 1;
                StringBuilder sb = new(maxLength);
                sb.Append(s.AsSpan(0, halfLen));
                sb.Append('…');
                sb.Append(s.AsSpan(s.Length - halfLen, halfLen));
                return sb.ToString();
            }
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

        internal void WriteError(Exception ex)
        {
            _logger.LogError("An error has occurred {Exception}", ex);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(_resourceLoader.Strings.GetUnhandledException(ex));
            Console.ResetColor();
        }

        internal void WriteUnknownFolderWarning(string address, string folder)
        {
            LogCommandWarning($"Unknown folder", new { Address = address, Folder = folder });

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(_resourceLoader.Strings.GetUnknownFolderWarning(address, folder));
            Console.ResetColor();
        }

        internal void PrintContacts(int pageSize, IEnumerable<Contact> contacts)
        {
            ArgumentNullException.ThrowIfNull(contacts);

            if (!contacts.Any())
            {
                Console.WriteLine(_resourceLoader.Strings.EmptyContactList);
                return;
            }

            do
            {
                foreach (Contact contact in contacts.Take(pageSize))
                {
                    PrintContact(contact);
                }

                contacts = contacts.Skip(pageSize);
            }
            while (contacts.Any() && ReadBoolValue(_resourceLoader.Strings.AskMoreContacts));
        }

        internal Task PrintAllMessagesAsync(int pageSize, Func<int, Message, Task<IEnumerable<Message>>> messageSource)
        {
            _logger.LogMethodCall();
            return PrintMessagesAsync(_resourceLoader.Strings.PrintAllMessagesHeader, pageSize, messageSource);
        }

        internal Task PrintFolderMessagesAsync(string accountAddress, string folderName, int pageSize, Func<int, Message, Task<IEnumerable<Message>>> messageSource)
        {
            _logger.LogMethodCall();
            return PrintMessagesAsync(_resourceLoader.Strings.GetPrintFolderMessagesHeader(accountAddress, folderName), pageSize, messageSource);
        }

        internal Task PrintContactMessagesAsync(string contactAddress, int pageSize, Func<int, Message, Task<IEnumerable<Message>>> messageSource)
        {
            _logger.LogMethodCall();
            return PrintMessagesAsync(_resourceLoader.Strings.GetPrintContactMessagesHeader(contactAddress), pageSize, messageSource);
        }

        internal string ReadValue(string message, ConsoleColor foreground = ConsoleColor.Gray)
        {
            _logger.LogMethodCall();
            return ConsoleExtension.ReadValue(message, (message) => ConsoleExtension.Write(message, foreground), Console.ReadLine) ?? throw new ReadValueCanceledException();
        }

        private string ReadSecretValue(string message, ConsoleColor foreground = ConsoleColor.Gray)
        {
            _logger.LogMethodCall();
            return ConsoleExtension.ReadValue(message, (message) => ConsoleExtension.Write(message, foreground), () => ConsoleExtension.ReadSecretLine()) ?? throw new ReadValueCanceledException();
        }

        private bool ReadBoolValue(string message, ConsoleColor foreground = ConsoleColor.Gray)
        {
            _logger.LogMethodCall();
            return ConsoleExtension.ReadBool(message, (message) => ConsoleExtension.Write(message, foreground));
        }

        private async Task PrintMessagesAsync(string header, int pageSize, Func<int, Message, Task<IEnumerable<Message>>> source)
        {
            ArgumentNullException.ThrowIfNull(source);

            Console.WriteLine(header);

            Message lastItem = null!;

            while (true)
            {
                IEnumerable<Message> items = await source(pageSize, lastItem).ConfigureAwait(false);
                int count = 0;
                foreach (Message item in items)
                {
                    if (item != null)
                    {
                        PrintMessage(item, true);
                        lastItem = item;
                        ++count;
                    }
                }

                if (count < pageSize || !ReadBoolValue(_resourceLoader.Strings.AskMoreMessages))
                {
                    break;
                }
            }
        }

        private void PrintContact(Contact contact)
        {
            ArgumentNullException.ThrowIfNull(contact);

            _logger.LogDebug("Print contact: {Id}", contact.Id);

            Console.WriteLine(_resourceLoader.Strings.GetContactDetailsText(contact.Id, contact.Email.Address, contact.FullName, contact.UnreadCount));
        }

        private void LogCommandWarning(string reason)
        {
            _logger.LogWarning("The command failed. (Reason: {WarningReason}).", reason);
        }

        private void LogCommandWarning(string reason, object parameters)
        {
            _logger.LogWarning("The command failed. (Reason: {WarningReason}; Parameters: {@Parameters}).", reason, parameters);
        }
    }
}
