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

using Microsoft.Extensions.Configuration;

using NUnit.Framework;

namespace Eppie.CLI.Tests.Services
{
    [TestFixture]
    public class ApplicationLaunchOptionsTests
    {
        [Test]
        public void ConstructorWhenLaunchOptionsAreMissingUsesDefaults()
        {
            ApplicationLaunchOptions options = TestApplicationFactory.CreateLaunchOptionsFromConfiguration();

            Assert.Multiple(() =>
            {
                Assert.That(options.NonInteractive, Is.False);
                Assert.That(options.OutputFormat, Is.EqualTo(ApplicationOutputFormat.Text));
                Assert.That(options.AssumeYes, Is.False);
                Assert.That(options.UnlockPasswordFromStandardInput, Is.False);
            });
        }

        [Test]
        public void ConstructorWhenOutputIsProvidedAsSeparateJsonCommandLineOptionEnablesJsonOutput()
        {
            ApplicationLaunchOptions options = TestApplicationFactory.CreateLaunchOptionsFromCommandLine("--output", TestConstants.Json);

            Assert.That(options.OutputFormat, Is.EqualTo(ApplicationOutputFormat.Json));
        }

        [Test]
        public void ConstructorWhenOutputIsProvidedAsEqualsJsonCommandLineOptionEnablesJsonOutput()
        {
            ApplicationLaunchOptions options = TestApplicationFactory.CreateLaunchOptionsFromCommandLine("--output=json");

            Assert.That(options.OutputFormat, Is.EqualTo(ApplicationOutputFormat.Json));
        }

        [Test]
        public void ConstructorWhenNonInteractiveIsProvidedWithExplicitTrueValueEnablesNonInteractive()
        {
            ApplicationLaunchOptions options = TestApplicationFactory.CreateLaunchOptionsFromCommandLine($"--{ApplicationLaunchOptions.NonInteractiveConfigurationKey}={TestConstants.True}");

            Assert.That(options.NonInteractive, Is.True);
        }

        [Test]
        public void ConstructorWhenAssumeYesIsProvidedWithExplicitTrueValueEnablesAssumeYes()
        {
            ApplicationLaunchOptions options = TestApplicationFactory.CreateLaunchOptionsFromCommandLine($"--{ApplicationLaunchOptions.AssumeYesConfigurationKey}={TestConstants.True}");

            Assert.That(options.AssumeYes, Is.True);
        }

        [TestCase(TestConstants.True)]
        [TestCase(TestConstants.TruePascal)]
        [TestCase(TestConstants.TrueUpper)]
        public void ConstructorWhenUnlockPasswordFromStandardInputIsPresentEnablesUnlockPasswordFromStandardInput(string optionValue)
        {
            ApplicationLaunchOptions options = TestApplicationFactory.CreateLaunchOptionsFromConfiguration((ApplicationLaunchOptions.UnlockPasswordFromStandardInputConfigurationKey, optionValue));

            Assert.That(options.UnlockPasswordFromStandardInput, Is.True);
        }

        [TestCase(TestConstants.True)]
        [TestCase(TestConstants.TruePascal)]
        [TestCase(TestConstants.TrueUpper)]
        public void ConstructorWhenConfigurationEnablesNonInteractiveReadsValueFromConfiguration(string optionValue)
        {
            ApplicationLaunchOptions options = TestApplicationFactory.CreateLaunchOptionsFromConfiguration((ApplicationLaunchOptions.NonInteractiveConfigurationKey, optionValue));

            Assert.That(options.NonInteractive, Is.True);
        }

        [TestCase(TestConstants.True)]
        [TestCase(TestConstants.TruePascal)]
        [TestCase(TestConstants.TrueUpper)]
        public void ConstructorWhenConfigurationEnablesAssumeYesReadsValueFromConfiguration(string optionValue)
        {
            ApplicationLaunchOptions options = TestApplicationFactory.CreateLaunchOptionsFromConfiguration((ApplicationLaunchOptions.AssumeYesConfigurationKey, optionValue));

            Assert.That(options.AssumeYes, Is.True);
        }

        [Test]
        public void ConstructorWhenConfigurationSetsOutputJsonReadsValueFromConfiguration()
        {
            ApplicationLaunchOptions options = TestApplicationFactory.CreateLaunchOptionsFromConfiguration((ApplicationLaunchOptions.OutputConfigurationKey, TestConstants.Json));

            Assert.That(options.OutputFormat, Is.EqualTo(ApplicationOutputFormat.Json));
        }

        [Test]
        public void ConstructorWhenUnlockPasswordFromStandardInputIsProvidedWithExplicitTrueValueEnablesUnlockPasswordFromStandardInput()
        {
            ApplicationLaunchOptions options = TestApplicationFactory.CreateLaunchOptionsFromCommandLine($"--{ApplicationLaunchOptions.UnlockPasswordFromStandardInputConfigurationKey}={TestConstants.True}");

            Assert.That(options.UnlockPasswordFromStandardInput, Is.True);
        }

        [Test]
        public void ConstructorWhenUnlockPasswordFromStandardInputIsProvidedWithExplicitFalseValueDisablesUnlockPasswordFromStandardInput()
        {
            ApplicationLaunchOptions options = TestApplicationFactory.CreateLaunchOptionsFromCommandLine($"--{ApplicationLaunchOptions.UnlockPasswordFromStandardInputConfigurationKey}={TestConstants.False}");

            Assert.That(options.UnlockPasswordFromStandardInput, Is.False);
        }

        [Test]
        public void ConstructorWhenCommandLineOverridesConfigurationCommandLineWins()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [ApplicationLaunchOptions.UnlockPasswordFromStandardInputConfigurationKey] = TestConstants.True,
                })
                .AddCommandLine([$"--{ApplicationLaunchOptions.UnlockPasswordFromStandardInputConfigurationKey}={TestConstants.False}"])
                .Build();

            ApplicationLaunchOptions options = TestApplicationFactory.CreateLaunchOptions(configuration);

            Assert.That(options.UnlockPasswordFromStandardInput, Is.False);
        }

        [Test]
        public void ConstructorWhenDuplicateBooleanOptionsAreProvidedLastCommandLineValueWins()
        {
            ApplicationLaunchOptions options = TestApplicationFactory.CreateLaunchOptionsFromCommandLine($"--{ApplicationLaunchOptions.AssumeYesConfigurationKey}={TestConstants.False}", $"--{ApplicationLaunchOptions.AssumeYesConfigurationKey}={TestConstants.True}");

            Assert.That(options.AssumeYes, Is.True);
        }

        [Test]
        public void ConstructorWhenDuplicateOutputOptionsAreProvidedLastCommandLineValueWins()
        {
            ApplicationLaunchOptions options = TestApplicationFactory.CreateLaunchOptionsFromCommandLine($"--{ApplicationLaunchOptions.OutputConfigurationKey}=text", $"--{ApplicationLaunchOptions.OutputConfigurationKey}=json");

            Assert.That(options.OutputFormat, Is.EqualTo(ApplicationOutputFormat.Json));
        }

        [Test]
        public void ConstructorWhenBooleanOptionHasInvalidCommandLineValueThrowsInvalidOperationException()
        {
            Assert.That(
                () => TestApplicationFactory.CreateLaunchOptionsFromCommandLine($"--{ApplicationLaunchOptions.AssumeYesConfigurationKey}=abc"),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void ConstructorWhenOutputOptionHasInvalidValueFallsBackToText()
        {
            ApplicationLaunchOptions options = TestApplicationFactory.CreateLaunchOptionsFromCommandLine($"--{ApplicationLaunchOptions.OutputConfigurationKey}=xml");

            Assert.That(options.OutputFormat, Is.EqualTo(ApplicationOutputFormat.Text));
        }

        [Test]
        public void ConstructorWhenOutputIsProvidedAsUppercaseJsonCommandLineOptionEnablesJsonOutput()
        {
            ApplicationLaunchOptions options = TestApplicationFactory.CreateLaunchOptionsFromCommandLine($"--{ApplicationLaunchOptions.OutputConfigurationKey}=JSON");

            Assert.That(options.OutputFormat, Is.EqualTo(ApplicationOutputFormat.Json));
        }

        [Test]
        public void ConstructorWhenAssumeYesIsProvidedWithExplicitFalseValueDisablesAssumeYes()
        {
            ApplicationLaunchOptions options = TestApplicationFactory.CreateLaunchOptionsFromCommandLine($"--{ApplicationLaunchOptions.AssumeYesConfigurationKey}={TestConstants.False}");

            Assert.That(options.AssumeYes, Is.False);
        }

        [Test]
        public void ConstructorWhenNonInteractiveIsProvidedWithSeparateFalseValueDisablesNonInteractive()
        {
            ApplicationLaunchOptions options = TestApplicationFactory.CreateLaunchOptionsFromCommandLine($"--{ApplicationLaunchOptions.NonInteractiveConfigurationKey}", TestConstants.False);

            Assert.That(options.NonInteractive, Is.False);
        }

        [Test]
        public void ConstructorWhenNonInteractiveIsProvidedWithSeparateTrueValueEnablesNonInteractive()
        {
            ApplicationLaunchOptions options = TestApplicationFactory.CreateLaunchOptionsFromCommandLine($"--{ApplicationLaunchOptions.NonInteractiveConfigurationKey}", TestConstants.True);

            Assert.That(options.NonInteractive, Is.True);
        }
    }
}
