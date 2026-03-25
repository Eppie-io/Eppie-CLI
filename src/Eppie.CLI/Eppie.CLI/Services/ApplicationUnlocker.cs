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

using Eppie.CLI.Tools;

using Microsoft.Extensions.Logging;

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal sealed class ApplicationUnlocker(
        ILogger<ApplicationUnlocker> logger,
        Application application,
        CoreProvider coreProvider) : IApplicationUnlocker
    {
        private readonly ILogger<ApplicationUnlocker> _logger = logger;
        private readonly Application _application = application;
        private readonly CoreProvider _coreProvider = coreProvider;

        public async Task<bool> UnlockAsync(CancellationToken cancellationToken)
        {
            _logger.LogMethodCall();

            bool isFirstTime = await _coreProvider.TuviMailCore.IsFirstApplicationStartAsync(cancellationToken).ConfigureAwait(false);

            if (isFirstTime)
            {
                _application.WriteUninitializedAppWarning();
                return false;
            }

            bool success = await _coreProvider.TuviMailCore.InitializeApplicationAsync(_application.AskPassword(), cancellationToken).ConfigureAwait(false);

            if (!success)
            {
                _application.WriteInvalidPasswordWarning();
            }

            return success;
        }
    }
}
