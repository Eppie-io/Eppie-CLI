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

using Microsoft.Extensions.DependencyInjection;

namespace Eppie.CLI.Options
{
    internal static partial class ServiceCollectionExtensions
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection ConfigureOptions()
            {
                ArgumentNullException.ThrowIfNull(services, nameof(services));

                services.AddOptions<ApplicationLaunchOptions>()
                        .BindConfiguration(string.Empty);

                services.AddOptions<AuthorizationOptions>()
                        .BindConfiguration(AuthorizationOptions.SectionName);

                services.AddOptions<ConsoleOptions>()
                        .BindConfiguration(ConsoleOptions.SectionName);

                services.AddOptions<MailOptions>()
                        .BindConfiguration(MailOptions.SectionName);

                return services;
            }
        }
    }
}
