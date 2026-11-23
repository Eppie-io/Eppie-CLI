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
using System.Text.Json;

using Eppie.CLI.Exceptions;
using Eppie.CLI.Json;

using Tuvi.Core.Entities;

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal sealed class EmailAccountInputResolver(Application application) : IEmailAccountInputResolver
    {
        private const string AddAccountCommandName = "add-account";

        private readonly Application _application = application;

        public async Task<Account> ResolveAsync()
        {
            string json = await _application.ReadStandardInputToEndAsync().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(json))
            {
                ThrowStructuredStandardInputInvalidJsonError();
            }

            EmailStructuredStandardInput? structuredInput;

            try
            {
                structuredInput = JsonSerializer.Deserialize(json, StructuredStandardInput.Default.EmailStructuredStandardInput);
            }
            catch (JsonException ex)
            {
                throw new ApplicationCommandException(new StructuredStandardInputInvalidJsonErrorOutput(AddAccountCommandName), innerException: ex);
            }

            if (structuredInput is null)
            {
                ThrowStructuredStandardInputInvalidJsonError();
            }

            EmailStructuredStandardInput validatedStructuredInput = structuredInput;

            EnsureStructuredInputPropertyIsPresent(validatedStructuredInput.Email, InputName.Email);
            EnsureStructuredInputPropertyIsPresent(validatedStructuredInput.AccountPassword, InputName.AccountPassword);
            EnsureStructuredInputPropertyIsPresent(validatedStructuredInput.ImapServer, InputName.ImapServer);
            EnsureStructuredInputPropertyIsPresent(validatedStructuredInput.ImapPort, InputName.ImapPort);
            EnsureStructuredInputPropertyIsPresent(validatedStructuredInput.SmtpServer, InputName.SmtpServer);
            EnsureStructuredInputPropertyIsPresent(validatedStructuredInput.SmtpPort, InputName.SmtpPort);

            Account account = Account.Default;
            account.Type = MailBoxType.Email;
            account.Email = new EmailAddress(validatedStructuredInput.Email);
            account.AuthData = new BasicAuthData()
            {
                Password = validatedStructuredInput.AccountPassword
            };
            account.IncomingServerAddress = validatedStructuredInput.ImapServer;
            account.IncomingServerPort = validatedStructuredInput.ImapPort.Value;
            account.IncomingMailProtocol = MailProtocol.IMAP;
            account.OutgoingServerAddress = validatedStructuredInput.SmtpServer;
            account.OutgoingServerPort = validatedStructuredInput.SmtpPort.Value;
            account.OutgoingMailProtocol = MailProtocol.SMTP;

            return account;
        }

        private static void EnsureStructuredInputPropertyIsPresent([NotNull] string? value, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ApplicationCommandException(new StructuredStandardInputMissingPropertyErrorOutput(AddAccountCommandName, propertyName));
            }
        }

        private static void EnsureStructuredInputPropertyIsPresent([NotNull] int? value, string propertyName)
        {
            if (!value.HasValue || value.Value <= 0)
            {
                throw new ApplicationCommandException(new StructuredStandardInputMissingPropertyErrorOutput(AddAccountCommandName, propertyName));
            }
        }

        [DoesNotReturn]
        private static void ThrowStructuredStandardInputInvalidJsonError()
        {
            throw new ApplicationCommandException(new StructuredStandardInputInvalidJsonErrorOutput(AddAccountCommandName));
        }
    }
}
