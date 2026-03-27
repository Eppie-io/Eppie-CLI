// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2026 Eppie (https://eppie.io)                                    //
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

using Eppie.CLI.Services;

namespace Eppie.CLI.Exceptions
{
    internal sealed class ApplicationCommandException : Exception
    {
        public ApplicationCommandException()
        {
        }

        public ApplicationCommandException(string? message)
            : base(message)
        {
        }

        public ApplicationCommandException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        internal ApplicationCommandException(ApplicationOutput output, int exitCode, bool logStackTrace = false, Exception? innerException = null)
            : base(message: null, innerException)
        {
            ArgumentNullException.ThrowIfNull(output);

            Output = output;
            ExitCode = exitCode;
            LogStackTrace = logStackTrace;
        }

        internal ApplicationOutput? Output { get; }

        internal int ExitCode { get; }

        internal bool LogStackTrace { get; }
    }
}
