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
        IApplicationOutputWriter outputWriter,
        CoreProvider coreProvider) : IApplicationUnlocker
    {
        private readonly ILogger<ApplicationUnlocker> _logger = logger;
        private readonly Application _application = application;
        private readonly IApplicationOutputWriter _outputWriter = outputWriter;
        private readonly CoreProvider _coreProvider = coreProvider;

        public async Task<bool> UnlockAsync(CancellationToken cancellationToken, bool readPasswordFromStandardInput = false)
        {
            _logger.LogMethodCall();

            bool isFirstTime = await _coreProvider.TuviMailCore.IsFirstApplicationStartAsync(cancellationToken).ConfigureAwait(false);

            if (isFirstTime)
            {
                _logger.LogWarning("The command failed. (Reason: The application hasn't been initialized yet.).");
                _outputWriter.Write(new UninitializedAppWarningOutput());
                return false;
            }

            string password = readPasswordFromStandardInput
                ? _application.ReadPasswordFromStandardInput()
                : _application.AskPassword();

            bool success = await _coreProvider.TuviMailCore.InitializeApplicationAsync(password, cancellationToken).ConfigureAwait(false);

            if (!success)
            {
                _logger.LogWarning("The command failed. (Reason: Invalid Password).");
                _outputWriter.Write(new InvalidPasswordWarningOutput());
            }

            return success;
        }
    }
}
