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

using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using Eppie.CLI.Services;

using static Eppie.CLI.Json.AuthorizationStartedStructuredStatusOutput;
using static Eppie.CLI.Json.InitializedStructuredStatusOutput;

namespace Eppie.CLI.Json
{
    internal abstract record StructuredStatusOutput(string Code) : StructuredOutput("status")
    { }

    internal abstract record StructuredStatusOutput<TData>(string Code, TData Data) : StructuredStatusOutput(Code)
    { }

    internal sealed record InitializedStructuredStatusOutput : StructuredStatusOutput<InitializedData>
    {
        internal record InitializedData(IEnumerable<string> SeedPhrase);

        private InitializedStructuredStatusOutput(InitializedData data)
            : base("initialized", data)
        { }

        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredStatusOutputJsonContext.Default.InitializedStructuredStatusOutput;
        }

        public static InitializedStructuredStatusOutput Create(ApplicationInitializedOutput initializedOutput)
        {
            ArgumentNullException.ThrowIfNull(initializedOutput);
            return new InitializedStructuredStatusOutput(new InitializedData(initializedOutput.SeedPhrase));
        }
    }

    internal sealed record ResetStructuredStatusOutput : StructuredStatusOutput
    {
        private ResetStructuredStatusOutput()
            : base("reset")
        { }

        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredStatusOutputJsonContext.Default.ResetStructuredStatusOutput;
        }
        public static ResetStructuredStatusOutput Create()
        {
            return new ResetStructuredStatusOutput();
        }
    }

    internal sealed record OpenedStructuredStatusOutput : StructuredStatusOutput
    {
        private OpenedStructuredStatusOutput()
            : base("opened")
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredStatusOutputJsonContext.Default.OpenedStructuredStatusOutput;
        }
        public static OpenedStructuredStatusOutput Create()
        {
            return new OpenedStructuredStatusOutput();
        }
    }

    internal sealed record RestoredStructuredStatusOutput : StructuredStatusOutput
    {
        private RestoredStructuredStatusOutput()
            : base("restored")
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredStatusOutputJsonContext.Default.RestoredStructuredStatusOutput;
        }
        public static RestoredStructuredStatusOutput Create()
        {
            return new RestoredStructuredStatusOutput();
        }
    }

    internal sealed record AuthorizationStartedStructuredStatusOutput : StructuredStatusOutput<AuthorizationStartedData>
    {
        internal record AuthorizationStartedData(string ServiceName);

        private AuthorizationStartedStructuredStatusOutput(AuthorizationStartedData data)
            : base("authorizationStarted", data)
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredStatusOutputJsonContext.Default.AuthorizationStartedStructuredStatusOutput;
        }
        public static AuthorizationStartedStructuredStatusOutput Create(AuthorizationToServiceOutput authorizationToServiceOutput)
        {
            ArgumentNullException.ThrowIfNull(authorizationToServiceOutput);
            AuthorizationStartedData data = new(authorizationToServiceOutput.ServiceName);
            return new AuthorizationStartedStructuredStatusOutput(data);
        }
    }

    internal sealed record AuthorizationCompletedStructuredStatusOutput : StructuredStatusOutput
    {
        private AuthorizationCompletedStructuredStatusOutput()
            : base("authorizationCompleted")
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredStatusOutputJsonContext.Default.AuthorizationCompletedStructuredStatusOutput;
        }
        public static AuthorizationCompletedStructuredStatusOutput Create()
        {
            return new AuthorizationCompletedStructuredStatusOutput();
        }
    }

    internal sealed record MessageSentStructuredStatusOutput : StructuredStatusOutput<MessageSentStructuredStatusOutput.MessageSentData>
    {
        internal record MessageSentData(string Subject, string To, string From);

        private MessageSentStructuredStatusOutput(MessageSentData data)
            : base("messageSent", data)
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredStatusOutputJsonContext.Default.MessageSentStructuredStatusOutput;
        }
        public static MessageSentStructuredStatusOutput Create(MessageSentOutput messageSentOutput)
        {
            ArgumentNullException.ThrowIfNull(messageSentOutput);
            return new MessageSentStructuredStatusOutput(new MessageSentData(messageSentOutput.Subject,
                                                                             messageSentOutput.To,
                                                                             messageSentOutput.From));
        }
    }

    internal sealed record MessageDeletedStructuredStatusOutput : StructuredStatusOutput<MessageDeletedStructuredStatusOutput.MessageDeletedData>
    {
        internal record MessageDeletedData(string Account, string Folder, uint Id, int Pk);

        private MessageDeletedStructuredStatusOutput(MessageDeletedData data)
            : base("messageDeleted", data)
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredStatusOutputJsonContext.Default.MessageDeletedStructuredStatusOutput;
        }
        public static MessageDeletedStructuredStatusOutput Create(MessageDeletedOutput messageDeletedOutput)
        {
            ArgumentNullException.ThrowIfNull(messageDeletedOutput);
            return new MessageDeletedStructuredStatusOutput(new MessageDeletedData(messageDeletedOutput.AccountAddress,
                                                                                   messageDeletedOutput.FolderName,
                                                                                   messageDeletedOutput.Id,
                                                                                   messageDeletedOutput.Pk));
        }
    }

    internal sealed record FolderSyncedStructuredStatusOutput : StructuredStatusOutput<FolderSyncedStructuredStatusOutput.FolderSyncedData>
    {
        internal record FolderSyncedData(string Account, string Folder);

        private FolderSyncedStructuredStatusOutput(FolderSyncedData data)
            : base("folderSynced", data)
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredStatusOutputJsonContext.Default.FolderSyncedStructuredStatusOutput;
        }
        public static FolderSyncedStructuredStatusOutput Create(FolderSyncedOutput folderSyncedOutput)
        {
            ArgumentNullException.ThrowIfNull(folderSyncedOutput);
            return new FolderSyncedStructuredStatusOutput(new FolderSyncedData(folderSyncedOutput.AccountAddress,
                                                                               folderSyncedOutput.FolderName));
        }
    }

    internal sealed record InputRequiredStructuredStatusOutput : StructuredStatusOutput<InputRequiredStructuredStatusOutput.InputRequiredData>
    {
        internal record InputRequiredData(string Input);

        private InputRequiredStructuredStatusOutput(InputRequiredData data)
            : base("inputRequired", data)
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredStatusOutputJsonContext.Default.InputRequiredStructuredStatusOutput;
        }
        public static InputRequiredStructuredStatusOutput Create(InputRequiredOutput inputRequiredOutput)
        {
            ArgumentNullException.ThrowIfNull(inputRequiredOutput);
            return new InputRequiredStructuredStatusOutput(new InputRequiredData(inputRequiredOutput.Input));
        }
    }

    internal sealed record HumanVerificationRequiredStructuredStatusOutput
        : StructuredStatusOutput<HumanVerificationRequiredStructuredStatusOutput.HumanVerificationRequiredData>
    {
        internal record HumanVerificationRequiredData(Uri VerificationUri, Uri HelpUri);

        private HumanVerificationRequiredStructuredStatusOutput(HumanVerificationRequiredData data)
            : base("humanVerificationRequired", data)
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredStatusOutputJsonContext.Default.HumanVerificationRequiredStructuredStatusOutput;
        }
        public static HumanVerificationRequiredStructuredStatusOutput Create(ProtonHumanVerificationRequiredOutput output)
        {
            ArgumentNullException.ThrowIfNull(output);
            return new HumanVerificationRequiredStructuredStatusOutput(new HumanVerificationRequiredData(
                                                                       output.VerificationUri,
                                                                       ProtonHumanVerificationRequiredOutput.HelpUri));
        }
    }

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
                                 PropertyNameCaseInsensitive = true,
                                 NumberHandling = JsonNumberHandling.AllowReadingFromString)]
    [JsonSerializable(typeof(InitializedStructuredStatusOutput))]
    [JsonSerializable(typeof(ResetStructuredStatusOutput))]
    [JsonSerializable(typeof(OpenedStructuredStatusOutput))]
    [JsonSerializable(typeof(RestoredStructuredStatusOutput))]
    [JsonSerializable(typeof(AuthorizationStartedStructuredStatusOutput))]
    [JsonSerializable(typeof(AuthorizationCompletedStructuredStatusOutput))]
    [JsonSerializable(typeof(MessageSentStructuredStatusOutput))]
    [JsonSerializable(typeof(MessageDeletedStructuredStatusOutput))]
    [JsonSerializable(typeof(FolderSyncedStructuredStatusOutput))]
    [JsonSerializable(typeof(InputRequiredStructuredStatusOutput))]
    [JsonSerializable(typeof(HumanVerificationRequiredStructuredStatusOutput))]
    internal partial class StructuredStatusOutputJsonContext : JsonSerializerContext
    { }
}
