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

namespace Eppie.CLI.Exceptions
{
    /// <summary>
    /// The exception that is thrown when reading the standard input stream is canceled.
    /// </summary>
    public class ReadValueCanceledException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadValueCanceledException" /> class with default properties.
        /// </summary>
        public ReadValueCanceledException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadValueCanceledException" /> class with a specific message that describes the current exception.
        /// </summary>
        /// <param name="message">A message that describes the current exception.</param>
        public ReadValueCanceledException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadValueCanceledException" /> class with a specific message that describes the current exception, an inner exception.
        /// </summary>
        /// <param name="message">A message that describes the current exception.</param>
        /// <param name="innerException">The inner exception.</param>
        public ReadValueCanceledException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
