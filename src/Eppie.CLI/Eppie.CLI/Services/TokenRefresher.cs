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

using Eppie.CLI.Common;
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

            MailServer mailServer = GetMailServer(mailServiceName);

            IRefreshable refresher = _authorizationProvider.CreateRefreshTokenClient(mailServer);

            _logger.LogDebug("Refreshing token [hash code: {RefreshTokenHash}] (mail service: {MailServer})", refreshToken.GetHashCode(StringComparison.Ordinal), mailServer);
            AuthCredential fresh = await refresher.RefreshAsync(CreateToken(refreshToken), cancellationToken).ConfigureAwait(false);

            return new AuthToken()
            {
                AccessToken = fresh.AccessToken,
                RefreshToken = fresh.RefreshToken,
                ExpiresIn = fresh.ExpiresIn,
            };
        }

        private static MailServer GetMailServer(string mailServerName)
        {
            return Enum.TryParse(mailServerName, out MailServer mailServer) && mailServer != MailServer.Other
                ? mailServer
                : throw new AuthenticationException("Unsupported Mail service");
        }

        private static AuthCredential CreateToken(string refreshToken)
        {
            return new AuthCredential(tokenType: Credential.BearerType,
                                      accessToken: string.Empty,
                                      refreshToken: refreshToken,
                                      idToken: string.Empty,
                                      expiresIn: TimeSpan.Zero,
                                      scope: string.Empty);
        }
    }
}
