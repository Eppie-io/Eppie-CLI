// ---------------------------------------------------------------------------- //
//                                                                              //
//   Copyright 2023 Eppie(https://eppie.io)                                     //
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

using Microsoft.Extensions.Localization;
using System.Runtime.CompilerServices;

namespace Eppie.CLI.Exceptions
{
    public class ResourceLoaderException : Exception
    {
        public ResourceLoaderException()
        {
        }

        public ResourceLoaderException(string message) : base(message)
        {
        }

        public ResourceLoaderException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class ResourceNotFoundException : ResourceLoaderException
    {
        public ResourceNotFoundException()
        {
        }

        public ResourceNotFoundException(string message)
            : base(message)
        {
        }

        public ResourceNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public static void ThrowIfNotFound(LocalizedString stringLocalizer)
        {
            ArgumentNullException.ThrowIfNull(stringLocalizer);

            if (stringLocalizer.ResourceNotFound)
            {
                throw new ResourceNotFoundException($"Resource '{stringLocalizer.Name}' not found in location '{stringLocalizer.SearchedLocation}'.");
            }
        }
    }

    public class AssemblyAttributeMissedException : ResourceLoaderException
    {
        public AssemblyAttributeMissedException()
        {
        }

        public AssemblyAttributeMissedException(string message)
            : base(message)
        {
        }

        public AssemblyAttributeMissedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public static void ThrowIfMissed<T>(T? attribute, [CallerArgumentExpression(nameof(attribute))] string? attributeName = null)
        {
            if (attribute is null)
            {
                throw new AssemblyAttributeMissedException($"Assembly attribute '{attributeName}' can't be read.");
            }
        }
    }
}
