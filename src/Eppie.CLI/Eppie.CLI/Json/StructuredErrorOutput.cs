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
    internal abstract record StructuredErrorOutput<TData>(string Code, string Message, TData Data) : StructuredOutput("error")
    { }

    internal abstract record StructuredErrorOutput(string Code, string Message) : StructuredErrorOutput<object?>(Code, Message, null)
    { }

    internal sealed record InteractiveStructuredErrorOutput : StructuredErrorOutput
    {
        private InteractiveStructuredErrorOutput(string message)
            : base("interactiveInputNotSupported", message)
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return ErrorJsonContext.Default.InteractiveStructuredErrorOutput;
        }
        public static InteractiveStructuredErrorOutput Create(string message)
        {
            ArgumentNullException.ThrowIfNull(message);
            return new InteractiveStructuredErrorOutput(message);
        }
    }

    internal sealed record StandardInputEndedStructuredErrorOutput : StructuredErrorOutput
    {
        private StandardInputEndedStructuredErrorOutput(string message)
            : base("standardInputEnded", message)
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return ErrorJsonContext.Default.StandardInputEndedStructuredErrorOutput;
        }
        public static StandardInputEndedStructuredErrorOutput Create(string message)
        {
            ArgumentNullException.ThrowIfNull(message);
            return new StandardInputEndedStructuredErrorOutput(message);
        }
    }

    internal sealed record ImpossibleInitializationStructuredErrorOutput : StructuredErrorOutput
    {
        private ImpossibleInitializationStructuredErrorOutput(string message)
            : base("impossibleInitialization", message)
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return ErrorJsonContext.Default.ImpossibleInitializationStructuredErrorOutput;
        }
        public static ImpossibleInitializationStructuredErrorOutput Create(string message)
        {
            ArgumentNullException.ThrowIfNull(message);
            return new ImpossibleInitializationStructuredErrorOutput(message);
        }
    }

    internal sealed record AuthorizationCanceledStructuredErrorOutput : StructuredErrorOutput
    {
        private AuthorizationCanceledStructuredErrorOutput(string message)
            : base("authorizationCanceled", message)
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return ErrorJsonContext.Default.AuthorizationCanceledStructuredErrorOutput;
        }
        public static AuthorizationCanceledStructuredErrorOutput Create(string message)
        {
            ArgumentNullException.ThrowIfNull(message);
            return new AuthorizationCanceledStructuredErrorOutput(message);
        }
    }

    internal sealed record OperationStructuredErrorOutput : StructuredErrorOutput<OperationStructuredErrorOutput.OperationData>
    {
        internal record OperationData(string Operation);

        private OperationStructuredErrorOutput(string message, OperationData data)
            : base("nonInteractiveOperationNotSupported", message, data)
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return ErrorJsonContext.Default.OperationStructuredErrorOutput;
        }
        public static OperationStructuredErrorOutput Create(string message,
                                                      NonInteractiveOperationNotSupportedErrorOutput output)
        {
            ArgumentNullException.ThrowIfNull(message);
            ArgumentNullException.ThrowIfNull(output);
            return new OperationStructuredErrorOutput(message, new OperationData(output.Operation));
        }
    }

    internal sealed record InputInvalidStructuredErrorOutput : StructuredErrorOutput<InputInvalidStructuredErrorOutput.InputInvalidData>
    {
        internal record InputInvalidData(string CommandName);
        private InputInvalidStructuredErrorOutput(string message, InputInvalidData data)
            : base("structuredStandardInputInvalidJson", message, data)
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return ErrorJsonContext.Default.InputInvalidStructuredErrorOutput;
        }
        public static InputInvalidStructuredErrorOutput Create(string message,
                                                     StructuredStandardInputInvalidJsonErrorOutput output)
        {
            ArgumentNullException.ThrowIfNull(message);
            ArgumentNullException.ThrowIfNull(output);
            return new InputInvalidStructuredErrorOutput(message, new InputInvalidData(output.CommandName));
        }
    }

    internal record MissingPropertyStructuredErrorOutput : StructuredErrorOutput<MissingPropertyStructuredErrorOutput.MissingPropertyData>
    {
        internal record MissingPropertyData(string CommandName, string PropertyName);
        private MissingPropertyStructuredErrorOutput(string message, MissingPropertyData data)
            : base("structuredStandardInputMissingProperty", message, data)
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return ErrorJsonContext.Default.MissingPropertyStructuredErrorOutput;
        }
        public static MissingPropertyStructuredErrorOutput Create(string message,
                                                        StructuredStandardInputMissingPropertyErrorOutput output)
        {
            ArgumentNullException.ThrowIfNull(message);
            ArgumentNullException.ThrowIfNull(output);
            return new MissingPropertyStructuredErrorOutput(message, new MissingPropertyData(output.CommandName,
                                                                                   output.PropertyName));
        }
    }

    internal record UnhandledExceptionStructuredErrorOutput : StructuredErrorOutput<UnhandledExceptionStructuredErrorOutput.ExceptionData>
    {
        internal record ExceptionData(string? ExceptionType);
        private UnhandledExceptionStructuredErrorOutput(string message, ExceptionData data)
            : base("unhandledException", message, data)
        { }
        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return ErrorJsonContext.Default.UnhandledExceptionStructuredErrorOutput;
        }
        public static UnhandledExceptionStructuredErrorOutput Create(string message,
                                                           UnhandledExceptionOutput output)
        {
            ArgumentNullException.ThrowIfNull(message);
            ArgumentNullException.ThrowIfNull(output);
            return new UnhandledExceptionStructuredErrorOutput(message, new ExceptionData(output.Exception.GetType().FullName));
        }
    }

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
                                 PropertyNameCaseInsensitive = true,
                                 NumberHandling = JsonNumberHandling.AllowReadingFromString)]
    [JsonSerializable(typeof(InteractiveStructuredErrorOutput))]
    [JsonSerializable(typeof(StandardInputEndedStructuredErrorOutput))]
    [JsonSerializable(typeof(ImpossibleInitializationStructuredErrorOutput))]
    [JsonSerializable(typeof(AuthorizationCanceledStructuredErrorOutput))]
    [JsonSerializable(typeof(OperationStructuredErrorOutput))]
    [JsonSerializable(typeof(InputInvalidStructuredErrorOutput))]
    [JsonSerializable(typeof(MissingPropertyStructuredErrorOutput))]
    [JsonSerializable(typeof(UnhandledExceptionStructuredErrorOutput))]
    internal partial class ErrorJsonContext : JsonSerializerContext
    { }
}
