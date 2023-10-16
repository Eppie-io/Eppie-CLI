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
            private string? _logoFormat;
            internal string LogoFormat => _logoFormat ??= _localizer.LoadString(GetStringResourceName(section: "Header"));

            private string? _description;
            internal string Description => _description ??= _localizer.LoadString(GetStringResourceName(section: "Header"));

            private string? _environmentNameFormat;
            internal string EnvironmentNameFormat => _environmentNameFormat ??= _localizer.LoadString(GetStringResourceName(section: "Header"));

            private string? _contentRootPathFormat;
            internal string ContentRootPathFormat => _contentRootPathFormat ??= _localizer.LoadString(GetStringResourceName(section: "Header"));

            private static string GetStringResourceName(string? section = null, [CallerMemberName] string name = "", string? category = "Text")
            {
                return string.Join('.', new string?[] { section, name, category });
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

            internal string Name => ExecutingAssembly.GetName().Name ?? DefaultApplicationTitle;
            internal string Version => ExecutingAssembly.GetName().Version?.ToString() ?? DefaultApplicationVersion;
            internal string Title => ReadAssemblyAttribute<AssemblyTitleAttribute>(ExecutingAssembly)?.Title ?? DefaultApplicationTitle;
            internal string FileVersion => ReadAssemblyAttribute<AssemblyFileVersionAttribute>(ExecutingAssembly)?.Version ?? DefaultApplicationVersion;
            internal string InformationalVersion => ReadAssemblyAttribute<AssemblyInformationalVersionAttribute>(ExecutingAssembly)?.InformationalVersion ?? DefaultApplicationVersion;

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
