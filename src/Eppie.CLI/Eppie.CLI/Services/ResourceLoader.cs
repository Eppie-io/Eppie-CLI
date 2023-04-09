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
        internal ProgramStrings Strings { get; init; }

        public ResourceLoader(IStringLocalizer<Resources.Program> localizer)
        {
            Strings = new ProgramStrings(localizer);
        }

        /// <summary>
        /// Represents a set of resource strings from the 'Program.resx' file.
        /// </summary>
        internal record ProgramStrings
        {
            private IStringLocalizer Localizer { get; init; }
            internal ProgramStrings(IStringLocalizer localizer)
            {
                Localizer = localizer;
            }

            /// <summary>
            /// Gets the startup banner or the copyright message
            /// </summary>
            internal string LogoText => GetString("Logo.Text");

            private string GetString(string name)
            {
                return ResourceLoader.GetString(Localizer, name);
            }
        }

        /// <summary>
        /// Gets the string resource with the given name.
        /// Throws the <see cref="ResourceNotFoundException"/> exception when the resource is not found.
        /// </summary>
        /// <param name="localizer">Localized strings provider.</param>
        /// <param name="name">The name of the string resource.</param>
        /// <returns>The string resource.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ResourceNotFoundException"></exception>
        private static string GetString(IStringLocalizer localizer, string name)
        {
            ArgumentNullException.ThrowIfNull(localizer);
            ArgumentNullException.ThrowIfNull(name);

            LocalizedString stringLocalizer = localizer[name];

            ResourceNotFoundException.ThrowIfResourceNotFound(stringLocalizer, nameof(name));

            return stringLocalizer.Value;
        }

        /// <summary>
        /// Gets the string resource with the given name and formatted with the supplied arguments.
        /// Throws the <see cref="ResourceNotFoundException"/> exception when the resource is not found.
        /// </summary>
        /// <param name="localizer">Localized strings provider.</param>
        /// <param name="name">The name of the string resource.</param>
        /// <param name="arguments">The values to format the string with.</param>
        /// <returns>The formatted string resource.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ResourceNotFoundException"></exception>
        private static string GetFormattedString(IStringLocalizer localizer, string name, params object[] arguments)
        {
            ArgumentNullException.ThrowIfNull(localizer);
            ArgumentNullException.ThrowIfNull(name);

            LocalizedString stringLocalizer = localizer[name, arguments];

            ResourceNotFoundException.ThrowIfResourceNotFound(stringLocalizer, nameof(name));

            return stringLocalizer.Value;
        }
    }
}
