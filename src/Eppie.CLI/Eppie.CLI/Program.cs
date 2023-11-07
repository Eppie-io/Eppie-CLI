// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2023 Eppie (https://eppie.io)                                    //
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
using Eppie.CLI.Options;
using Eppie.CLI.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace Eppie.CLI
{
    internal sealed class Program
    {
        public static IHost Host { get; private set; } = null!;

        private static void Main(string[] args)
        {
            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .UseContentRoot(AppContext.BaseDirectory)
                .ConfigureServices(ConfigureServices)
                .UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration))
                .UseConsoleLifetime(options => options.SuppressStatusMessages = true)
                .Build();

            Host.Run();

            Log.CloseAndFlush();
        }

        private static void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(ctx);

            services.Configure<ConsoleOptions>(ctx.Configuration, new BinderOptions { BindNonPublicProperties = true });

            services.AddLocalization()
                    .AddSingleton<ResourceLoader>()
                    .AddSingleton<CoreProvider>()
                    .AddSingleton<MainMenu>()
                    .AddSingleton<Application>()

                    .AddHostedService<ApplicationLoop>();
        }
    }
}
