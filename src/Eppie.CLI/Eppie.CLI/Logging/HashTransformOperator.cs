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

using System.Globalization;

using Serilog.Enrichers.Sensitive;

namespace Eppie.CLI.Logging
{
    /// <summary>
    /// A generic masking operator that wraps existing masking operators and transforms
    /// detected sensitive data into hash-based identifiers.
    /// </summary>
    /// <typeparam name="TBaseMaskingOperator">The base masking operator to wrap.</typeparam>
    public class HashTransformOperator<TBaseMaskingOperator> : IMaskingOperator
        where TBaseMaskingOperator : IMaskingOperator, new()
    {
        private TBaseMaskingOperator BaseOperator { get; } = new TBaseMaskingOperator();

        /// <summary>
        /// Masks the input string by detecting sensitive patterns and replacing them with hash-based identifiers.
        /// </summary>
        /// <param name="input">The input string to mask.</param>
        /// <param name="mask">The mask parameter (not used in this implementation).</param>
        /// <returns>A masking result containing the transformed string if a match was found.</returns>
        public MaskingResult Mask(string input, string mask)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            MaskingResult result = BaseOperator.Mask(input, mask);

            if (result.Match)
            {
                result.Result = string.Concat("#", input.GetHashCode(StringComparison.Ordinal).ToString(CultureInfo.InvariantCulture));
            }

            return result;
        }
    }
}
