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

using Eppie.CLI.Entities;
using Eppie.CLI.Options;

using Finebits.Authorization.OAuth2.Abstractions;
using Finebits.Authorization.OAuth2.AuthenticationBroker;
using Finebits.Authorization.OAuth2.AuthenticationBroker.Abstractions;
using Finebits.Authorization.OAuth2.Google;
using Finebits.Authorization.OAuth2.Microsoft;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal class AuthorizationProvider(IServiceProvider serviceProvider)
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        internal IAuthorizationClient GetAuthorizationClient(MailService mailService)
        {
            return _serviceProvider.GetRequiredKeyedService<IAuthorizationClient>(mailService);
        }

        internal IRefreshable GetRefreshTokenClient(MailService mailService)
        {
            return _serviceProvider.GetRequiredKeyedService<IRefreshable>(mailService);
        }
    }

    internal static class AuthProviderDependencyInjectionExtension
    {
        private const string AuthHttpClientName = "OAuth2";

        internal static IServiceCollection AddAuthorizationProvider(this IServiceCollection services)
        {
            return DesktopAuthenticationBroker.IsSupported
                ? services.AddSingleton<AuthorizationProvider>()
                          .AddSingleton<IWebBrowserLauncher, WebBrowserLauncher>()
                          .AddTransient<IAuthenticationBroker, DesktopAuthenticationBroker>()
                          .AddKeyedTransient<IAuthorizationClient>(MailService.Gmail, (services, key) => CreateGoogleAuthClient(services))
                          .AddKeyedTransient<IAuthorizationClient>(MailService.Outlook, (services, key) => CreateOutlookAuthClient(services))
                          .AddKeyedTransient<IRefreshable>(MailService.Gmail, (services, key) => CreateGoogleAuthClient(services))
                          .AddKeyedTransient<IRefreshable>(MailService.Outlook, (services, key) => CreateOutlookAuthClient(services))
                : throw new InvalidOperationException("DesktopAuthenticationBroker is not supported");
        }

        private static GoogleAuthClient CreateGoogleAuthClient(IServiceProvider services)
        {
            AuthorizationOptions authOptions = services.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

            return new GoogleAuthClient(
                 httpClient: services.GetRequiredService<IHttpClientFactory>().CreateClient(AuthHttpClientName),
                 broker: services.GetRequiredService<IAuthenticationBroker>(),
                 config: new GoogleConfiguration
                 {
                     ClientId = authOptions.Gmail.ClientId,
                     ClientSecret = authOptions.Gmail.ClientSecret,
                     RedirectUri = authOptions.Gmail.RedirectUri ?? DesktopAuthenticationBroker.GetLoopbackUri(),
                     ScopeList = authOptions.Gmail.Scope
                 });
        }

        private static MicrosoftAuthClient CreateOutlookAuthClient(IServiceProvider services)
        {
            AuthorizationOptions authOptions = services.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

            return new MicrosoftAuthClient(
                 httpClient: services.GetRequiredService<IHttpClientFactory>().CreateClient(AuthHttpClientName),
                 broker: services.GetRequiredService<IAuthenticationBroker>(),
                 config: new MicrosoftConfiguration
                 {
                     ClientId = authOptions.Outlook.ClientId,
                     RedirectUri = authOptions.Outlook.RedirectUri ?? DesktopAuthenticationBroker.GetLoopbackUri(),
                     ScopeList = authOptions.Outlook.Scope
                 });
        }
    }
}
