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

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Eppie.CLI.Services
{
    internal abstract class WorkService : BackgroundService
    {
        public CancellationToken Ready => _ready.Token;

        protected ILogger Logger { get; private set; }

        private readonly CancellationTokenSource _waitLaunch = new();
        private readonly CancellationTokenSource _waitInitialization = new();
        private readonly CancellationTokenSource _ready = new();

        public const bool Initializer = true;

        protected WorkService(ILogger logger)
        {
            Logger = logger;
        }

        public void Launch()
        {
            Logger.LogTrace("WorkService.Launch has been called.");

            _waitLaunch.Cancel();
        }

        public void Initialize()
        {
            Logger.LogTrace("WorkService.Initialize has been called.");

            _waitInitialization.Cancel();
        }

        public override void Dispose()
        {
            base.Dispose();

            _waitLaunch.Dispose();
            _waitInitialization.Dispose();
            _ready.Dispose();
        }

        protected virtual Task InitializeAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task DoWorkAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            await WaitAsync(_waitInitialization.Token, stoppingToken).ConfigureAwait(false);
            await InitializeAsync(stoppingToken).ConfigureAwait(false);

            _ready.Cancel();

            await WaitAsync(_waitLaunch.Token, stoppingToken).ConfigureAwait(false);
            await DoWorkAsync(stoppingToken).ConfigureAwait(false);
        }

        private async Task WaitAsync(CancellationToken waitToken, CancellationToken stoppingToken)
        {
            Logger.LogTrace("WorkService.WaitAsync has been called.");

            try
            {
                using CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, waitToken);

                await Task.Delay(Timeout.Infinite, linkedTokenSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            { }
        }
    }
}
