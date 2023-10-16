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
using System.Globalization;
using System.Numerics;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Eppie.CLI.Services
{
    /// <summary>
    ///  Represents a service that provides configuration properties.
    /// </summary>
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal sealed class ProgramConfiguration
    {
        private IConfiguration Configuration { get; init; }

        public ProgramConfiguration(IConfiguration config)
        {
            Configuration = config;
        }

        /// <summary>
        /// Gets a configuration value that can be read from the '--Console:CultureInfo' or '/Console:CultureInfo' command-line argument,
        /// or from the '$.Console.CultureInfo' parameter in the 'appsettings.json' or 'appsettings.[<see cref="IHostEnvironment.EnvironmentName"/>].json' file.
        /// </summary>
        internal CultureInfo ConsoleCultureInfo => ReadValue(
            key: "Console:CultureInfo",
            defaultValue: CultureInfo.CurrentCulture,
            converter: CultureInfo.GetCultureInfo,
            ignore: (exception) => exception is CultureNotFoundException);

        /// <summary>
        /// Gets a configuration value that can be read from the '--Console:Encoding' or '/Console:Encoding' command-line argument,
        /// or from the '$.Console.Encoding' parameter in the 'appsettings.json' or 'appsettings.[<see cref="IHostEnvironment.EnvironmentName"/>].json' file.
        /// </summary>
        internal Encoding ConsoleEncoding => ReadValue(
            key: "Console:Encoding",
            defaultValue: Encoding.Unicode,
            converter: Encoding.GetEncoding,
            ignore: (exception) => exception is ArgumentException or NotSupportedException);

        private string ReadStringValue(string key, string defaultValue)
        {
            return TryReadStringValue(key, out string value) ? value : defaultValue;
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Reserved for future use")]
        private string ReadStringValue(string key)
        {
            return ReadStringValue(key, string.Empty);
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Reserved for future use")]
        private bool ReadBooleanValue(string key, bool defaultValue = false)
        {
            return ReadValue(
                key: key,
                defaultValue: defaultValue,
                converter: (param) => bool.TryParse(param, out bool value) ? value : defaultValue);
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Reserved for future use")]
        private T ReadNumberValue<T>(string key, T defaultValue)
            where T : INumber<T>
        {
            return ReadValue(
                key: key,
                defaultValue: defaultValue,
                converter: (param) => T.TryParse(param, ConsoleCultureInfo, out T? value) ? value : defaultValue);
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Reserved for future use")]
        private T ReadEnumValue<T>(string key, T defaultValue, bool ignoreCase = false)
            where T : struct, Enum
        {
            return ReadValue(
                key: key,
                defaultValue: defaultValue,
                converter: (param) => Enum.TryParse(param, ignoreCase, out T value) ? value : defaultValue);
        }

        private T ReadValue<T>(string key, T defaultValue, Func<string, T> converter, Func<Exception, bool>? ignore = null)
        {
            ArgumentNullException.ThrowIfNull(converter, nameof(converter));

            try
            {
                return TryReadStringValue(key, out string value) ? converter(value) : defaultValue;
            }
            catch (Exception ex) when (ignore?.Invoke(ex) is true)
            {
                return defaultValue;
            }
        }

        private bool TryReadStringValue(string key, out string value)
        {
            value = Configuration[key] ?? string.Empty;
            return !string.IsNullOrEmpty(value);
        }
    }
}
