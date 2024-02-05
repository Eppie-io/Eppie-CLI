// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2024 Eppie (https://eppie.io)                                    //
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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eppie.CLI.Options
{
    internal interface IConfigurationSectionOptions
    {
        string SectionName { get; }
    }

    internal static class ConfigurationSectionOptionsExtension
    {
        public static void Configure<TOptions>(this IServiceCollection services, IConfiguration configuration)
            where TOptions : class, IConfigurationSectionOptions
        {
            services.Configure<TOptions>(configureOptions => configuration.GetSection(configureOptions.SectionName));
        }

        public static void Configure<TOptions>(this IServiceCollection services, IConfiguration configuration, BinderOptions binderOptions)
            where TOptions : class, IConfigurationSectionOptions
        {
            static void Clone(BinderOptions options, BinderOptions other)
            {
                options.ErrorOnUnknownConfiguration = other.ErrorOnUnknownConfiguration;
                options.BindNonPublicProperties = other.BindNonPublicProperties;
            }

            services.Configure<TOptions>(options => configuration.GetSection(options.SectionName).Bind(options, opt => Clone(opt, binderOptions)));
        }
    }
}
