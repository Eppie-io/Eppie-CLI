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

using System.Collections.Concurrent;
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
    internal class TokenResolver(ILogger<TokenResolver> logger,
                                 AuthorizationProvider authorizationProvider) : ITokenResolver, IDisposable
    {
        private readonly SemaphoreSlim _semaphore = new(1);
        private static readonly TimeSpan ReserveTime = TimeSpan.FromMinutes(1);

        private readonly ILogger<TokenResolver> _logger = logger;
        private readonly AuthorizationProvider _authorizationProvider = authorizationProvider;

        private readonly ConcurrentDictionary<EmailAddress, TokenData> _tokenStore = new();
        private bool _disposedValue;

        public void AddOrUpdateToken(EmailAddress emailAddress, string emailService, string refreshToken)
        {
            _logger.LogMethodCall();

            ArgumentNullException.ThrowIfNull(emailAddress);
            ArgumentException.ThrowIfNullOrWhiteSpace(emailService);
            ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

            MailService mailService = GetMailService(emailService, emailAddress);

            TokenData data = CreateTokenData(mailService, refreshToken);

            _tokenStore.AddOrUpdate(
                key: emailAddress,
                addValue: data,
                updateValueFactory: (key, oldData) => oldData.Token.RefreshToken.Equals(data.Token.RefreshToken, StringComparison.Ordinal) ? oldData : data);
        }

        public async Task<(string accessToken, string refreshToken)> GetAccessTokenAsync(EmailAddress emailAddress, CancellationToken cancellationToken = default)
        {
            _logger.LogMethodCall();
            ArgumentNullException.ThrowIfNull(emailAddress);

            TokenData data = FindData(emailAddress);
            AuthorizationToken result = data.Token;

            if (data.ExpireTime < DateTime.UtcNow + ReserveTime)
            {
                result = await RefreshAsync(emailAddress, cancellationToken).ConfigureAwait(false);
            }

            return (result.AccessToken, result.RefreshToken);
        }

        private async Task<AuthorizationToken> RefreshAsync(EmailAddress emailAddress, CancellationToken cancellationToken = default)
        {
            _logger.LogMethodCall();
            ArgumentNullException.ThrowIfNull(emailAddress);

            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                TokenData data = FindData(emailAddress);

                if (data.ExpireTime < DateTime.UtcNow + ReserveTime)
                {
                    AuthorizationToken token = data.Token;

                    IRefreshable refresher = _authorizationProvider.CreateRefreshTokenClient(data.MailService);
                    AuthorizationToken freshToken = await refresher.RefreshTokenAsync(token, cancellationToken).ConfigureAwait(false);
                    token.Update(freshToken);

                    TokenData newData = CreateTokenData(data.MailService, token);

                    _tokenStore.AddOrUpdate(emailAddress, newData, (key, oldData) => newData);

                    return token;
                }

                return data.Token;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private TokenData FindData(EmailAddress emailAddress)
        {
            return _tokenStore.TryGetValue(emailAddress, out TokenData? data)
                ? data
                : throw new AuthenticationException(emailAddress, "The token is not stored", null);
        }

        private static MailService GetMailService(string emailService, EmailAddress emailAddress)
        {
            return Enum.TryParse(emailService, out MailService mailService) && mailService != MailService.Other
                ? mailService
                : throw new AuthenticationException(emailAddress, "Unsupported Mail service", null);
        }

        private static TokenData CreateTokenData(MailService mailService, AuthorizationToken token)
        {
            ArgumentNullException.ThrowIfNull(token);

            return new()
            {
                MailService = mailService,
                ExpireTime = DateTime.UtcNow + token.ExpiresIn,
                Token = token
            };
        }

        private static TokenData CreateTokenData(MailService mailService, string refreshToken)
        {
            return new()
            {
                MailService = mailService,
                ExpireTime = DateTime.MinValue,
                Token = new(accessToken: string.Empty,
                    refreshToken: refreshToken,
                    tokenType: Token.BearerType,
                    expiresIn: TimeSpan.Zero,
                    scope: string.Empty)
            };
        }

        protected class TokenData
        {
            internal required MailService MailService { get; set; }
            internal required DateTime ExpireTime { get; set; }
            internal required AuthorizationToken Token { get; set; }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _semaphore.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null

                _tokenStore.Clear();
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
