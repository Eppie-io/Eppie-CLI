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

using System.Collections.ObjectModel;

using Eppie.CLI.Exceptions;
using Eppie.CLI.Menu;
using Eppie.CLI.Options;
using Eppie.CLI.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

using Tuvi.Core;
using Tuvi.Core.Entities;
using Tuvi.Proton;

namespace Eppie.CLI.Tests.Services
{
    [TestFixture]
    public class ProtonLoginRetryPolicyTests
    {
        private const string ValidToken = "challenge:solution";

        private ServiceProvider _serviceProvider = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _serviceProvider = new ServiceCollection()
                .AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance)
                .AddLocalization()
                .BuildServiceProvider();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _serviceProvider.Dispose();
        }

        [TestCase(ProtonLoginStage.HumanVerification)]
        [TestCase(ProtonLoginStage.TwoFactorCode)]
        [TestCase(ProtonLoginStage.MailboxPassword)]
        public void WhenStructuredValueIsRejectedTheLoginIsNotRetriedForever(ProtonLoginStage stage)
        {
            RecordingOutputWriter writer = new();
            RetryingLoginHelper loginHelper = new(stage);

            ApplicationCommandException? exception = Assert.ThrowsAsync<ApplicationCommandException>(
                () => RunAddProtonAccountAsync(writer, loginHelper, new StubProtonAccountInput(ValidToken)));

            Assert.Multiple(() =>
            {
                Assert.That(loginHelper.ProviderCallCount, Is.EqualTo(2), "a value that cannot change must not be replayed");
                Assert.That(loginHelper.LimitReached, Is.False);
                Assert.That(loginHelper.Cancelled, Is.True);
                Assert.That(writer.Written, Has.Some.InstanceOf<UnsuccessfulAttemptWarningOutput>());

                Assert.That(writer.Written.OfType<ProtonHumanVerificationRequiredOutput>().Count(),
                            Is.EqualTo(stage == ProtonLoginStage.HumanVerification ? 2 : 0));

                Assert.That(exception!.Output, Is.InstanceOf<AuthorizationCanceledOutput>());
            });
        }

        [Test]
        public void WhenAnEmptyTokenIsEnteredVerificationIsDeclinedWithoutContactingProton()
        {
            RecordingOutputWriter writer = new();
            RetryingLoginHelper loginHelper = new(ProtonLoginStage.HumanVerification);

            ApplicationCommandException? exception = Assert.ThrowsAsync<ApplicationCommandException>(
                () => RunAddProtonAccountAsync(writer, loginHelper, new StubProtonAccountInput("   ", supportsRetry: true)));

            Assert.Multiple(() =>
            {
                Assert.That(loginHelper.ProviderCallCount, Is.EqualTo(1));
                Assert.That(loginHelper.Cancelled, Is.True);
                Assert.That(exception!.Output, Is.InstanceOf<AuthorizationCanceledOutput>());
            });
        }

        [TestCase(ProtonLoginStage.HumanVerification)]
        [TestCase(ProtonLoginStage.TwoFactorCode)]
        [TestCase(ProtonLoginStage.MailboxPassword)]
        public void WhenAValueIsBlankItIsDeclinedWithoutContactingProton(ProtonLoginStage stage)
        {
            RecordingOutputWriter writer = new();
            RetryingLoginHelper loginHelper = new(stage);

            ApplicationCommandException? exception = Assert.ThrowsAsync<ApplicationCommandException>(
                () => RunAddProtonAccountAsync(writer, loginHelper, new StubProtonAccountInput(string.Empty, supportsRetry: true)));

            Assert.Multiple(() =>
            {
                Assert.That(loginHelper.ProviderCallCount, Is.EqualTo(1));
                Assert.That(loginHelper.Cancelled, Is.True);
                Assert.That(exception!.Output, Is.InstanceOf<AuthorizationCanceledOutput>());
            });
        }

        [Test]
        public void WhenTheLoginIsCancelledWithoutAnOperatorDecliningItKeepsItsOwnIdentity()
        {
            RecordingOutputWriter writer = new();
            TimingOutLoginHelper loginHelper = new();

            Assert.ThrowsAsync<TaskCanceledException>(
                () => RunAddProtonAccountAsync(writer, loginHelper, new StubProtonAccountInput(ValidToken)));

            Assert.That(writer.Written, Has.None.InstanceOf<AuthorizationCanceledOutput>());
        }

        [Test]
        public void WhenStandardInputRunsOutTheLoginLeavesTheReasonIntact()
        {
            RecordingOutputWriter writer = new();
            RetryingLoginHelper loginHelper = new(ProtonLoginStage.HumanVerification);

            Assert.ThrowsAsync<ReadValueCanceledException>(
                () => RunAddProtonAccountAsync(writer, loginHelper, new ExhaustedStandardInput()));

            Assert.That(writer.Written, Has.None.InstanceOf<AuthorizationCanceledOutput>());
        }

        [Test]
        public void SurroundingWhitespaceIsStrippedFromTheVerificationToken()
        {
            RecordingOutputWriter writer = new();
            CapturingLoginHelper loginHelper = new();

            Assert.ThrowsAsync<NotSupportedException>(
                () => RunAddProtonAccountAsync(writer, loginHelper, new StubProtonAccountInput($"  {ValidToken}\t")));

            Assert.That(loginHelper.CapturedToken, Is.EqualTo(ValidToken));
        }

        [TestCase(ProtonLoginStage.HumanVerification)]
        [TestCase(ProtonLoginStage.TwoFactorCode)]
        [TestCase(ProtonLoginStage.MailboxPassword)]
        public void WhenInputCanChangeTheValueTheLoginKeepsAskingAgain(ProtonLoginStage stage)
        {
            RecordingOutputWriter writer = new();
            RetryingLoginHelper loginHelper = new(stage, maxCalls: 3);

            Assert.ThrowsAsync<NotSupportedException>(
                () => RunAddProtonAccountAsync(writer, loginHelper, new StubProtonAccountInput(ValidToken, supportsRetry: true)));

            Assert.Multiple(() =>
            {
                Assert.That(loginHelper.ProviderCallCount, Is.EqualTo(3));
                Assert.That(loginHelper.LimitReached, Is.True);
                Assert.That(loginHelper.Cancelled, Is.False);
            });
        }

        private Task RunAddProtonAccountAsync(RecordingOutputWriter writer, IProtonLoginHelper loginHelper, IProtonAccountInput input)
        {
            IStringLocalizer<Resources.Program> localizer = _serviceProvider.GetRequiredService<IStringLocalizer<Resources.Program>>();
            ResourceLoader resourceLoader = new(localizer);

            Application application = new(NullLogger<Application>.Instance,
                                          new StubHostApplicationLifetime(),
                                          TestApplicationFactory.CreateLaunchOptionsOptions(nonInteractive: true),
                                          writer,
                                          Microsoft.Extensions.Options.Options.Create(new MailOptions()),
                                          resourceLoader);

            Actions actions = new(NullLogger<Actions>.Instance,
                                  application,
                                  TestApplicationFactory.CreateLaunchOptionsOptions(nonInteractive: true),
                                  writer,
                                  new StubFailureHandler(),
                                  new StubOutputCoordinator(),
                                  new StubEmailAccountInputResolver(),
                                  new StubProtonAccountInputResolver(input),
                                  loginHelper,
                                  new AuthorizationProvider(NullLogger<AuthorizationProvider>.Instance, _serviceProvider),
                                  new StubCoreProvider());

            return actions.AddAccountActionAsync(new MenuCommand.CommandAddAccountOptions.Options(
                MenuCommand.CommandAddAccountOptions.AccountType.Proton,
                InputJsonFromStandardInput: true));
        }

        public enum ProtonLoginStage
        {
            HumanVerification,
            TwoFactorCode,
            MailboxPassword,
        }

        private sealed class RetryingLoginHelper(ProtonLoginStage stage, int maxCalls = 25) : IProtonLoginHelper
        {
            private static readonly Uri VerifierUri = new(TestConstants.ProtonCaptchaUri);

            internal int ProviderCallCount { get; private set; }

            internal bool Cancelled { get; private set; }

            internal bool LimitReached { get; private set; }

            public async Task<ProtonCredentials> LoginAsync(string userName,
                                                            string password,
                                                            TwoFactorCodeProvider twoFactorCodeProvider,
                                                            MailboxPasswordProvider mailboxPasswordProvider,
                                                            HumanVerifier humanVerifier,
                                                            CancellationToken cancellationToken)
            {
                ArgumentNullException.ThrowIfNull(twoFactorCodeProvider);
                ArgumentNullException.ThrowIfNull(mailboxPasswordProvider);
                ArgumentNullException.ThrowIfNull(humanVerifier);

                Exception? previousAttemptException = null;

                while (ProviderCallCount < maxCalls)
                {
                    ProviderCallCount++;

                    bool completed = stage switch
                    {
                        ProtonLoginStage.TwoFactorCode => (await twoFactorCodeProvider(previousAttemptException, cancellationToken).ConfigureAwait(false)).completed,
                        ProtonLoginStage.MailboxPassword => (await mailboxPasswordProvider(previousAttemptException, cancellationToken).ConfigureAwait(false)).completed,
                        ProtonLoginStage.HumanVerification => (await humanVerifier(VerifierUri, previousAttemptException, cancellationToken).ConfigureAwait(false)).completed,
                        _ => throw new NotSupportedException($"Unknown stage '{stage}'."),
                    };

                    if (!completed)
                    {
                        Cancelled = true;
                        throw new OperationCanceledException();
                    }

                    previousAttemptException = new InvalidOperationException("Proton rejected the value");
                }

                LimitReached = true;
                throw new NotSupportedException("The provider kept being asked up to the cap.");
            }
        }

        private sealed class TimingOutLoginHelper : IProtonLoginHelper
        {
            public Task<ProtonCredentials> LoginAsync(string userName,
                                                      string password,
                                                      TwoFactorCodeProvider twoFactorCodeProvider,
                                                      MailboxPasswordProvider mailboxPasswordProvider,
                                                      HumanVerifier humanVerifier,
                                                      CancellationToken cancellationToken)
            {
                throw new TaskCanceledException("The request was canceled due to the configured HttpClient.Timeout.");
            }
        }

        private sealed class CapturingLoginHelper : IProtonLoginHelper
        {
            private static readonly Uri VerifierUri = new(TestConstants.ProtonCaptchaUri);

            internal string? CapturedToken { get; private set; }

            public async Task<ProtonCredentials> LoginAsync(string userName,
                                                            string password,
                                                            TwoFactorCodeProvider twoFactorCodeProvider,
                                                            MailboxPasswordProvider mailboxPasswordProvider,
                                                            HumanVerifier humanVerifier,
                                                            CancellationToken cancellationToken)
            {
                ArgumentNullException.ThrowIfNull(humanVerifier);

                (_, _, string token) = await humanVerifier(VerifierUri, null, cancellationToken).ConfigureAwait(false);
                CapturedToken = token;

                throw new NotSupportedException("The token has been captured, the rest of the login is out of scope.");
            }
        }

        private sealed class ExhaustedStandardInput : IProtonAccountInput
        {
            public bool SupportsRetry => true;

            public string Email => TestConstants.UserAddress;

            public string AccountPassword => "secret";

            public string GetTwoFactorCode(bool firstAttempt)
            {
                throw new ReadValueCanceledException();
            }

            public string GetMailboxPassword(bool firstAttempt)
            {
                throw new ReadValueCanceledException();
            }

            public string GetHumanVerificationToken()
            {
                throw new ReadValueCanceledException();
            }
        }

        private sealed class StubProtonAccountInput(string value, bool supportsRetry = false) : IProtonAccountInput
        {
            public bool SupportsRetry => supportsRetry;

            public string Email => TestConstants.UserAddress;

            public string AccountPassword => "secret";

            public string GetTwoFactorCode(bool firstAttempt)
            {
                return value;
            }

            public string GetMailboxPassword(bool firstAttempt)
            {
                return value;
            }

            public string GetHumanVerificationToken()
            {
                return value;
            }
        }

        private sealed class StubProtonAccountInputResolver(IProtonAccountInput input) : IProtonAccountInputResolver
        {
            public Task<IProtonAccountInput> ResolveAsync(bool inputJsonFromStandardInput)
            {
                return Task.FromResult(input);
            }
        }

        private sealed class RecordingOutputWriter : IApplicationOutputWriter
        {
            internal Collection<ApplicationOutput> Written { get; } = [];

            public ApplicationOutputFormat Format => ApplicationOutputFormat.Text;

            public void Write(ApplicationOutput output)
            {
                Written.Add(output);
            }
        }

        private sealed class StubFailureHandler : IApplicationFailureHandler
        {
            public void HandleControlledCommandFailure(ApplicationCommandException exception)
            {
                throw exception ?? new ApplicationCommandException();
            }

            public void HandleUnhandledException(Exception exception)
            {
                throw exception ?? new InvalidOperationException();
            }

            public void ReportBackgroundException(Exception exception)
            {
                throw exception ?? new InvalidOperationException();
            }
        }

        private sealed class StubOutputCoordinator : IApplicationOutputCoordinator
        {
            public void WriteContacts(ApplicationListingOptions options, IEnumerable<Contact> contacts, Func<bool> askMore)
            {
            }

            public Task WriteMessagesAsync(string header,
                                           ApplicationListingOptions options,
                                           Func<int, Tuvi.Core.Entities.Message, Task<IEnumerable<Tuvi.Core.Entities.Message>>> source,
                                           Func<bool> askMore)
            {
                return Task.CompletedTask;
            }
        }

        private sealed class StubEmailAccountInputResolver : IEmailAccountInputResolver
        {
            public Task<Account> ResolveAsync()
            {
                throw new NotSupportedException();
            }
        }

        private sealed class StubCoreProvider : ITuviMailCoreProvider
        {
            public ITuviMail TuviMailCore => throw new NotSupportedException("The login must fail before the account is stored.");

            public Task ResetAsync()
            {
                throw new NotSupportedException();
            }
        }

        private sealed class StubHostApplicationLifetime : Microsoft.Extensions.Hosting.IHostApplicationLifetime
        {
            public CancellationToken ApplicationStarted => CancellationToken.None;

            public CancellationToken ApplicationStopping => CancellationToken.None;

            public CancellationToken ApplicationStopped => CancellationToken.None;

            public void StopApplication()
            {
            }
        }
    }
}
