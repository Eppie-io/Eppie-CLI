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

using Eppie.CLI.Services;
using Eppie.CLI.Tests.TestDoubles;

using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

using Tuvi.Core;

namespace Eppie.CLI.Tests.Services
{
    [TestFixture]
    public class ApplicationUnlockerTests
    {
        [Test]
        public async Task UnlockAsyncWhenApplicationIsUninitializedReturnsFalseAndWritesUninitializedWarning()
        {
            FakeApplicationOutputWriter outputWriter = new();
            TuviMailInvocationState tuviMailState = new() { IsFirstApplicationStartResult = true };
            ApplicationUnlocker unlocker = CreateUnlocker(outputWriter, tuviMailState);

            bool result = await unlocker.UnlockAsync(CancellationToken.None, readPasswordFromStandardInput: true).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.False);
                Assert.That(tuviMailState.InitializeApplicationCallCount, Is.Zero);
                Assert.That(outputWriter.LastOutput, Is.TypeOf<UninitializedAppWarningOutput>());
            });
        }

        [Test]
        public async Task UnlockAsyncWhenReadPasswordFromStandardInputIsTrueReadsProvidedPasswordAndInitializesApplication()
        {
            FakeApplicationOutputWriter outputWriter = new();
            TuviMailInvocationState tuviMailState = new();
            FakeApplicationPasswordReader passwordReader = new() { StandardInputPassword = TestConstants.VaultPassword };
            ApplicationUnlocker unlocker = CreateUnlocker(outputWriter, tuviMailState, passwordReader);

            bool result = await unlocker.UnlockAsync(CancellationToken.None, readPasswordFromStandardInput: true).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(tuviMailState.InitializeApplicationCallCount, Is.EqualTo(1));
                Assert.That(tuviMailState.LastPassword, Is.EqualTo(TestConstants.VaultPassword));
                Assert.That(passwordReader.ReadPasswordFromStandardInputCallCount, Is.EqualTo(1));
                Assert.That(passwordReader.AskPasswordCallCount, Is.Zero);
                Assert.That(outputWriter.LastOutput, Is.Null);
            });
        }

        [Test]
        public async Task UnlockAsyncWhenReadPasswordFromStandardInputIsFalseUsesInteractivePasswordReader()
        {
            FakeApplicationOutputWriter outputWriter = new();
            TuviMailInvocationState tuviMailState = new();
            FakeApplicationPasswordReader passwordReader = new() { Password = "interactive-password" };
            ApplicationUnlocker unlocker = CreateUnlocker(outputWriter, tuviMailState, passwordReader);

            bool result = await unlocker.UnlockAsync(CancellationToken.None, readPasswordFromStandardInput: false).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(tuviMailState.InitializeApplicationCallCount, Is.EqualTo(1));
                Assert.That(tuviMailState.LastPassword, Is.EqualTo("interactive-password"));
                Assert.That(passwordReader.AskPasswordCallCount, Is.EqualTo(1));
                Assert.That(passwordReader.ReadPasswordFromStandardInputCallCount, Is.Zero);
                Assert.That(outputWriter.LastOutput, Is.Null);
            });
        }

        [Test]
        public async Task UnlockAsyncWhenPasswordIsInvalidReturnsFalseAndWritesInvalidPasswordWarning()
        {
            FakeApplicationOutputWriter outputWriter = new();
            TuviMailInvocationState tuviMailState = new() { InitializeApplicationResult = false };
            FakeApplicationPasswordReader passwordReader = new() { StandardInputPassword = TestConstants.WrongPassword };
            ApplicationUnlocker unlocker = CreateUnlocker(outputWriter, tuviMailState, passwordReader);

            bool result = await unlocker.UnlockAsync(CancellationToken.None, readPasswordFromStandardInput: true).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.False);
                Assert.That(tuviMailState.InitializeApplicationCallCount, Is.EqualTo(1));
                Assert.That(passwordReader.ReadPasswordFromStandardInputCallCount, Is.EqualTo(1));
                Assert.That(outputWriter.LastOutput, Is.TypeOf<InvalidPasswordWarningOutput>());
            });
        }

        [Test]
        public async Task UnlockAsyncPassesCancellationTokenToCoreCalls()
        {
            FakeApplicationOutputWriter outputWriter = new();
            TuviMailInvocationState tuviMailState = new();
            FakeApplicationPasswordReader passwordReader = new() { StandardInputPassword = TestConstants.VaultPassword };
            ApplicationUnlocker unlocker = CreateUnlocker(outputWriter, tuviMailState, passwordReader);
            using CancellationTokenSource cancellationTokenSource = new();

            bool result = await unlocker.UnlockAsync(cancellationTokenSource.Token, readPasswordFromStandardInput: true).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(tuviMailState.LastIsFirstApplicationStartCancellationToken, Is.EqualTo(cancellationTokenSource.Token));
                Assert.That(tuviMailState.LastInitializeApplicationCancellationToken, Is.EqualTo(cancellationTokenSource.Token));
            });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The fake core instance is assigned to the test core provider and lives for the lifetime of the unlocker under test.")]
        private static ApplicationUnlocker CreateUnlocker(FakeApplicationOutputWriter outputWriter, TuviMailInvocationState tuviMailState, FakeApplicationPasswordReader? passwordReader = null)
        {
            ArgumentNullException.ThrowIfNull(outputWriter);
            ArgumentNullException.ThrowIfNull(tuviMailState);

            FakeTuviMailCoreProvider coreProvider = new(new FakeTuviMail(tuviMailState));
            passwordReader ??= new FakeApplicationPasswordReader();

            return new ApplicationUnlocker(NullLogger<ApplicationUnlocker>.Instance, passwordReader, outputWriter, coreProvider);
        }

        private sealed class FakeApplicationPasswordReader : IApplicationPasswordReader
        {
            internal int AskPasswordCallCount { get; private set; }
            internal int ReadPasswordFromStandardInputCallCount { get; private set; }
            internal string Password { get; init; } = TestConstants.VaultPassword;
            internal string StandardInputPassword { get; init; } = TestConstants.VaultPassword;

            public string AskPassword()
            {
                AskPasswordCallCount++;
                return Password;
            }

            public string ReadPasswordFromStandardInput()
            {
                ReadPasswordFromStandardInputCallCount++;
                return StandardInputPassword;
            }
        }

        private sealed class FakeTuviMailCoreProvider(ITuviMail tuviMailCore) : ITuviMailCoreProvider
        {
            public ITuviMail TuviMailCore { get; } = tuviMailCore;

            public Task ResetAsync()
            {
                return Task.CompletedTask;
            }
        }
    }
}
