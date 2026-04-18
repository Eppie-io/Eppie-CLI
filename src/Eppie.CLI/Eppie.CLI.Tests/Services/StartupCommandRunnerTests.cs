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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NUnit.Framework;

namespace Eppie.CLI.Tests.Services
{
    [TestFixture]
    public class StartupCommandRunnerTests
    {
        [Test]
        public async Task TryRunAsyncWhenStartupCommandArgumentsAreMissingReturnsFalse()
        {
            IOptions<ApplicationLaunchOptions> launchOptions = CreateLaunchOptions();
            FakeApplicationUnlocker applicationUnlocker = new();
            FakeApplicationMenu applicationMenu = new();
            StartupCommandRunner runner = new(NullLogger<StartupCommandRunner>.Instance, new RawCommandLineArguments([]), launchOptions, new FakeApplicationOutputWriter(), applicationUnlocker, applicationMenu);

            bool result = await runner.TryRunAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.False);
                Assert.That(applicationUnlocker.UnlockCallCount, Is.Zero);
                Assert.That(applicationMenu.InvokeCommandCallCount, Is.Zero);
            });
        }

        [Test]
        public async Task TryRunAsyncWhenOnlyLaunchOptionsAreConfiguredReturnsFalse()
        {
            RawCommandLineArguments commandLineArguments = new([$"--{ApplicationLaunchOptions.NonInteractiveConfigurationKey}={TestConstants.True}", $"--{ApplicationLaunchOptions.OutputConfigurationKey}={TestConstants.Json}"]);
            IOptions<ApplicationLaunchOptions> launchOptions = TestApplicationFactory.CreateLaunchOptionsOptions(TestApplicationFactory.CreateLaunchOptionsFromCommandLine(commandLineArguments.Values.ToArray()));
            FakeApplicationUnlocker applicationUnlocker = new();
            FakeApplicationMenu applicationMenu = new();
            StartupCommandRunner runner = new(NullLogger<StartupCommandRunner>.Instance, commandLineArguments, launchOptions, new FakeApplicationOutputWriter(), applicationUnlocker, applicationMenu);

            bool result = await runner.TryRunAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.False);
                Assert.That(applicationUnlocker.UnlockCallCount, Is.Zero);
                Assert.That(applicationMenu.InvokeCommandCallCount, Is.Zero);
            });
        }

        [Test]
        public async Task TryRunAsyncWhenUnlockPasswordFromStandardInputIsProvidedWithExplicitTrueValueForLockedNonInteractiveStartupCommandUnlocksAndExecutesCommand()
        {
            RawCommandLineArguments commandLineArguments = new([$"--{ApplicationLaunchOptions.UnlockPasswordFromStandardInputConfigurationKey}={TestConstants.True}", $"--{ApplicationLaunchOptions.NonInteractiveConfigurationKey}={TestConstants.True}", StartupCommandArguments.CommandDelimiter, TestConstants.ListAccountsCommand]);
            IOptions<ApplicationLaunchOptions> launchOptions = TestApplicationFactory.CreateLaunchOptionsOptions(TestApplicationFactory.CreateLaunchOptionsFromCommandLine(commandLineArguments.Values.ToArray()));
            FakeApplicationUnlocker applicationUnlocker = new();
            FakeApplicationMenu applicationMenu = new();
            StartupCommandRunner runner = new(NullLogger<StartupCommandRunner>.Instance, commandLineArguments, launchOptions, new FakeApplicationOutputWriter(), applicationUnlocker, applicationMenu);

            bool result = await runner.TryRunAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(applicationUnlocker.UnlockCallCount, Is.EqualTo(1));
                Assert.That(applicationUnlocker.LastReadPasswordFromStandardInput, Is.True);
                Assert.That(applicationMenu.InvokeCommandCallCount, Is.EqualTo(1));
                Assert.That(applicationMenu.LastInvokedCommandArguments, Is.EqualTo([TestConstants.ListAccountsCommand]));
            });
        }

        [Test]
        public async Task TryRunAsyncWhenLockedInteractiveStartupCommandUnlockFailsDoesNotExecuteCommand()
        {
            IOptions<ApplicationLaunchOptions> launchOptions = CreateLaunchOptions();
            FakeApplicationUnlocker applicationUnlocker = new() { UnlockResult = false };
            FakeApplicationMenu applicationMenu = new();
            StartupCommandRunner runner = new(NullLogger<StartupCommandRunner>.Instance, new RawCommandLineArguments([StartupCommandArguments.CommandDelimiter, TestConstants.ListAccountsCommand]), launchOptions, new FakeApplicationOutputWriter(), applicationUnlocker, applicationMenu);

            bool result = await runner.TryRunAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(applicationUnlocker.UnlockCallCount, Is.EqualTo(1));
                Assert.That(applicationUnlocker.LastReadPasswordFromStandardInput, Is.False);
                Assert.That(applicationMenu.InvokeCommandCallCount, Is.Zero);
            });
        }

        [Test]
        public async Task TryRunAsyncWhenStartupCommandArgumentsAreConfiguredInvokesCommand()
        {
            IOptions<ApplicationLaunchOptions> launchOptions = CreateLaunchOptions();
            FakeApplicationUnlocker applicationUnlocker = new();
            FakeApplicationMenu applicationMenu = new();
            StartupCommandRunner runner = new(NullLogger<StartupCommandRunner>.Instance, new RawCommandLineArguments([StartupCommandArguments.CommandDelimiter, MenuCommand.Open.Name]), launchOptions, new FakeApplicationOutputWriter(), applicationUnlocker, applicationMenu);

            bool result = await runner.TryRunAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(applicationUnlocker.UnlockCallCount, Is.Zero);
                Assert.That(applicationMenu.InvokeCommandCallCount, Is.EqualTo(1));
                Assert.That(applicationMenu.LastInvokedCommandArguments, Is.EqualTo([MenuCommand.Open.Name]));
            });
        }

        [Test]
        public async Task TryRunAsyncWhenStartupCommandIsPrefixedWithInlineLaunchOptionInvokesCommand()
        {
            RawCommandLineArguments commandLineArguments = new([$"--{ApplicationLaunchOptions.NonInteractiveConfigurationKey}={TestConstants.True}", StartupCommandArguments.CommandDelimiter, MenuCommand.Open.Name]);
            IOptions<ApplicationLaunchOptions> launchOptions = TestApplicationFactory.CreateLaunchOptionsOptions(TestApplicationFactory.CreateLaunchOptionsFromCommandLine(commandLineArguments.Values.ToArray()));
            FakeApplicationUnlocker applicationUnlocker = new();
            FakeApplicationMenu applicationMenu = new();
            StartupCommandRunner runner = new(NullLogger<StartupCommandRunner>.Instance, commandLineArguments, launchOptions, new FakeApplicationOutputWriter(), applicationUnlocker, applicationMenu);

            bool result = await runner.TryRunAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(applicationUnlocker.UnlockCallCount, Is.Zero);
                Assert.That(applicationMenu.InvokeCommandCallCount, Is.EqualTo(1));
                Assert.That(applicationMenu.LastInvokedCommandArguments, Is.EqualTo([MenuCommand.Open.Name]));
            });
        }

        [Test]
        public async Task TryRunAsyncWhenStartupCommandIsPrefixedWithSeparateValueLaunchOptionSkipsBothOptionAndValue()
        {
            RawCommandLineArguments commandLineArguments = new([$"--{ApplicationLaunchOptions.NonInteractiveConfigurationKey}", TestConstants.False, StartupCommandArguments.CommandDelimiter, TestConstants.ListAccountsCommand]);
            IOptions<ApplicationLaunchOptions> launchOptions = TestApplicationFactory.CreateLaunchOptionsOptions(TestApplicationFactory.CreateLaunchOptionsFromCommandLine(commandLineArguments.Values.ToArray()));
            FakeApplicationUnlocker applicationUnlocker = new();
            FakeApplicationMenu applicationMenu = new();
            StartupCommandRunner runner = new(NullLogger<StartupCommandRunner>.Instance, commandLineArguments, launchOptions, new FakeApplicationOutputWriter(), applicationUnlocker, applicationMenu);

            bool result = await runner.TryRunAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(applicationUnlocker.UnlockCallCount, Is.EqualTo(1));
                Assert.That(applicationUnlocker.LastReadPasswordFromStandardInput, Is.False);
                Assert.That(applicationMenu.InvokeCommandCallCount, Is.EqualTo(1));
                Assert.That(applicationMenu.LastInvokedCommandArguments, Is.EqualTo([TestConstants.ListAccountsCommand]));
            });
        }

        [Test]
        public async Task TryRunAsyncWhenStartupCommandIsHelpDoesNotUnlockApplication()
        {
            IOptions<ApplicationLaunchOptions> launchOptions = CreateLaunchOptions(unlockPasswordFromStandardInput: true);
            FakeApplicationUnlocker applicationUnlocker = new();
            FakeApplicationMenu applicationMenu = new();
            StartupCommandRunner runner = new(NullLogger<StartupCommandRunner>.Instance, new RawCommandLineArguments([StartupCommandArguments.CommandDelimiter, "-h"]), launchOptions, new FakeApplicationOutputWriter(), applicationUnlocker, applicationMenu);

            bool result = await runner.TryRunAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(applicationUnlocker.UnlockCallCount, Is.Zero);
                Assert.That(applicationMenu.InvokeCommandCallCount, Is.EqualTo(1));
                Assert.That(applicationMenu.LastInvokedCommandArguments, Is.EqualTo(["-h"]));
            });
        }

        [Test]
        public async Task TryRunAsyncWhenStartupCommandIsOpenDoesNotUnlockApplication()
        {
            IOptions<ApplicationLaunchOptions> launchOptions = CreateLaunchOptions(unlockPasswordFromStandardInput: true);
            FakeApplicationUnlocker applicationUnlocker = new();
            FakeApplicationMenu applicationMenu = new();
            StartupCommandRunner runner = new(NullLogger<StartupCommandRunner>.Instance, new RawCommandLineArguments([StartupCommandArguments.CommandDelimiter, MenuCommand.Open.Name]), launchOptions, new FakeApplicationOutputWriter(), applicationUnlocker, applicationMenu);

            bool result = await runner.TryRunAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(applicationUnlocker.UnlockCallCount, Is.Zero);
                Assert.That(applicationMenu.InvokeCommandCallCount, Is.EqualTo(1));
                Assert.That(applicationMenu.LastInvokedCommandArguments, Is.EqualTo([MenuCommand.Open.Name]));
            });
        }

        [Test]
        public async Task TryRunAsyncWhenLockedInteractiveStartupCommandRunsUnlocksBeforeExecutingCommandWithoutReadingPasswordFromStandardInput()
        {
            IOptions<ApplicationLaunchOptions> launchOptions = CreateLaunchOptions();
            FakeApplicationUnlocker applicationUnlocker = new();
            FakeApplicationMenu applicationMenu = new();
            StartupCommandRunner runner = new(NullLogger<StartupCommandRunner>.Instance, new RawCommandLineArguments([StartupCommandArguments.CommandDelimiter, TestConstants.ListAccountsCommand]), launchOptions, new FakeApplicationOutputWriter(), applicationUnlocker, applicationMenu);

            bool result = await runner.TryRunAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(applicationUnlocker.UnlockCallCount, Is.EqualTo(1));
                Assert.That(applicationUnlocker.LastReadPasswordFromStandardInput, Is.False);
                Assert.That(applicationMenu.InvokeCommandCallCount, Is.EqualTo(1));
                Assert.That(applicationMenu.LastInvokedCommandArguments, Is.EqualTo([TestConstants.ListAccountsCommand]));
            });
        }

        [Test]
        public async Task TryRunAsyncWhenUnlockPasswordFromStandardInputIsEnabledInInteractiveModeIgnoresFlagAndUnlocksWithoutReadingPasswordFromStandardInput()
        {
            IOptions<ApplicationLaunchOptions> launchOptions = CreateLaunchOptions(unlockPasswordFromStandardInput: true);
            FakeApplicationUnlocker applicationUnlocker = new();
            FakeApplicationMenu applicationMenu = new();
            StartupCommandRunner runner = new(NullLogger<StartupCommandRunner>.Instance, new RawCommandLineArguments([$"--{ApplicationLaunchOptions.UnlockPasswordFromStandardInputConfigurationKey}={TestConstants.True}", StartupCommandArguments.CommandDelimiter, TestConstants.ListAccountsCommand]), launchOptions, new FakeApplicationOutputWriter(), applicationUnlocker, applicationMenu);

            bool result = await runner.TryRunAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(applicationUnlocker.UnlockCallCount, Is.EqualTo(1));
                Assert.That(applicationUnlocker.LastReadPasswordFromStandardInput, Is.False);
                Assert.That(applicationMenu.InvokeCommandCallCount, Is.EqualTo(1));
                Assert.That(applicationMenu.LastInvokedCommandArguments, Is.EqualTo([TestConstants.ListAccountsCommand]));
            });
        }

        [Test]
        public async Task TryRunAsyncWhenNonInteractiveUnlockFailsDoesNotExecuteCommand()
        {
            IOptions<ApplicationLaunchOptions> launchOptions = CreateLaunchOptions(unlockPasswordFromStandardInput: true, nonInteractive: true);
            FakeApplicationUnlocker applicationUnlocker = new() { UnlockResult = false };
            FakeApplicationMenu applicationMenu = new();
            StartupCommandRunner runner = new(NullLogger<StartupCommandRunner>.Instance, new RawCommandLineArguments([$"--{ApplicationLaunchOptions.UnlockPasswordFromStandardInputConfigurationKey}={TestConstants.True}", $"--{ApplicationLaunchOptions.NonInteractiveConfigurationKey}={TestConstants.True}", StartupCommandArguments.CommandDelimiter, TestConstants.ListAccountsCommand]), launchOptions, new FakeApplicationOutputWriter(), applicationUnlocker, applicationMenu);

            bool result = await runner.TryRunAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(applicationUnlocker.UnlockCallCount, Is.EqualTo(1));
                Assert.That(applicationUnlocker.LastReadPasswordFromStandardInput, Is.True);
                Assert.That(applicationMenu.InvokeCommandCallCount, Is.Zero);
            });
        }

        [Test]
        public async Task TryRunAsyncWhenUnlockPasswordFromStandardInputIsMissingForLockedNonInteractiveStartupCommandDoesNotUnlockOrExecuteCommand()
        {
            IOptions<ApplicationLaunchOptions> launchOptions = CreateLaunchOptions(nonInteractive: true);
            FakeApplicationUnlocker applicationUnlocker = new();
            FakeApplicationMenu applicationMenu = new();
            FakeApplicationOutputWriter outputWriter = new();
            StartupCommandRunner runner = new(NullLogger<StartupCommandRunner>.Instance, new RawCommandLineArguments([StartupCommandArguments.CommandDelimiter, TestConstants.ListAccountsCommand]), launchOptions, outputWriter, applicationUnlocker, applicationMenu);

            bool result = await runner.TryRunAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(applicationUnlocker.UnlockCallCount, Is.Zero);
                Assert.That(applicationMenu.InvokeCommandCallCount, Is.Zero);
                Assert.That(outputWriter.LastOutput, Is.TypeOf<StartupCommandRequiresUnlockPasswordFromStandardInputWarningOutput>());
                Assert.That(((StartupCommandRequiresUnlockPasswordFromStandardInputWarningOutput)outputWriter.LastOutput!).CommandName, Is.EqualTo(TestConstants.ListAccountsCommand));
            });
        }

        [Test]
        public async Task TryRunAsyncWhenLockedNonInteractiveStartupCommandHasExplicitUnlockPasswordFromStandardInputFalseWritesWarningAndSkipsCommand()
        {
            RawCommandLineArguments commandLineArguments = new([$"--{ApplicationLaunchOptions.UnlockPasswordFromStandardInputConfigurationKey}={TestConstants.False}", $"--{ApplicationLaunchOptions.NonInteractiveConfigurationKey}={TestConstants.True}", StartupCommandArguments.CommandDelimiter, TestConstants.ListAccountsCommand]);
            IOptions<ApplicationLaunchOptions> launchOptions = TestApplicationFactory.CreateLaunchOptionsOptions(TestApplicationFactory.CreateLaunchOptionsFromCommandLine(commandLineArguments.Values.ToArray()));
            FakeApplicationUnlocker applicationUnlocker = new();
            FakeApplicationMenu applicationMenu = new();
            FakeApplicationOutputWriter outputWriter = new();
            StartupCommandRunner runner = new(NullLogger<StartupCommandRunner>.Instance, commandLineArguments, launchOptions, outputWriter, applicationUnlocker, applicationMenu);

            bool result = await runner.TryRunAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(applicationUnlocker.UnlockCallCount, Is.Zero);
                Assert.That(applicationMenu.InvokeCommandCallCount, Is.Zero);
                Assert.That(outputWriter.LastOutput, Is.TypeOf<StartupCommandRequiresUnlockPasswordFromStandardInputWarningOutput>());
                Assert.That(((StartupCommandRequiresUnlockPasswordFromStandardInputWarningOutput)outputWriter.LastOutput!).CommandName, Is.EqualTo(TestConstants.ListAccountsCommand));
            });
        }

        [Test]
        public async Task TryRunAsyncWhenStartupCommandHasCommandSpecificArgumentsPreservesThem()
        {
            RawCommandLineArguments commandLineArguments = new([
                $"--{ApplicationLaunchOptions.NonInteractiveConfigurationKey}={TestConstants.True}",
                $"--{ApplicationLaunchOptions.UnlockPasswordFromStandardInputConfigurationKey}={TestConstants.True}",
                StartupCommandArguments.CommandDelimiter,
                TestConstants.ShowMessageCommand,
                TestConstants.AccountOption,
                TestConstants.AccountAddress,
                TestConstants.FolderOption,
                TestConstants.Inbox,
                TestConstants.MessageIdOption,
                TestConstants.One,
                TestConstants.PrimaryKeyOption,
                TestConstants.Two
            ]);
            IOptions<ApplicationLaunchOptions> launchOptions = TestApplicationFactory.CreateLaunchOptionsOptions(TestApplicationFactory.CreateLaunchOptionsFromCommandLine(commandLineArguments.Values.ToArray()));
            FakeApplicationUnlocker applicationUnlocker = new();
            FakeApplicationMenu applicationMenu = new();
            StartupCommandRunner runner = new(NullLogger<StartupCommandRunner>.Instance, commandLineArguments, launchOptions, new FakeApplicationOutputWriter(), applicationUnlocker, applicationMenu);

            bool result = await runner.TryRunAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(applicationUnlocker.UnlockCallCount, Is.EqualTo(1));
                Assert.That(applicationUnlocker.LastReadPasswordFromStandardInput, Is.True);
                Assert.That(applicationMenu.InvokeCommandCallCount, Is.EqualTo(1));
                Assert.That(applicationMenu.LastInvokedCommandArguments, Is.EqualTo([TestConstants.ShowMessageCommand, TestConstants.AccountOption, TestConstants.AccountAddress, TestConstants.FolderOption, TestConstants.Inbox, TestConstants.MessageIdOption, TestConstants.One, TestConstants.PrimaryKeyOption, TestConstants.Two]));
            });
        }

        [Test]
        public async Task TryRunAsyncWhenStartupCommandIsUnknownDoesNotAttemptUnlockAndStillInvokesMenu()
        {
            IOptions<ApplicationLaunchOptions> launchOptions = CreateLaunchOptions(unlockPasswordFromStandardInput: true, nonInteractive: true);
            FakeApplicationUnlocker applicationUnlocker = new();
            FakeApplicationMenu applicationMenu = new();
            StartupCommandRunner runner = new(NullLogger<StartupCommandRunner>.Instance, new RawCommandLineArguments([StartupCommandArguments.CommandDelimiter, "unknown-command"]), launchOptions, new FakeApplicationOutputWriter(), applicationUnlocker, applicationMenu);

            bool result = await runner.TryRunAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(applicationUnlocker.UnlockCallCount, Is.Zero);
                Assert.That(applicationMenu.InvokeCommandCallCount, Is.EqualTo(1));
                Assert.That(applicationMenu.LastInvokedCommandArguments, Is.EqualTo(["unknown-command"]));
            });
        }

        private static IOptions<ApplicationLaunchOptions> CreateLaunchOptions(bool unlockPasswordFromStandardInput = false, bool nonInteractive = false)
        {
            return TestApplicationFactory.CreateLaunchOptionsOptions(unlockPasswordFromStandardInput: unlockPasswordFromStandardInput, nonInteractive: nonInteractive);
        }

        private sealed class FakeApplicationUnlocker : IApplicationUnlocker
        {
            internal int UnlockCallCount { get; private set; }
            internal bool UnlockResult { get; init; } = true;
            internal bool LastReadPasswordFromStandardInput { get; private set; }

            public Task<bool> UnlockAsync(CancellationToken cancellationToken, bool readPasswordFromStandardInput = false)
            {
                UnlockCallCount++;
                LastReadPasswordFromStandardInput = readPasswordFromStandardInput;
                return Task.FromResult(UnlockResult);
            }
        }

        private sealed class FakeApplicationMenu : IApplicationMenu
        {
            internal int InvokeCommandCallCount { get; private set; }
            internal IReadOnlyList<string>? LastInvokedCommandArguments { get; private set; }

            public Task InvokeCommandAsync(string commandText)
            {
                return InvokeCommandAsync([commandText]);
            }

            public Task InvokeCommandAsync(IReadOnlyList<string> commandArguments)
            {
                ArgumentNullException.ThrowIfNull(commandArguments);

                InvokeCommandCallCount++;
                LastInvokedCommandArguments = [.. commandArguments];
                return Task.CompletedTask;
            }

            public Task LoopAsync(CancellationToken stoppingToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
