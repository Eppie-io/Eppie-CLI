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

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

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
        private const string DefaultLogFilePath = "./logs/default.log";

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "to log exceptions")]
        private static void Main(string[] args)
        {
            try
            {
                ConfigureDefaultLogger();

                Log.Information("Launching the application...");

                Host.CreateDefaultBuilder(args)
                    .UseContentRoot(AppContext.BaseDirectory)
                    .ConfigureServices(ConfigureServices)
                    .UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration), preserveStaticLogger: true)
                    .Build()
                    .Run();

                Log.Information("The application is shutting down...");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "The application terminated with an error.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(ctx);

            services.ConfigureOptions<ConsoleOptions>(ctx.Configuration, new BinderOptions { BindNonPublicProperties = true });
            services.ConfigureOptions<AuthorizationOptions>(ctx.Configuration, new BinderOptions { BindNonPublicProperties = true });

            services.AddLocalization()
                    .AddHttpClient()
                    .AddAuthorizationProvider()
                    .AddSingleton<ResourceLoader>()
                    .AddSingleton<CoreProvider>()
                    .AddSingleton<Application>()
                    .AddSingleton<IHostLifetime, ApplicationLifetime>()
                    .AddSingleton<MainMenu>()
                    .AddHostedService<ApplicationMenuLoop>();
        }

        private static void ConfigureDefaultLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Fatal,
                                 formatProvider: CultureInfo.InvariantCulture)
                .WriteTo.Debug(formatProvider: CultureInfo.InvariantCulture)
                .WriteTo.File(path: DefaultLogFilePath,
                              formatProvider: CultureInfo.InvariantCulture)
                .CreateLogger();
        }
    }
}
