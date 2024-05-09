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
using Eppie.CLI.Tools;

using Finebits.Authorization.OAuth2.Abstractions;
using Finebits.Authorization.OAuth2.Types;

using Microsoft.Extensions.Logging;

using Tuvi.Core;
using Tuvi.Core.Entities;

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal class TokenRefresher(ILogger<TokenRefresher> logger,
                                 AuthorizationProvider authorizationProvider) : ITokenRefresher
    {
        private readonly ILogger<TokenRefresher> _logger = logger;
        private readonly AuthorizationProvider _authorizationProvider = authorizationProvider;

        public async Task<AuthToken> RefreshTokenAsync(string mailServiceName, string refreshToken, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(mailServiceName);
            ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

            _logger.LogMethodCall();

            MailService mailService = GetMailService(mailServiceName);

            IRefreshable refresher = _authorizationProvider.CreateRefreshTokenClient(mailService);

            _logger.LogDebug("Refreshing token [hash code: {RefreshTokenHash}] (mail service: {MailService})", refreshToken.GetHashCode(StringComparison.Ordinal), mailService);
            AuthorizationToken freshToken = await refresher.RefreshTokenAsync(CreateToken(refreshToken), cancellationToken).ConfigureAwait(false);

            return new AuthToken()
            {
                AccessToken = freshToken.AccessToken,
                RefreshToken = freshToken.RefreshToken,
                ExpiresIn = freshToken.ExpiresIn,
            };
        }

        private static MailService GetMailService(string mailServiceName)
        {
            return Enum.TryParse(mailServiceName, out MailService mailService) && mailService != MailService.Other
                ? mailService
                : throw new AuthenticationException("Unsupported Mail service");
        }

        private static AuthorizationToken CreateToken(string refreshToken)
        {
            return new AuthorizationToken(accessToken: string.Empty,
                                          refreshToken: refreshToken,
                                          tokenType: Token.BearerType,
                                          expiresIn: TimeSpan.Zero,
                                          scope: string.Empty);
        }
    }
}
