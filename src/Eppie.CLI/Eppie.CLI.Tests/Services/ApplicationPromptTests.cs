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

using Eppie.CLI.Exceptions;
using Eppie.CLI.Options;
using Eppie.CLI.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

namespace Eppie.CLI.Tests.Services
{
    [TestFixture]
    public class ApplicationPromptTests
    {
        private const string PromptMarker = "Enter ";

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

        [Test]
        public void NonInteractiveTwoFactorCodeRequestReplacesThePromptWithAnAnnouncement()
        {
            (string output, string value) = ReadWithRedirectedInput("123456", application => application.AskTwoFactorCode(firstAttempt: true), outputJson: true);

            Assert.Multiple(() =>
            {
                Assert.That(value, Is.EqualTo("123456"));

                Assert.That(output, Does.Not.Contain(PromptMarker));
                Assert.That(output, Does.Contain("twoFactorCode"));
            });
        }

        [Test]
        public void NonInteractiveHumanVerificationTokenRequestReplacesThePromptWithAnAnnouncement()
        {
            (string output, string value) = ReadWithRedirectedInput("challenge:solution", application => application.AskHumanVerificationToken(), outputJson: true);

            Assert.Multiple(() =>
            {
                Assert.That(value, Is.EqualTo("challenge:solution"));
                Assert.That(output, Does.Not.Contain(PromptMarker));
                Assert.That(output, Does.Contain("humanVerificationToken"));
            });
        }

        [Test]
        public void InteractiveTwoFactorCodeRequestStillShowsThePrompt()
        {
            (string output, _) = ReadWithRedirectedInput("123456",
                                                         application => application.AskTwoFactorCode(firstAttempt: true),
                                                         nonInteractive: false);

            Assert.That(output, Does.Contain("two-factor"));
        }

        [TestCase("AskTwoFactorCode", TestName = "InteractiveJsonRunRefusesToReadTheTwoFactorCode")]
        [TestCase("AskHumanVerificationToken", TestName = "InteractiveJsonRunRefusesToReadTheVerificationToken")]
        public void InteractiveJsonRunRefusesToReadAValue(string prompt)
        {
            ApplicationCommandException? exception = Assert.Throws<ApplicationCommandException>(
                () => ReadWithRedirectedInput("123456",
                                              application => prompt == "AskTwoFactorCode"
                                                  ? application.AskTwoFactorCode(firstAttempt: true)
                                                  : application.AskHumanVerificationToken(),
                                              nonInteractive: false,
                                              outputJson: true));

            Assert.That(exception!.Output, Is.InstanceOf<InteractiveInputNotSupportedErrorOutput>());
        }

        private (string Output, string Value) ReadWithRedirectedInput(string input,
                                                                      Func<Application, string> read,
                                                                      bool nonInteractive = true,
                                                                      bool outputJson = false)
        {
            ArgumentNullException.ThrowIfNull(read);

            TextReader originalIn = Console.In;
            try
            {
                using StringReader reader = new(input + Environment.NewLine);
                Console.SetIn(reader);

                string value = string.Empty;
                string output = TestConsole.CaptureOutput(() => value = read(CreateApplication(nonInteractive, outputJson)));

                return (output, value);
            }
            finally
            {
                Console.SetIn(originalIn);
            }
        }

        private Application CreateApplication(bool nonInteractive, bool outputJson = false)
        {
            IStringLocalizer<Resources.Program> localizer = _serviceProvider.GetRequiredService<IStringLocalizer<Resources.Program>>();
            ResourceLoader resourceLoader = new(localizer);

            IApplicationOutputWriter writer = outputJson
                ? new JsonApplicationOutputWriter(resourceLoader)
                : new TextApplicationOutputWriter(resourceLoader);

            return new Application(NullLogger<Application>.Instance,
                                   new StubHostApplicationLifetime(),
                                   TestApplicationFactory.CreateLaunchOptionsOptions(nonInteractive: nonInteractive),
                                   writer,
                                   Microsoft.Extensions.Options.Options.Create(new MailOptions()),
                                   resourceLoader);
        }

        private sealed class StubHostApplicationLifetime : IHostApplicationLifetime
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
