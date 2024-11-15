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
using System.Globalization;
using System.Text;

using Microsoft.Extensions.Hosting;

namespace Eppie.CLI.Options
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal class ConsoleOptions : IConfigurationSectionOptions
    {
        public string SectionName => "Console";

        /// <summary>
        /// Gets a configuration value that can be read from the '--Console:CultureInfo' or '/Console:CultureInfo' command-line argument,
        /// or from the '$.Console.CultureInfo' parameter in the 'appsettings.json' or 'appsettings.[<see cref="IHostEnvironment.EnvironmentName"/>].json' file.
        /// </summary>
        public CultureInfo CultureInfo { get; init; } = CultureInfo.CurrentCulture;

        /// <summary>
        /// Gets a configuration value that can be read from the '--Console:Encoding' or '/Console:Encoding' command-line argument,
        /// or from the '$.Console.Encoding' parameter in the 'appsettings.json' or 'appsettings.[<see cref="IHostEnvironment.EnvironmentName"/>].json' file.
        /// </summary>
        public Encoding Encoding => _encoding ??= OptionConverter.ConvertValue(
            value: EncodingName,
            defaultValue: Encoding.Unicode,
            converter: Encoding.GetEncoding,
            ignore: (exception) => exception is ArgumentException or NotSupportedException);

        private string? EncodingName { get; init; }
        private Encoding? _encoding;
    }
}
