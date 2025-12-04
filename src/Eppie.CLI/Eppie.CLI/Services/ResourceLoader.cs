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

            internal string Description => field ??= _localizer.LoadString(GetStringResourceName(category: "Header"));

            internal string Goodbye => field ??= _localizer.LoadString(GetStringResourceName());

            internal string SeedPhraseHeader => field ??= _localizer.LoadString(GetStringResourceName());

            internal string SeedPhraseFooter => field ??= _localizer.LoadString(GetStringResourceName());

            internal string AskPassword => field ??= _localizer.LoadString(GetStringResourceName());

            internal string AskNewPassword => field ??= _localizer.LoadString(GetStringResourceName());

            internal string ConfirmPassword => field ??= _localizer.LoadString(GetStringResourceName());

            internal string AskAccountAddress => field ??= _localizer.LoadString(GetStringResourceName());

            internal string AskAccountPassword => field ??= _localizer.LoadString(GetStringResourceName());

            internal string AskTwoFactorCode => field ??= _localizer.LoadString(GetStringResourceName());

            internal string AskMailboxPassword => field ??= _localizer.LoadString(GetStringResourceName());

            private string GetServerAddressQuestionText(string resourceName, string? defaultServer)
            {
                string defaultServerTemplate = string.Empty;

                if (!string.IsNullOrEmpty(defaultServer))
                {
                    defaultServerTemplate = _localizer.LoadFormattedString(GetStringResourceName(category: "Message", name: "DefaultServerTemplate"), defaultServer);
                }

                return _localizer.LoadFormattedString(GetStringResourceName(category: "Message", name: resourceName), defaultServerTemplate);
            }

            internal string GetSMTPServerQuestionText(string? defaultServer)
            {
                return GetServerAddressQuestionText("AskSMTPServer", defaultServer);
            }

            internal string GetIMAPServerQuestionText(string defaultServer)
            {
                return GetServerAddressQuestionText("AskIMAPServer", defaultServer);
            }

            internal string GetSMTPPortQuestionText(int defaultPort)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(category: "Message", name: "AskSMTPPort"), defaultPort);
            }

            internal string GetIMAPPortQuestionText(int defaultPort)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(category: "Message", name: "AskIMAPPort"), defaultPort);
            }

            internal string HeaderAccountList => field ??= _localizer.LoadString(GetStringResourceName());

            internal string EmptyAccountList => field ??= _localizer.LoadString(GetStringResourceName());

            internal string EmptyContactList => field ??= _localizer.LoadString(GetStringResourceName());

            internal string AppReset => field ??= _localizer.LoadString(GetStringResourceName());

            internal string AppOpened => field ??= _localizer.LoadString(GetStringResourceName());

            internal string AppRestored => field ??= _localizer.LoadString(GetStringResourceName());

            internal string ConfirmReset => field ??= _localizer.LoadString(GetStringResourceName());

            internal string AskSeedPhrase => field ??= _localizer.LoadString(GetStringResourceName());

            internal string AskRestorePath => field ??= _localizer.LoadString(GetStringResourceName());

            internal string AskMessageBody => field ??= _localizer.LoadString(GetStringResourceName());

            internal string AuthorizationCanceled => field ??= _localizer.LoadString(GetStringResourceName());

            internal string AuthorizationCompleted => field ??= _localizer.LoadString(GetStringResourceName());

            internal string InvalidPassword => field ??= _localizer.LoadString(GetStringResourceName(category: "Warning"));

            internal string SecondInitialization => field ??= _localizer.LoadString(GetStringResourceName(category: "Warning"));

            internal string Uninitialized => field ??= _localizer.LoadString(GetStringResourceName(category: "Warning"));

            internal string UnsuccessfulAttempt => field ??= _localizer.LoadString(GetStringResourceName(category: "Warning"));

            internal string ImpossibleInitialization => field ??= _localizer.LoadString(GetStringResourceName(category: "Error"));

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

            internal string ExitDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            internal string InitDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            internal string OpenDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            internal string ResetDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            internal string AddAccountDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            internal string ListAccountsDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            internal string RestoreDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            internal string SendDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            internal string ImportDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            internal string ShowMessageDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            internal string SyncFolderDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            internal string ShowAllMessagesDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            internal string ShowFolderMessagesDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            internal string ShowContactMessagesDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            internal string ListContactsDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "Menu"));

            internal string AccountTypeDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            internal string SenderDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            internal string ReceiverDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            internal string SubjectDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            internal string KeyBundleFileDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            internal string AccountAddressDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            internal string ContactAddressDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            internal string AccountFolderDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            internal string PageSizeDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            internal string MessageIDDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            internal string MessagePKDescription => field ??= _localizer.LoadString(GetStringResourceName(category: "MenuOption"));

            internal string PrintAllMessagesHeader => field ??= _localizer.LoadString(GetStringResourceName(category: "Information"));

            internal string AskMoreMessages => field ??= _localizer.LoadString(GetStringResourceName());

            internal string AskMoreContacts => field ??= _localizer.LoadString(GetStringResourceName());

            internal string SelectOptionHeader => field ??= _localizer.LoadString(GetStringResourceName());

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

            internal string GetContactDetailsText(int id, string address, string fullName, int unreadCount)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(category: "Information", name: "ContactDetails"), id, address, fullName, unreadCount);
            }

            internal string GetAskOptionText(string defaultOption)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(name: "AskOption"), defaultOption);
            }

            internal string GetAuthorizationToServiceText(string serviceName)
            {
                return _localizer.LoadFormattedString(GetStringResourceName(name: "AuthorizationToService"), serviceName);
            }

            private static string GetStringResourceName(string category = "Message", [CallerMemberName] string name = "")
            {
                return string.Join('.', [category, name]);
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

            internal string Name => field ??= ExecutingAssembly.GetName().Name ?? DefaultApplicationTitle;

            internal string Version => field ??= ExecutingAssembly.GetName().Version?.ToString() ?? DefaultApplicationVersion;

            internal string Title => field ??= ReadAssemblyAttribute<AssemblyTitleAttribute>(ExecutingAssembly)?.Title ?? DefaultApplicationTitle;

            internal string FileVersion => field ??= ReadAssemblyAttribute<AssemblyFileVersionAttribute>(ExecutingAssembly)?.Version ?? DefaultApplicationVersion;

            internal string InformationalVersion => field ??= ReadAssemblyAttribute<AssemblyInformationalVersionAttribute>(ExecutingAssembly)?.InformationalVersion ?? DefaultApplicationVersion;

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
