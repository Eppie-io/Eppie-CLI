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

using System.Diagnostics.CodeAnalysis;

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal sealed class ApplicationPagingPolicy(ApplicationLaunchOptions launchOptions) : IApplicationPagingPolicy
    {
        private readonly ApplicationLaunchOptions _launchOptions = launchOptions;

        public bool ShouldAggregatePagesBeforeWrite => _launchOptions.OutputFormat == ApplicationOutputFormat.Json;

        public bool ShouldContinue(bool hasMore, Func<bool> askMore)
        {
            ArgumentNullException.ThrowIfNull(askMore);

            return hasMore && (ShouldAggregatePagesBeforeWrite || (!_launchOptions.NonInteractive && askMore()));
        }
    }
}
