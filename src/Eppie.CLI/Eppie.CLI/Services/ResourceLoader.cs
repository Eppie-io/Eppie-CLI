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
using System.Reflection;
using System.Runtime.CompilerServices;

using Eppie.CLI.Exceptions;

using Microsoft.Extensions.Localization;

namespace Eppie.CLI.Services
{
    /// <summary>
    /// Represents a service that provides localized resources.
    /// </summary>
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal sealed class ResourceLoader(IStringLocalizer<Resources.Program> localizer)
    {
        /// <summary>
        /// Gets the resource strings from the 'Program.resx' file.
        /// </summary>
        internal ProgramStringLoader Strings { get; init; } = new ProgramStringLoader(localizer);
        internal AssemblyStringLoader AssemblyStrings { get; init; } = new AssemblyStringLoader();

        /// <summary>
        /// Represents a set of resource strings from the 'Program.resx' file.
        /// </summary>
        internal class ProgramStringLoader
        {
            private readonly IStringLocalizer _localizer;

            internal ProgramStringLoader(IStringLocalizer localizer)
            {
                _localizer = localizer;
            }

            /// <summary>
            /// Gets the startup banner or the copyright message.
            /// </summary>
            internal string GetLogo(string name, string version)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(category: "Header", name: "LogoFormat"), name, version);
            }

            private string? _description;
            internal string Description => _description ??= _localizer.LoadString(GetStringResourceName(category: "Header"));

            private string? _goodbye;
            internal string Goodbye => _goodbye ??= _localizer.LoadString(GetStringResourceName());

            private string? _seedPhraseHeader;
            internal string SeedPhraseHeader => _seedPhraseHeader ??= _localizer.LoadString(GetStringResourceName());

            private string? _seedPhraseFooter;
            internal string SeedPhraseFooter => _seedPhraseFooter ??= _localizer.LoadString(GetStringResourceName());

            private string? _askPassword;
            internal string AskPassword => _askPassword ??= _localizer.LoadString(GetStringResourceName());

            private string? _askNewPassword;
            internal string AskNewPassword => _askNewPassword ??= _localizer.LoadString(GetStringResourceName());

            private string? _confirmPassword;
            internal string ConfirmPassword => _confirmPassword ??= _localizer.LoadString(GetStringResourceName());

            private string? _askAccountAddress;
            internal string AskAccountAddress => _askAccountAddress ??= _localizer.LoadString(GetStringResourceName());

            private string? _askAccountPassword;
            internal string AskAccountPassword => _askAccountPassword ??= _localizer.LoadString(GetStringResourceName());

            private string? _askIMAPServer;
            internal string AskIMAPServer => _askIMAPServer ??= _localizer.LoadString(GetStringResourceName());

            private string? _askSMTPServer;
            internal string AskSMTPServer => _askSMTPServer ??= _localizer.LoadString(GetStringResourceName());

            private string? _headerAccountList;
            internal string HeaderAccountList => _headerAccountList ??= _localizer.LoadString(GetStringResourceName());

            private string? _emptyAccountList;
            internal string EmptyAccountList => _emptyAccountList ??= _localizer.LoadString(GetStringResourceName());

            private string? _emptyContactList;
            internal string EmptyContactList => _emptyContactList ??= _localizer.LoadString(GetStringResourceName());

            private string? _appReset;
            internal string AppReset => _appReset ??= _localizer.LoadString(GetStringResourceName());

            private string? _appOpened;
            internal string AppOpened => _appOpened ??= _localizer.LoadString(GetStringResourceName());

            private string? _appRestored;
            internal string AppRestored => _appRestored ??= _localizer.LoadString(GetStringResourceName());

            private string? _confirmReset;
            internal string ConfirmReset => _confirmReset ??= _localizer.LoadString(GetStringResourceName());

            private string? _askSeedPhrase;
            internal string AskSeedPhrase => _askSeedPhrase ??= _localizer.LoadString(GetStringResourceName());

            private string? _askRestorePath;
            internal string AskRestorePath => _askRestorePath ??= _localizer.LoadString(GetStringResourceName());

            private string? _askMessageBody;
            internal string AskMessageBody => _askMessageBody ??= _localizer.LoadString(GetStringResourceName());

            private string? _invalidPassword;
            internal string InvalidPassword => _invalidPassword ??= _localizer.LoadString(GetStringResourceName(category: "Warning"));

            private string? _secondInitialization;
            internal string SecondInitialization => _secondInitialization ??= _localizer.LoadString(GetStringResourceName(category: "Warning"));

            private string? _uninitialized;
            internal string Uninitialized => _uninitialized ??= _localizer.LoadString(GetStringResourceName(category: "Warning"));

            private string? _impossibleInitialization;
            internal string ImpossibleInitialization => _impossibleInitialization ??= _localizer.LoadString(GetStringResourceName(category: "Error"));

            internal string GetUnknownFolderWarning(string address, string folder)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(category: "Warning", name: "UnknownFolder"), folder, address);
            }

            internal string GetUnhandledException(Exception exception)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(category: "Error", name: "UnhandledException"), exception);
            }

            internal string GetMenuDescription(string name, string version)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(category: "Menu", name: "Description"), name, version);
            }

            private string? _exitDescription;
            internal string ExitDescription => _exitDescription ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            private string? _initDescription;
            internal string InitDescription => _initDescription ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            private string? _openDescription;
            internal string OpenDescription => _openDescription ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            private string? _resetDescription;
            internal string ResetDescription => _resetDescription ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            private string? _addAccountDescription;
            internal string AddAccountDescription => _addAccountDescription ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            private string? _listAccountsDescription;
            internal string ListAccountsDescription => _listAccountsDescription ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            private string? _restoreDescription;
            internal string RestoreDescription => _restoreDescription ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            private string? _sendDescription;
            internal string SendDescription => _sendDescription ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            private string? _importDescription;
            internal string ImportDescription => _importDescription ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            private string? _showMessageDescription;
            internal string ShowMessageDescription => _showMessageDescription ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            private string? _showAllMessagesDescription;
            internal string ShowAllMessagesDescription => _showAllMessagesDescription ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            private string? _showFolderMessagesDescription;
            internal string ShowFolderMessagesDescription => _showFolderMessagesDescription ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            private string? _showContactMessagesDescription;
            internal string ShowContactMessagesDescription => _showContactMessagesDescription ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            private string? _listContactsDescription;
            internal string ListContactsDescription => _listContactsDescription ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            private string? _accountTypeDescription;
            internal string AccountTypeDescription => _accountTypeDescription ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            private string? _senderDescription;
            internal string SenderDescription => _senderDescription ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            private string? _receiverDescription;
            internal string ReceiverDescription => _receiverDescription ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            private string? _subjectDescription;
            internal string SubjectDescription => _subjectDescription ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            private string? _keyBundleFileDescription;
            internal string KeyBundleFileDescription => _keyBundleFileDescription ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            private string? _accountAddressDescription;
            internal string AccountAddressDescription => _accountAddressDescription ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            private string? _contactAddressDescription;
            internal string ContactAddressDescription => _contactAddressDescription ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            private string? _accountFolderDescription;
            internal string AccountFolderDescription => _accountFolderDescription ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            private string? _pageSizeDescription;
            internal string PageSizeDescription => _pageSizeDescription ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            private string? _messageIDDescription;
            internal string MessageIDDescription => _messageIDDescription ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            private string? _messagePKDescription;
            internal string MessagePKDescription => _messagePKDescription ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            private string? _printAllMessagesHeader;
            internal string PrintAllMessagesHeader => _printAllMessagesHeader ??= _localizer.LoadString(GetStringResourceName(category: "Information"));

            private string? _askMoreMessages;
            internal string AskMoreMessages => _askMoreMessages ??= _localizer.LoadString(GetStringResourceName());

            private string? _askMoreContacts;
            internal string AskMoreContacts => _askMoreContacts ??= _localizer.LoadString(GetStringResourceName());

            internal string GetPrintFolderMessagesHeader(string accountAddress, string folder)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(category: "Information", name: "PrintFolderMessagesHeader"), accountAddress, folder);
            }

            internal string GetPrintContactMessagesHeader(string contactAddress)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(category: "Information", name: "PrintContactMessagesHeader"), contactAddress);
            }

            internal string GetMessageDetailsText(uint id, int pk, DateTimeOffset date, string to, string from, string folder, string subject)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(category: "Information", name: "MessageDetails"), id, pk, date, to, from, folder, subject);
            }

            internal string GetMessageIDPropertyText(uint id)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(category: "Information", name: "MessageIDProperty"), id);
            }

            internal string GetMessagePKPropertyText(int pk)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(category: "Information", name: "MessagePKProperty"), pk);
            }

            internal string GetMessageDatePropertyText(DateTimeOffset date)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(category: "Information", name: "MessageDateProperty"), date);
            }

            internal string GetMessageFolderText(string folder)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(category: "Information", name: "MessageFolder"), folder);
            }

            internal string GetMessageFromPropertyText(string from)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(category: "Information", name: "MessageFromProperty"), from);
            }

            internal string GetMessageToPropertyText(string to)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(category: "Information", name: "MessageToProperty"), to);
            }

            internal string GetMessageCcPropertyText(string cc)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(category: "Information", name: "MessageCcProperty"), cc);
            }

            internal string GetMessageBccPropertyText(string bcc)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(category: "Information", name: "MessageBccProperty"), bcc);
            }

            internal string GetMessageSubjectPropertyText(string subject)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(category: "Information", name: "MessageSubjectProperty"), subject);
            }

            internal string GetMessageAttachmentsCountText(int attachments)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(category: "Information", name: "MessageAttachmentsCountProperty"), attachments);
            }

            private static string GetStringResourceName(string category = "Message", [CallerMemberName] string name = "")
            {
                return string.Join('.', new string?[] { category, name });
            }

            internal string GetContactDetailsText(int id, string address, string fullName, int unreadCount)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(category: "Information", name: "ContactDetails"), id, address, fullName, unreadCount);
            }
        }

        /// <summary>
        /// Represents a set of strings from the executable assembly.
        /// </summary>
        internal class AssemblyStringLoader
        {
            private const string DefaultApplicationTitle = "Eppie";
            private const string DefaultApplicationVersion = "1.0.0.0";

            private Assembly ExecutingAssembly { get; }
            internal AssemblyStringLoader()
            {
                ExecutingAssembly = Assembly.GetExecutingAssembly();
            }

            private string? _name;
            internal string Name => _name ??= ExecutingAssembly.GetName().Name ?? DefaultApplicationTitle;

            private string? _version;
            internal string Version => _version ??= ExecutingAssembly.GetName().Version?.ToString() ?? DefaultApplicationVersion;

            private string? _title;
            internal string Title => _title ??= ReadAssemblyAttribute<AssemblyTitleAttribute>(ExecutingAssembly)?.Title ?? DefaultApplicationTitle;

            private string? _fileVersion;
            internal string FileVersion => _fileVersion ??= ReadAssemblyAttribute<AssemblyFileVersionAttribute>(ExecutingAssembly)?.Version ?? DefaultApplicationVersion;

            private string? _informationalVersion;
            internal string InformationalVersion => _informationalVersion ??= ReadAssemblyAttribute<AssemblyInformationalVersionAttribute>(ExecutingAssembly)?.InformationalVersion ?? DefaultApplicationVersion;

            private static TAttribute? ReadAssemblyAttribute<TAttribute>(Assembly assembly)
                where TAttribute : Attribute
            {
                TAttribute? attribute = assembly.GetCustomAttribute<TAttribute>();
#if DEBUG
                AssemblyAttributeMissedException.ThrowIfMissed(attribute, typeof(TAttribute).FullName);
#endif
                return attribute;
            }
        }
    }

    file static class StringLocalizerExtensions
    {
        /// <summary>
        /// Gets the string resource with the given name.
        /// Debug throws the <see cref="ResourceNotFoundException"/> exception when the resource is not found.
        /// </summary>
        /// <param name="localizer">Localized strings provider <see cref="IStringLocalizer"/>.</param>
        /// <param name="name">The name of the string resource.</param>
        /// <returns>The string resource.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ResourceNotFoundException"></exception>
        public static string LoadString(this IStringLocalizer localizer, string name)
        {
            ArgumentNullException.ThrowIfNull(localizer);
            ArgumentNullException.ThrowIfNull(name);
#if DEBUG
            LocalizedString localizedString = localizer[name];
            ResourceNotFoundException.ThrowIfNotFound(localizedString);

            return localizedString.Value;
#else
            return localizer.GetString(name);
#endif
        }

        /// <summary>
        /// Gets the string resource with the given name and formatted with the supplied arguments.
        /// Debug throws the <see cref="ResourceNotFoundException"/> exception when the resource is not found.
        /// </summary>
        /// <param name="localizer">Localized strings provider <see cref="IStringLocalizer"/>.</param>
        /// <param name="name">The name of the string resource.</param>
        /// <param name="arguments">The values to format the string with.</param>
        /// <returns>The formatted string resource.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ResourceNotFoundException"></exception>
        public static string LoadFormattedString(this IStringLocalizer localizer, string name, params object[] arguments)
        {
            ArgumentNullException.ThrowIfNull(localizer);
            ArgumentNullException.ThrowIfNull(name);

#if DEBUG
            LocalizedString localizedString = localizer[name, arguments];
            ResourceNotFoundException.ThrowIfNotFound(localizedString);

            return localizedString.Value;
#else
            return localizer.GetString(name, arguments);
#endif
        }
    }
}
