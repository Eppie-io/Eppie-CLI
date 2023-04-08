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

using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal sealed class ProgramConfiguration
    {
        private IConfiguration Configuration { get; init; }

        public ProgramConfiguration(IConfiguration config)
        {
            Configuration = config;
        }

        internal CultureInfo ConsoleCultureInfo => ReadValue(
            key: "Console:CultureInfo",
            defaultValue: CultureInfo.CurrentCulture,
            converter: CultureInfo.GetCultureInfo,
            ignore: (exception) => exception is CultureNotFoundException);

        internal Encoding ConsoleEncoding => ReadValue(
            key: "Console:Encoding",
            defaultValue: Encoding.Unicode,
            converter: Encoding.GetEncoding,
            ignore: (exception) => exception is ArgumentException || exception is NotSupportedException);

        private string ReadStringValue(string key, string defaultValue)
        {
            return TryReadStringValue(key, out var value) ? value : defaultValue;
        }

        private string ReadStringValue(string key)
        {
            return ReadStringValue(key, string.Empty);
        }

        private bool ReadBooleanValue(string key, bool defaultValue = false)
        {
            return ReadValue(
                key: key,
                defaultValue: defaultValue,
                converter: (param) => bool.TryParse(param, out var value) ? value : defaultValue);
        }

        private T ReadNumberValue<T>(string key, T defaultValue)
            where T : INumber<T>
        {
            return ReadValue(
                key: key,
                defaultValue: defaultValue,
                converter: (param) => T.TryParse(param, ConsoleCultureInfo, out T? value) ? value : defaultValue);
        }

        private T ReadEnumValue<T>(string key, T defaultValue, bool ignoreCase = false)
            where T : struct
        {
            return ReadValue(
                key: key,
                defaultValue: defaultValue,
                converter: (param) => Enum.TryParse(param, ignoreCase, out T value) ? value : defaultValue);
        }

        private T ReadValue<T>(string key, T defaultValue, Func<string, T> converter, Func<Exception, bool>? ignore = null)
        {
            try
            {
                return TryReadStringValue(key, out var value) ? converter(value) : defaultValue;
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
