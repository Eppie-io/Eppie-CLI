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
    internal sealed class ResourceLoader
    {
        /// <summary>
        /// Gets the resource strings from the 'Program.resx' file.
        /// </summary>
        internal ProgramStringLoader Strings { get; init; }
        internal AssemblyStringLoader AssemblyStrings { get; init; }

        public ResourceLoader(IStringLocalizer<Resources.Program> localizer)
        {
            AssemblyStrings = new AssemblyStringLoader();
            Strings = new ProgramStringLoader(localizer);
        }

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

            private string? _appReset;
            internal string AppReset => _appReset ??= _localizer.LoadString(GetStringResourceName());

            private string? _appOpened;
            internal string AppOpened => _appOpened ??= _localizer.LoadString(GetStringResourceName());

            private string? _confirmReset;
            internal string ConfirmReset => _confirmReset ??= _localizer.LoadString(GetStringResourceName());

            private string? _invalidPassword;
            internal string InvalidPassword => _invalidPassword ??= _localizer.LoadString(GetStringResourceName(category: "Warning"));

            private string? _secondInitialization;
            internal string SecondInitialization => _secondInitialization ??= _localizer.LoadString(GetStringResourceName(category: "Warning"));

            private string? _uninitialized;
            internal string Uninitialized => _uninitialized ??= _localizer.LoadString(GetStringResourceName(category: "Warning"));

            private string? _impossibleInitialization;
            internal string ImpossibleInitialization => _impossibleInitialization ??= _localizer.LoadString(GetStringResourceName(category: "Error"));

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

            private static string GetStringResourceName(string category = "Message", [CallerMemberName] string name = "")
            {
                return string.Join('.', new string?[] { category, name });
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
