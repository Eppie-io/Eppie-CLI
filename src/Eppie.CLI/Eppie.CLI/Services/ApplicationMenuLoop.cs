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

using Eppie.CLI.Menu;
using Eppie.CLI.Tools;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal partial class ApplicationMenuLoop(
        ILogger<ApplicationMenuLoop> logger,
        IHostApplicationLifetime lifetime,
        MainMenu mainMenu) : BackgroundService
    {
        private readonly ILogger<ApplicationMenuLoop> _logger = logger;
        private readonly IHostApplicationLifetime _lifetime = lifetime;
        private readonly MainMenu _mainMenu = mainMenu;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogMethodCall();
            await Task.Yield();

            if (!stoppingToken.IsCancellationRequested)
            {
                using CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _lifetime.ApplicationStopping);
                await _mainMenu.LoopAsync(cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }
    }
}
