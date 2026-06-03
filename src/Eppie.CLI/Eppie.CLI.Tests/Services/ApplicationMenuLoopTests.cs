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

using Eppie.CLI.Menu;
using Eppie.CLI.Services;
using Eppie.CLI.Tests.TestDoubles;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NUnit.Framework;

namespace Eppie.CLI.Tests.Services
{
    [TestFixture]
    public class ApplicationMenuLoopTests
    {
        [Test]
        public async Task ExecuteAsyncWhenStartupCommandIsConfiguredInvokesCommandAndStopsApplication()
        {
            FakeApplicationMenu applicationMenu = new();
            using FakeHostApplicationLifetime lifetime = new();
            FakeStartupCommandRunner startupCommandRunner = new() { TryRunResult = true };
            using TestApplicationMenuLoop loop = new(lifetime, TestApplicationFactory.CreateLaunchOptionsOptions(), startupCommandRunner, new FakeApplicationOutputWriter(), applicationMenu);

            await loop.RunAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(startupCommandRunner.TryRunCallCount, Is.EqualTo(1));
                Assert.That(applicationMenu.LoopCallCount, Is.Zero);
                Assert.That(lifetime.StopApplicationCallCount, Is.EqualTo(1));
            });
        }

        [Test]
        public async Task ExecuteAsyncWhenNoStartupCommandIsConfiguredStartsInteractiveLoop()
        {
            FakeApplicationMenu applicationMenu = new();
            using FakeHostApplicationLifetime lifetime = new();
            FakeStartupCommandRunner startupCommandRunner = new() { TryRunResult = false };
            using TestApplicationMenuLoop loop = new(lifetime, TestApplicationFactory.CreateLaunchOptionsOptions(), startupCommandRunner, new FakeApplicationOutputWriter(), applicationMenu);

            await loop.RunAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(startupCommandRunner.TryRunCallCount, Is.EqualTo(1));
                Assert.That(applicationMenu.LoopCallCount, Is.EqualTo(1));
                Assert.That(lifetime.StopApplicationCallCount, Is.Zero);
            });
        }

        [Test]
        public async Task ExecuteAsyncWhenNoStartupCommandIsConfiguredInNonInteractiveModeWritesErrorAndStopsApplication()
        {
            FakeApplicationMenu applicationMenu = new();
            using FakeHostApplicationLifetime lifetime = new();
            FakeStartupCommandRunner startupCommandRunner = new() { TryRunResult = false };
            FakeApplicationOutputWriter outputWriter = new();
            using TestApplicationMenuLoop loop = new(lifetime, TestApplicationFactory.CreateLaunchOptionsOptions(nonInteractive: true), startupCommandRunner, outputWriter, applicationMenu);

            await loop.RunAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(startupCommandRunner.TryRunCallCount, Is.EqualTo(1));
                Assert.That(applicationMenu.LoopCallCount, Is.Zero);
                Assert.That(lifetime.StopApplicationCallCount, Is.EqualTo(1));
                Assert.That(outputWriter.LastOutput, Is.TypeOf<NonInteractiveOperationNotSupportedErrorOutput>());
                Assert.That(((NonInteractiveOperationNotSupportedErrorOutput)outputWriter.LastOutput!).Operation, Is.EqualTo(TestConstants.InteractiveMenu));
            });
        }

        [Test]
        public async Task ExecuteAsyncWhenStoppingTokenIsAlreadyCanceledDoesNothing()
        {
            FakeApplicationMenu applicationMenu = new();
            using FakeHostApplicationLifetime lifetime = new();
            FakeStartupCommandRunner startupCommandRunner = new() { TryRunResult = false };
            using TestApplicationMenuLoop loop = new(lifetime, TestApplicationFactory.CreateLaunchOptionsOptions(), startupCommandRunner, new FakeApplicationOutputWriter(), applicationMenu);
            using CancellationTokenSource cancellationTokenSource = new();
            await cancellationTokenSource.CancelAsync().ConfigureAwait(false);

            await loop.RunAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(startupCommandRunner.TryRunCallCount, Is.Zero);
                Assert.That(applicationMenu.LoopCallCount, Is.Zero);
                Assert.That(lifetime.StopApplicationCallCount, Is.Zero);
            });
        }

        [Test]
        public async Task ExecuteAsyncWhenStartupCommandIsNotRunAndApplicationStoppingIsTriggeredPassesLinkedCancellationTokenToMenuLoop()
        {
            FakeApplicationMenu applicationMenu = new();
            using FakeHostApplicationLifetime lifetime = new();
            FakeStartupCommandRunner startupCommandRunner = new() { TryRunResult = false };
            using TestApplicationMenuLoop loop = new(lifetime, TestApplicationFactory.CreateLaunchOptionsOptions(), startupCommandRunner, new FakeApplicationOutputWriter(), applicationMenu);

            lifetime.StopApplication();
            await loop.RunAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(startupCommandRunner.TryRunCallCount, Is.EqualTo(1));
                Assert.That(applicationMenu.LoopCallCount, Is.EqualTo(1));
                Assert.That(applicationMenu.LastLoopCancellationToken.CanBeCanceled, Is.True);
                Assert.That(applicationMenu.LastLoopCancellationToken.IsCancellationRequested, Is.True);
            });
        }

        [Test]
        public async Task ExecuteAsyncWhenStartupCommandIsNotRunPassesCancelableNonCanceledLinkedTokenToMenuLoop()
        {
            FakeApplicationMenu applicationMenu = new();
            using FakeHostApplicationLifetime lifetime = new();
            FakeStartupCommandRunner startupCommandRunner = new() { TryRunResult = false };
            using TestApplicationMenuLoop loop = new(lifetime, TestApplicationFactory.CreateLaunchOptionsOptions(), startupCommandRunner, new FakeApplicationOutputWriter(), applicationMenu);

            await loop.RunAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(startupCommandRunner.TryRunCallCount, Is.EqualTo(1));
                Assert.That(applicationMenu.LoopCallCount, Is.EqualTo(1));
                Assert.That(applicationMenu.LastLoopCancellationToken.CanBeCanceled, Is.True);
                Assert.That(applicationMenu.LastLoopCancellationToken.IsCancellationRequested, Is.False);
            });
        }

        private sealed class TestApplicationMenuLoop(
            IHostApplicationLifetime lifetime,
            IOptions<ApplicationLaunchOptions> launchOptions,
            IStartupCommandRunner startupCommandRunner,
            IApplicationOutputWriter outputWriter,
            IApplicationMenu applicationMenu)
            : ApplicationMenuLoop(NullLogger<ApplicationMenuLoop>.Instance, lifetime, launchOptions, startupCommandRunner, outputWriter, applicationMenu)
        {
            internal Task RunAsync(CancellationToken cancellationToken)
            {
                return ExecuteAsync(cancellationToken);
            }
        }

        private sealed class FakeStartupCommandRunner : IStartupCommandRunner
        {
            internal int TryRunCallCount { get; private set; }
            internal bool TryRunResult { get; init; }

            public Task<bool> TryRunAsync(CancellationToken cancellationToken)
            {
                TryRunCallCount++;
                return Task.FromResult(TryRunResult);
            }
        }

        private sealed class FakeApplicationMenu : IApplicationMenu
        {
            internal int LoopCallCount { get; private set; }
            internal CancellationToken LastLoopCancellationToken { get; private set; }

            public Task InvokeCommandAsync(string commandText)
            {
                return Task.CompletedTask;
            }

            public Task InvokeCommandAsync(IReadOnlyList<string> commandArguments)
            {
                ArgumentNullException.ThrowIfNull(commandArguments);
                return Task.CompletedTask;
            }

            public Task LoopAsync(CancellationToken stoppingToken)
            {
                LoopCallCount++;
                LastLoopCancellationToken = stoppingToken;
                return Task.CompletedTask;
            }
        }

        private sealed class FakeHostApplicationLifetime : IHostApplicationLifetime, IDisposable
        {
            internal int StopApplicationCallCount { get; private set; }
            private readonly CancellationTokenSource _applicationStopping = new();

            public CancellationToken ApplicationStarted => CancellationToken.None;

            public CancellationToken ApplicationStopping => _applicationStopping.Token;

            public CancellationToken ApplicationStopped => CancellationToken.None;

            public void StopApplication()
            {
                StopApplicationCallCount++;
                _applicationStopping.Cancel();
            }

            public void Dispose()
            {
                _applicationStopping.Dispose();
            }
        }
    }
}
