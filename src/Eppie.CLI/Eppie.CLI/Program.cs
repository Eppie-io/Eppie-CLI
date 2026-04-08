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

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

#if DEBUG
using Eppie.CLI.Logging;
#endif
using Eppie.CLI.Menu;
using Eppie.CLI.Options;
using Eppie.CLI.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Serilog;
using Serilog.Enrichers.Sensitive;

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
                    .ConfigureServices((context, services) => ConfigureServices(context, services, args))
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

        private static void ConfigureServices(HostBuilderContext ctx, IServiceCollection services, string[] args)
        {
            ArgumentNullException.ThrowIfNull(ctx);

            services.ConfigureOptions<ConsoleOptions>(ctx.Configuration, new BinderOptions { BindNonPublicProperties = true });
            services.ConfigureOptions<MailOptions>(ctx.Configuration, new BinderOptions { BindNonPublicProperties = true });
            services.ConfigureOptions<AuthorizationOptions>(ctx.Configuration, new BinderOptions { BindNonPublicProperties = true });
            services.ConfigureRootOptions<ApplicationLaunchOptions>(ctx.Configuration, new BinderOptions { BindNonPublicProperties = true });

            services.AddLocalization()
                    .AddHttpClient()
                    .AddAuthorizationProvider()

                    .AddSingleton(new RawCommandLineArguments(args))

                    .AddSingleton<CoreProvider>()
                    .AddSingleton<ITuviMailCoreProvider>(serviceProvider => serviceProvider.GetRequiredService<CoreProvider>())
                    .AddSingleton<Application>()
                    .AddSingleton<IApplicationPasswordReader>(serviceProvider => serviceProvider.GetRequiredService<Application>())

                    .AddSingleton<ResourceLoader>()
                    .AddSingleton<TextApplicationOutputWriter>()
                    .AddSingleton<JsonApplicationOutputWriter>()
                    .AddSingleton<IApplicationOutputWriter>(serviceProvider => serviceProvider.GetRequiredService<IOptions<ApplicationLaunchOptions>>().Value.OutputFormat == ApplicationOutputFormat.Json
                         ? serviceProvider.GetRequiredService<JsonApplicationOutputWriter>()
                         : serviceProvider.GetRequiredService<TextApplicationOutputWriter>())
                    .AddSingleton<IApplicationPagingPolicy, ApplicationPagingPolicy>()
                    .AddSingleton<IApplicationOutputCoordinator, ApplicationOutputCoordinator>()
                    .AddSingleton<IApplicationFailureHandler, ApplicationFailureHandler>()
                    .AddSingleton<IEmailAccountInputResolver, EmailAccountInputResolver>()
                    .AddSingleton<IProtonAccountInputResolver, ProtonAccountInputResolver>()

                    .AddSingleton<IApplicationUnlocker, ApplicationUnlocker>()
                    .AddSingleton<IStartupCommandRunner, StartupCommandRunner>()

                    .AddSingleton<MainMenu>()
                    .AddSingleton<IApplicationMenu>(serviceProvider => serviceProvider.GetRequiredService<MainMenu>())

                    .AddSingleton<IHostLifetime, ApplicationLifetime>()
                    .AddHostedService<ApplicationMenuLoop>();
        }

        private static void ConfigureDefaultLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.WithSensitiveDataMasking(options =>
                {
                    options.Mode = MaskingMode.Globally;

#if DEBUG
                    options.MaskingOperators =
                    [
                        new HashTransformOperator<EmailAddressMaskingOperator>(),
                        new HashTransformOperator<IbanMaskingOperator>(),
                        new HashTransformOperator<CreditCardMaskingOperator>()
                    ];
#endif
                })
                .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Fatal,
                                 formatProvider: CultureInfo.InvariantCulture)
                .WriteTo.Debug(formatProvider: CultureInfo.InvariantCulture)
                .WriteTo.File(path: DefaultLogFilePath,
                              formatProvider: CultureInfo.InvariantCulture)
                .CreateLogger();
        }
    }
}
