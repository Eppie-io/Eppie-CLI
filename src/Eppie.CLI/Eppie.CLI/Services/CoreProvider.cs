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

using ComponentBuilder;

using Eppie.CLI.Tools;

using Microsoft.Extensions.Logging;

using Tuvi.Core;

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal class CoreProvider(ILogger<CoreProvider> logger,
                                ITokenRefresher tokenRefresher)
    {
        private readonly ILogger<CoreProvider> _logger = logger;
        private readonly ITokenRefresher _tokenRefresher = tokenRefresher;

        private ITuviMail? _tuviMailCore;
        public ITuviMail TuviMailCore => _tuviMailCore ??= CreateTuviMail();

        public async Task ResetAsync()
        {
            _logger.LogMethodCall();

            await TuviMailCore.ResetApplicationAsync().ConfigureAwait(false);

            if (_tuviMailCore is not null)
            {
                _tuviMailCore.Dispose();
                _tuviMailCore = null;
            }
        }

        private ITuviMail CreateTuviMail()
        {
            _logger.LogMethodCall();

            return Components.CreateTuviMailCore("data.db", new ImplementationDetailsProvider("Eppie seed", "Eppie.Package", "backup@system.service.eppie.io"), _tokenRefresher);
        }
    }
}
