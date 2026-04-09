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
using Microsoft.Extensions.Options;

namespace Eppie.CLI.Tests.Services
{
    internal static class TestApplicationFactory
    {
        internal static IConfiguration CreateEmptyConfiguration()
        {
            return new ConfigurationBuilder().Build();
        }

        internal static IConfiguration CreateCommandLineConfiguration(params string[] arguments)
        {
            ArgumentNullException.ThrowIfNull(arguments);

            return arguments.Length > 0
                ? new ConfigurationBuilder().AddCommandLine(arguments).Build()
                : CreateEmptyConfiguration();
        }

        internal static IConfiguration CreateConfiguration(params (string Key, string? Value)[] values)
        {
            ArgumentNullException.ThrowIfNull(values);

            return values.Length > 0
                ? new ConfigurationBuilder().AddInMemoryCollection(values.Select(static pair => new KeyValuePair<string, string?>(pair.Key, pair.Value))).Build()
                : CreateEmptyConfiguration();
        }

        internal static RawCommandLineArguments CreateCommandLineArguments(params string[] arguments)
        {
            ArgumentNullException.ThrowIfNull(arguments);

            return new RawCommandLineArguments(arguments);
        }

        internal static ApplicationLaunchOptions CreateLaunchOptions(IConfiguration configuration, params string[] arguments)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(arguments);

            IConfigurationBuilder builder = new ConfigurationBuilder().AddConfiguration(configuration);

            if (arguments.Length > 0)
            {
                builder.AddCommandLine(arguments);
            }

            ApplicationLaunchOptions options = (ApplicationLaunchOptions)Activator.CreateInstance(typeof(ApplicationLaunchOptions), nonPublic: true)!;
            builder.Build().Bind(options, binderOptions => binderOptions.BindNonPublicProperties = true);
            return options;
        }

        internal static ApplicationLaunchOptions CreateLaunchOptionsFromCommandLine(params string[] arguments)
        {
            ArgumentNullException.ThrowIfNull(arguments);

            return CreateLaunchOptions(CreateCommandLineConfiguration(arguments), arguments);
        }

        internal static ApplicationLaunchOptions CreateLaunchOptionsFromConfiguration(params (string Key, string? Value)[] values)
        {
            ArgumentNullException.ThrowIfNull(values);

            return CreateLaunchOptions(CreateConfiguration(values));
        }

        internal static ApplicationLaunchOptions CreateLaunchOptions(bool unlockPasswordFromStandardInput = false, bool nonInteractive = false, bool assumeYes = false, string? output = null)
        {
            List<string> arguments = [];

            if (unlockPasswordFromStandardInput)
            {
                arguments.Add($"--{ApplicationLaunchOptions.UnlockPasswordFromStandardInputConfigurationKey}={TestConstants.True}");
            }

            if (nonInteractive)
            {
                arguments.Add($"--{ApplicationLaunchOptions.NonInteractiveConfigurationKey}={TestConstants.True}");
            }

            if (assumeYes)
            {
                arguments.Add($"--{ApplicationLaunchOptions.AssumeYesConfigurationKey}={TestConstants.True}");
            }

            if (output is not null)
            {
                arguments.Add($"--{ApplicationLaunchOptions.OutputConfigurationKey}={output}");
            }

            return CreateLaunchOptions(CreateEmptyConfiguration(), [.. arguments]);
        }

        internal static IOptions<ApplicationLaunchOptions> CreateLaunchOptionsOptions(bool unlockPasswordFromStandardInput = false, bool nonInteractive = false, bool assumeYes = false, string? output = null)
        {
            return Microsoft.Extensions.Options.Options.Create(CreateLaunchOptions(unlockPasswordFromStandardInput, nonInteractive, assumeYes, output));
        }

        internal static IOptions<ApplicationLaunchOptions> CreateLaunchOptionsOptions(ApplicationLaunchOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return Microsoft.Extensions.Options.Options.Create(options);
        }
    }
}
