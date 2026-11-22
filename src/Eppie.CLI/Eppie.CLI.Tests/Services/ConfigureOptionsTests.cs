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

using Eppie.CLI.Common;
using Eppie.CLI.Options;
using Eppie.CLI.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NUnit.Framework;

namespace Eppie.CLI.Tests.Services
{
    [TestFixture]
    public class ConfigureOptionsTests
    {
        [Test]
        public void ConfigureOptionsBindsGeneratedOptionsFromConfigurationAndCommandLine()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["non-interactive"] = "false",
                    ["Console:CultureInfo"] = "fr-FR",
                    ["MailOptions:Servers:Gmail:SMTP"] = "smtp.example.test",
                    ["MailOptions:Servers:Gmail:SMTPPort"] = "587",
                    ["Authorization:Gmail:RedirectUri"] = "https://localhost/callback",
                })
                .AddCommandLine(["--non-interactive=true"])
                .Build();

            using ServiceProvider provider = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .ConfigureOptions()
                .BuildServiceProvider();

            ApplicationLaunchOptions launch = provider
                .GetRequiredService<IOptions<ApplicationLaunchOptions>>().Value;
            ConsoleOptions console = provider
                .GetRequiredService<IOptions<ConsoleOptions>>().Value;
            MailOptions mail = provider
                .GetRequiredService<IOptions<MailOptions>>().Value;
            AuthorizationOptions authorization = provider
                .GetRequiredService<IOptions<AuthorizationOptions>>().Value;

            Assert.Multiple(() =>
            {
                Assert.That(launch.NonInteractive, Is.True); // command line overrides config
                Assert.That(console.CultureInfo.Name, Is.EqualTo("fr-FR"));
                Assert.That(mail.Servers[MailServer.Gmail].SMTP, Is.EqualTo("smtp.example.test"));
                Assert.That(mail.Servers[MailServer.Gmail].SMTPPort, Is.EqualTo(587));
                Assert.That(authorization.Gmail.RedirectUri, Is.EqualTo(new Uri("https://localhost/callback")));
                Assert.That(authorization.Gmail.ClientId, Is.EqualTo("<Gmail-ClientId>")); // default retained
            });
        }
    }
}
