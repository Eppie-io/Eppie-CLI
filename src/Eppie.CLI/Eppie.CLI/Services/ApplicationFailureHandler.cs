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

using Eppie.CLI.Exceptions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal sealed class ApplicationFailureHandler(
        ILogger<ApplicationFailureHandler> logger,
        IOptions<ApplicationLaunchOptions> launchOptions,
        IApplicationOutputWriter outputWriter) : IApplicationFailureHandler
    {
        private readonly ILogger<ApplicationFailureHandler> _logger = logger;
        private readonly ApplicationLaunchOptions _launchOptions = launchOptions.Value;
        private readonly IApplicationOutputWriter _outputWriter = outputWriter;

        public void HandleControlledCommandFailure(ApplicationCommandException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentNullException.ThrowIfNull(exception.Output);

            if (exception.LogStackTrace && !IsAutomationOutput())
            {
                _logger.LogError(exception, "Command failed with a controlled exception.");
            }
            else
            {
                _logger.LogDebug("Command failed with controlled output {OutputType} and exit code {ExitCode}", exception.Output.GetType().Name, exception.ExitCode);
            }

            if (_launchOptions.NonInteractive)
            {
                Environment.ExitCode = exception.ExitCode;
            }

            _outputWriter.Write(exception.Output);
        }

        public void HandleUnhandledException(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            if (IsAutomationOutput())
            {
                _logger.LogDebug("Command failed with exception type {ExceptionType}: {ExceptionMessage}", exception.GetType().FullName, exception.Message);
            }
            else
            {
                _logger.LogError("An error has occurred {Exception}", exception);
            }

            _outputWriter.Write(new UnhandledExceptionOutput(exception));
        }

        private bool IsAutomationOutput()
        {
            return _launchOptions.NonInteractive || _outputWriter.Format == ApplicationOutputFormat.Json;
        }
    }
}
