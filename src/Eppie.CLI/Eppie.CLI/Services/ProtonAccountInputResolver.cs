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

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal sealed class ProtonAccountInputResolver(Application application) : IProtonAccountInputResolver
    {
        private const string AddAccountCommandName = "add-account";

        private readonly Application _application = application;

        public async Task<IProtonAccountInput> ResolveAsync(bool inputJsonFromStandardInput)
        {
            return inputJsonFromStandardInput
                ? await ReadStructuredStandardInputAsync().ConfigureAwait(false)
                : ReadInteractiveInput();
        }

        private IProtonAccountInput ReadInteractiveInput()
        {
            return new InteractiveProtonAccountInput(_application.AskAccountAddress(), _application.AskAccountPassword(), _application);
        }

        private async Task<IProtonAccountInput> ReadStructuredStandardInputAsync()
        {
            string json = await _application.ReadStandardInputToEndAsync().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(json))
            {
                ThrowStructuredStandardInputInvalidJsonError();
            }

            ProtonStructuredStandardInput? structuredInput;

            try
            {
                structuredInput = JsonSerializer.Deserialize(json, StructuredStandardInput.Default.ProtonStructuredStandardInput);
            }
            catch (JsonException ex)
            {
                throw new ApplicationCommandException(new StructuredStandardInputInvalidJsonErrorOutput(AddAccountCommandName), innerException: ex);
            }

            if (structuredInput is null)
            {
                ThrowStructuredStandardInputInvalidJsonError();
            }

            ProtonStructuredStandardInput validatedStructuredInput = structuredInput;

            EnsureStructuredInputPropertyIsPresent(validatedStructuredInput.Email, InputName.Email);
            EnsureStructuredInputPropertyIsPresent(validatedStructuredInput.AccountPassword, InputName.AccountPassword);

            return new StructuredStandardInputProtonAccountInput(validatedStructuredInput);
        }

        private static void EnsureStructuredInputPropertyIsPresent([NotNull] string? value, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ApplicationCommandException(new StructuredStandardInputMissingPropertyErrorOutput(AddAccountCommandName, propertyName));
            }
        }

        private static string GetRequiredStructuredInputPropertyValue([NotNull] string? value, string propertyName)
        {
            EnsureStructuredInputPropertyIsPresent(value, propertyName);
            return value;
        }

        [DoesNotReturn]
        private static void ThrowStructuredStandardInputInvalidJsonError()
        {
            throw new ApplicationCommandException(new StructuredStandardInputInvalidJsonErrorOutput(AddAccountCommandName));
        }

        private sealed class InteractiveProtonAccountInput(string email, string accountPassword, Application application) : IProtonAccountInput
        {
            private readonly Application _application = application;

            public bool SupportsRetry => _application.CanAskForNewValues;

            public string Email { get; } = email;

            public string AccountPassword { get; } = accountPassword;

            public string GetTwoFactorCode(bool firstAttempt)
            {
                return _application.AskTwoFactorCode(firstAttempt);
            }

            public string GetMailboxPassword(bool firstAttempt)
            {
                return _application.AskMailboxPassword(firstAttempt);
            }

            public string GetHumanVerificationToken()
            {
                return _application.AskHumanVerificationToken();
            }
        }

        private sealed class StructuredStandardInputProtonAccountInput(ProtonStructuredStandardInput input) : IProtonAccountInput
        {
            private readonly ProtonStructuredStandardInput _input = input;

            public bool SupportsRetry => false;

            public string Email => GetRequiredStructuredInputPropertyValue(_input.Email, InputName.Email);

            public string AccountPassword => GetRequiredStructuredInputPropertyValue(_input.AccountPassword, InputName.AccountPassword);

            public string GetTwoFactorCode(bool firstAttempt)
            {
                return GetRequiredStructuredInputPropertyValue(_input.TwoFactorCode, InputName.TwoFactorCode);
            }

            public string GetMailboxPassword(bool firstAttempt)
            {
                return GetRequiredStructuredInputPropertyValue(_input.MailboxPassword, InputName.MailboxPassword);
            }

            public string GetHumanVerificationToken()
            {
                return GetRequiredStructuredInputPropertyValue(_input.HumanVerificationToken, InputName.HumanVerificationToken);
            }
        }
    }
}
