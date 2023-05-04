// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2023 Eppie(https://eppie.io)                                     //
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

using Eppie.CLI.Exceptions;
using Microsoft.Extensions.Localization;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

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
            Strings = new ProgramStringLoader(localizer, AssemblyStrings);
        }

        /// <summary>
        /// Represents a set of resource strings from the 'Program.resx' file.
        /// </summary>
        internal class ProgramStringLoader
        {
            private readonly IStringLocalizer _localizer;
            private readonly AssemblyStringLoader _assemblyStringLoader;

            internal ProgramStringLoader(IStringLocalizer localizer, AssemblyStringLoader assemblyStringLoader)
            {
                _localizer = localizer;
                _assemblyStringLoader = assemblyStringLoader;
            }

            /// <summary>
            /// Gets the startup banner or the copyright message.
            /// </summary>
            internal string LogoMessage
            {
                get
                {
                    return _localizer.LoadFormattedString("LogoMessage.FormattedText", _assemblyStringLoader.Title,
                                                                                       _assemblyStringLoader.Version);
                }
            }
        }

        /// <summary>
        /// Represents a set of strings from the executable assembly.
        /// </summary>
        internal class AssemblyStringLoader
        {
            private readonly Assembly _assembly;
            internal AssemblyStringLoader()
            {
                _assembly = Assembly.GetExecutingAssembly();
            }

            internal string Name => ReadAssemblyValue(GetExecutingAssembly().GetName().Name, "Eppie");
            internal string Version => ReadAssemblyValue(GetExecutingAssembly().GetName().Version, "1.0");
            internal string Title => ReadAssemblyValue(GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>()?.Title, "Eppie");

            private Assembly GetExecutingAssembly()
            {
                return _assembly;
            }

            private static string ReadAssemblyValue<T>(T? attribute, string defaultValue, [CallerArgumentExpression(nameof(attribute))] string? attributeName = null)
            {
#if DEBUG
                AssemblyAttributeMissedException.ThrowIfMissed(attribute, attributeName);
#endif
                return attribute?.ToString() ?? defaultValue;
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
