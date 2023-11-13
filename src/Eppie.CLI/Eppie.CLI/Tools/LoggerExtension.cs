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

using System.Diagnostics;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;

namespace Eppie.CLI.Tools
{
    internal static class LoggerExtension
    {
        [Conditional("TRACE")]
        public static void LogMethodCall(this ILogger logger,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            logger.LogTrace("Method {MemberName} has been called; Source file path: {SourceFilePath}; Source line number: {SourceLineNumber}", memberName, sourceFilePath, sourceLineNumber);
        }

        public static IDisposable? BeginConsoleScope(this ILogger logger)
        {
            Debug.Assert(logger is not null);
            return logger.BeginScope(new Dictionary<string, object> { { "log-scope", "console" } });
        }
    }
}
