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
using Eppie.CLI.Tools;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal partial class ApplicationMenuLoop(
        ILogger<ApplicationMenuLoop> logger,
        IHostApplicationLifetime lifetime,
        IOptions<ApplicationLaunchOptions> launchOptions,
        IStartupCommandRunner startupCommandRunner,
        IApplicationFailureHandler failureHandler,
        Menu.IApplicationMenu applicationMenu) : BackgroundService
    {
        private const string InteractiveMenuOperationName = "interactive menu";

        private readonly ILogger<ApplicationMenuLoop> _logger = logger;
        private readonly IHostApplicationLifetime _lifetime = lifetime;
        private readonly ApplicationLaunchOptions _launchOptions = launchOptions.Value;
        private readonly IStartupCommandRunner _startupCommandRunner = startupCommandRunner;
        private readonly IApplicationFailureHandler _failureHandler = failureHandler;
        private readonly Menu.IApplicationMenu _applicationMenu = applicationMenu;

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "to report the failure instead of letting it escape the background service")]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogMethodCall();
            await Task.Yield();

            try
            {
                if (_launchOptions.OutputFormat == ApplicationOutputFormat.Json && !_launchOptions.NonInteractive)
                {
                    throw new ApplicationCommandException(new InteractiveInputNotSupportedErrorOutput());
                }

                if (!stoppingToken.IsCancellationRequested && await _startupCommandRunner.TryRunAsync(stoppingToken).ConfigureAwait(false))
                {
                    _lifetime.StopApplication();
                    return;
                }

                if (!stoppingToken.IsCancellationRequested)
                {
                    if (_launchOptions.NonInteractive)
                    {
                        throw new ApplicationCommandException(new NonInteractiveOperationNotSupportedErrorOutput(InteractiveMenuOperationName));
                    }

                    using CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _lifetime.ApplicationStopping);
                    await _applicationMenu.LoopAsync(cancellationTokenSource.Token).ConfigureAwait(false);
                }
            }
            catch (ReadValueCanceledException ex)
            {
                Fail(new ApplicationCommandException(new StandardInputEndedErrorOutput(), innerException: ex));
            }
            catch (ApplicationCommandException ex) when (ex.Output is not null)
            {
                Fail(ex);
            }
            catch (InputCanceledByUserException)
            {
                Abort();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                Abort();
            }
            catch (Exception ex)
            {
                _failureHandler.HandleUnhandledException(ex);
                _lifetime.StopApplication();
            }
        }

        private void Abort()
        {
            Environment.ExitCode = ApplicationCommandException.FailureExitCode;
            _lifetime.StopApplication();
        }

        private void Fail(ApplicationCommandException exception)
        {
            _failureHandler.HandleControlledCommandFailure(exception);
            _lifetime.StopApplication();
        }
    }
}
