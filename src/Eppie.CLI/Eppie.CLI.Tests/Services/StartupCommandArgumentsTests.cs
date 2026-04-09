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

using NUnit.Framework;

namespace Eppie.CLI.Tests.Services
{
    [TestFixture]
    public class StartupCommandArgumentsTests
    {
        [Test]
        public void GetStartupCommandArgumentsWhenArgumentsContainOnlyLeadingLaunchOptionsReturnsEmptyArray()
        {
            RawCommandLineArguments arguments = new([
                $"--{ApplicationLaunchOptions.NonInteractiveConfigurationKey}={TestConstants.True}",
                $"--{ApplicationLaunchOptions.OutputConfigurationKey}={TestConstants.Json}"
            ]);

            string[] result = StartupCommandArguments.GetStartupCommandArguments(arguments);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetStartupCommandArgumentsWhenDelimiterIsPresentReturnsArgumentsAfterDelimiter()
        {
            RawCommandLineArguments arguments = new([
                $"--{ApplicationLaunchOptions.NonInteractiveConfigurationKey}={TestConstants.False}",
                StartupCommandArguments.CommandDelimiter,
                MenuCommand.Open.Name
            ]);

            string[] result = StartupCommandArguments.GetStartupCommandArguments(arguments);

            Assert.That(result, Is.EqualTo([MenuCommand.Open.Name]));
        }

        [Test]
        public void GetStartupCommandArgumentsWhenDelimiterIsMissingReturnsEmptyArray()
        {
            RawCommandLineArguments arguments = new([
                TestConstants.ShowMessageCommand,
                $"--{ApplicationLaunchOptions.OutputConfigurationKey}",
                TestConstants.Json,
                TestConstants.AccountOption,
                TestConstants.AccountAddress
            ]);

            string[] result = StartupCommandArguments.GetStartupCommandArguments(arguments);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetStartupCommandArgumentsWhenDelimiterAppearsBeforeCommandPreservesRemainingArguments()
        {
            RawCommandLineArguments arguments = new([
                $"--{ApplicationLaunchOptions.OutputConfigurationKey}={TestConstants.Json}",
                StartupCommandArguments.CommandDelimiter,
                TestConstants.ShowMessageCommand,
                $"--{ApplicationLaunchOptions.OutputConfigurationKey}",
                TestConstants.Json,
                TestConstants.AccountOption,
                TestConstants.AccountAddress
            ]);

            string[] result = StartupCommandArguments.GetStartupCommandArguments(arguments);

            Assert.That(result, Is.EqualTo([
                TestConstants.ShowMessageCommand,
                $"--{ApplicationLaunchOptions.OutputConfigurationKey}",
                TestConstants.Json,
                TestConstants.AccountOption,
                TestConstants.AccountAddress]));
        }
    }
}
