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

namespace Eppie.CLI.Options
{
    internal static class OptionConverter
    {
        public static T ConvertEnumValue<T>(string value, T defaultValue, bool ignoreCase = false)
            where T : struct, Enum
        {
            return Enum.TryParse(value, ignoreCase, out T result) ? result : defaultValue;
        }

        public static T ConvertValue<T>(string? value, T defaultValue, Func<string, T?> converter, Func<Exception, bool>? ignore = null)
        {
            ArgumentNullException.ThrowIfNull(converter, nameof(converter));

            try
            {
                return converter(value ?? string.Empty) ?? defaultValue;
            }
            catch (Exception ex) when (ignore?.Invoke(ex) is true)
            {
                return defaultValue;
            }
        }
    }
}
