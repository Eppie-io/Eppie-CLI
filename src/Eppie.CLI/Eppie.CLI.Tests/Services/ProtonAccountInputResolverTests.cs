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
    public class ProtonAccountInputResolverTests
    {
        private const string HumanVerificationToken = "captcha-token:solution";

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
        public async Task StructuredStandardInputWhenHumanVerificationTokenIsPresentReturnsIt()
        {
            IProtonAccountInput input = await ResolveStructuredInputAsync(
                $$"""{"email":"{{TestConstants.UserAddress}}","accountPassword":"secret","humanVerificationToken":"{{HumanVerificationToken}}"}""").ConfigureAwait(false);

            Assert.That(input.GetHumanVerificationToken(), Is.EqualTo(HumanVerificationToken));
        }

        [Test]
        public async Task StructuredStandardInputWhenHumanVerificationTokenIsMissingReportsMissingProperty()
        {
            IProtonAccountInput input = await ResolveStructuredInputAsync(
                $$"""{"email":"{{TestConstants.UserAddress}}","accountPassword":"secret"}""").ConfigureAwait(false);

            ApplicationCommandException? exception = Assert.Throws<ApplicationCommandException>(() => input.GetHumanVerificationToken());

            Assert.That(exception!.Output, Is.InstanceOf<StructuredStandardInputMissingPropertyErrorOutput>());
            Assert.That(((StructuredStandardInputMissingPropertyErrorOutput)exception.Output!).PropertyName,
                        Is.EqualTo(TestConstants.HumanVerificationTokenPropertyName));
        }

        [Test]
        public async Task LineBasedStandardInputCannotBeRetriedInNonInteractiveMode()
        {
            IProtonAccountInput input = await ResolveWithRedirectedInputAsync($"{TestConstants.UserAddress}{Environment.NewLine}secret{Environment.NewLine}",
                                                                              inputJsonFromStandardInput: false,
                                                                              nonInteractive: true).ConfigureAwait(false);

            Assert.That(input.SupportsRetry, Is.False);
        }

        private Task<IProtonAccountInput> ResolveStructuredInputAsync(string json)
        {
            return ResolveWithRedirectedInputAsync(json, inputJsonFromStandardInput: true, nonInteractive: true);
        }

        private async Task<IProtonAccountInput> ResolveWithRedirectedInputAsync(string input, bool inputJsonFromStandardInput, bool nonInteractive)
        {
            TextReader originalIn = Console.In;
            try
            {
                using StringReader reader = new(input);
                Console.SetIn(reader);

                ProtonAccountInputResolver resolver = new(CreateApplication(nonInteractive));
                return await resolver.ResolveAsync(inputJsonFromStandardInput).ConfigureAwait(false);
            }
            finally
            {
                Console.SetIn(originalIn);
            }
        }

        private Application CreateApplication(bool nonInteractive)
        {
            IStringLocalizer<Resources.Program> localizer = _serviceProvider.GetRequiredService<IStringLocalizer<Resources.Program>>();
            ResourceLoader resourceLoader = new(localizer);

            return new Application(NullLogger<Application>.Instance,
                                   new StubHostApplicationLifetime(),
                                   TestApplicationFactory.CreateLaunchOptionsOptions(nonInteractive: nonInteractive),
                                   new TextApplicationOutputWriter(resourceLoader),
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
