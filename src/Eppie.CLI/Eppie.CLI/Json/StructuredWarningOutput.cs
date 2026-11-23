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

namespace Eppie.CLI.Json
{
    internal abstract record StructuredWarningOutput(string Code, string Message) : StructuredOutput("warning")
    { }

    internal abstract record StructuredWarningOutput<TData>(string Code, string Message, TData Data)
        : StructuredWarningOutput(Code, Message)
    { }

    internal sealed record InvalidPasswordStructuredWarningOutput : StructuredWarningOutput
    {
        private InvalidPasswordStructuredWarningOutput(string message)
            : base("invalidPassword", message)
        { }

        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredWarningOutputJsonContext.Default.InvalidPasswordStructuredWarningOutput;
        }

        public static InvalidPasswordStructuredWarningOutput Create(string message)
        {
            ArgumentNullException.ThrowIfNull(message);
            return new InvalidPasswordStructuredWarningOutput(message);
        }
    }

    internal sealed record SecondInitializationStructuredWarningOutput : StructuredWarningOutput
    {
        private SecondInitializationStructuredWarningOutput(string message)
            : base("alreadyInitialized", message)
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredWarningOutputJsonContext.Default.SecondInitializationStructuredWarningOutput;
        }
        public static SecondInitializationStructuredWarningOutput Create(string message)
        {
            ArgumentNullException.ThrowIfNull(message);
            return new SecondInitializationStructuredWarningOutput(message);
        }
    }

    internal sealed record UninitializedAppStructuredWarningOutput : StructuredWarningOutput
    {
        private UninitializedAppStructuredWarningOutput(string message)
            : base("uninitialized", message)
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredWarningOutputJsonContext.Default.UninitializedAppStructuredWarningOutput;
        }
        public static UninitializedAppStructuredWarningOutput Create(string message)
        {
            ArgumentNullException.ThrowIfNull(message);
            return new UninitializedAppStructuredWarningOutput(message);
        }
    }

    internal sealed record UnsuccessfulAttemptStructuredWarningOutput : StructuredWarningOutput
    {
        private UnsuccessfulAttemptStructuredWarningOutput(string message)
            : base("unsuccessfulAttempt", message)
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredWarningOutputJsonContext.Default.UnsuccessfulAttemptStructuredWarningOutput;
        }
        public static UnsuccessfulAttemptStructuredWarningOutput Create(string message)
        {
            ArgumentNullException.ThrowIfNull(message);
            return new UnsuccessfulAttemptStructuredWarningOutput(message);
        }
    }

    internal sealed record UnknownFolderStructuredWarningOutput
        : StructuredWarningOutput<UnknownFolderStructuredWarningOutput.UnknownFolderData>
    {
        internal record UnknownFolderData(string Address, string Folder);

        private UnknownFolderStructuredWarningOutput(string message, UnknownFolderData data)
            : base("unknownFolder", message, data)
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredWarningOutputJsonContext.Default.UnknownFolderStructuredWarningOutput;
        }
        public static UnknownFolderStructuredWarningOutput Create(string message, UnknownFolderWarningOutput output)
        {
            ArgumentNullException.ThrowIfNull(message);
            ArgumentNullException.ThrowIfNull(output);

            return new UnknownFolderStructuredWarningOutput(message, new UnknownFolderData(output.Address,
                                                                                           output.Folder));
        }
    }

    internal record CommandData(string CommandName);

    internal sealed record CommandStructuredWarningOutput : StructuredWarningOutput<CommandData>
    {
        private CommandStructuredWarningOutput(string message, CommandData data)
            : base("commandRequiresAssumeYesInNonInteractiveMode", message, data)
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredWarningOutputJsonContext.Default.CommandStructuredWarningOutput;
        }
        public static CommandStructuredWarningOutput Create(string message, string commandName)
        {
            ArgumentNullException.ThrowIfNull(message);
            ArgumentNullException.ThrowIfNull(commandName);
            return new CommandStructuredWarningOutput(message, new CommandData(commandName));
        }
    }

    internal sealed record UnlockPasswordStructuredWarningOutput : StructuredWarningOutput<CommandData>
    {
        private UnlockPasswordStructuredWarningOutput(string message, CommandData data)
            : base("startupCommandRequiresUnlockPasswordFromStandardInput", message, data)
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredWarningOutputJsonContext.Default.UnlockPasswordStructuredWarningOutput;
        }
        public static UnlockPasswordStructuredWarningOutput Create(string message, string commandName)
        {
            ArgumentNullException.ThrowIfNull(message);
            ArgumentNullException.ThrowIfNull(commandName);
            return new UnlockPasswordStructuredWarningOutput(message, new CommandData(commandName));
        }
    }

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
                                 PropertyNameCaseInsensitive = true,
                                 NumberHandling = JsonNumberHandling.AllowReadingFromString)]
    [JsonSerializable(typeof(InvalidPasswordStructuredWarningOutput))]
    [JsonSerializable(typeof(SecondInitializationStructuredWarningOutput))]
    [JsonSerializable(typeof(UninitializedAppStructuredWarningOutput))]
    [JsonSerializable(typeof(UnsuccessfulAttemptStructuredWarningOutput))]
    [JsonSerializable(typeof(UnknownFolderStructuredWarningOutput))]
    [JsonSerializable(typeof(CommandStructuredWarningOutput))]
    [JsonSerializable(typeof(UnlockPasswordStructuredWarningOutput))]
    internal partial class StructuredWarningOutputJsonContext : JsonSerializerContext
    { }
}
