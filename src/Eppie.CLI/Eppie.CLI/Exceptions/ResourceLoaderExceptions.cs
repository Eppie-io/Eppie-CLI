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

using System.Runtime.CompilerServices;

using Microsoft.Extensions.Localization;

namespace Eppie.CLI.Exceptions
{
    /// <summary>
    /// The exception that is thrown when resources cannot be loaded.
    /// </summary>
    public class ResourceLoaderException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceLoaderException" /> class with default properties.
        /// </summary>
        public ResourceLoaderException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceLoaderException" /> class with a specific message that describes the current exception.
        /// </summary>
        /// <param name="message">A message that describes the current exception.</param>
        public ResourceLoaderException(string message) : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceLoaderException" /> class with a specific message that describes the current exception, an inner exception.
        /// </summary>
        /// <param name="message">A message that describes the current exception.</param>
        /// <param name="innerException">The inner exception.</param>
        public ResourceLoaderException(string message, Exception innerException) : base(message, innerException)
        { }
    }

    /// <summary>
    /// The exception that is thrown when the specified name does not match any resource.
    /// </summary>
    public class ResourceNotFoundException : ResourceLoaderException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceNotFoundException" /> class with default properties.
        /// </summary>
        public ResourceNotFoundException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceNotFoundException" /> class with a specific message that describes the current exception.
        /// </summary>
        /// <param name="message">A message that describes the current exception.</param>
        public ResourceNotFoundException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceNotFoundException" /> class with a specific message that describes the current exception, an inner exception.
        /// </summary>
        /// <param name="message">A message that describes the current exception.</param>
        /// <param name="innerException">The inner exception.</param>
        public ResourceNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// The name of the resource that was attempted to be loaded.
        /// </summary>
        public string? Name { get; private set; }

        /// <summary>
        /// The location which was searched for a localization value.
        /// </summary>
        public string? SearchedLocation { get; private set; }

        /// <summary>
        /// Throws an exception if <paramref name="stringLocalizer"/> is null or its property <see cref="LocalizedString.ResourceNotFound"/> is true.
        /// </summary>
        /// <param name="stringLocalizer">The reference type argument to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stringLocalizer"/> is null.</exception>
        /// <exception cref="ResourceNotFoundException"><see cref="LocalizedString.ResourceNotFound"/> is true.</exception>
        public static void ThrowIfNotFound(LocalizedString stringLocalizer)
        {
            ArgumentNullException.ThrowIfNull(stringLocalizer);

            if (stringLocalizer.ResourceNotFound)
            {
                throw new ResourceNotFoundException($"Resource '{stringLocalizer.Name}' not found in location '{stringLocalizer.SearchedLocation ?? "unknown"}'.")
                {
                    Name = stringLocalizer.Name,
                    SearchedLocation = stringLocalizer.SearchedLocation
                };
            }
        }
    }

    /// <summary>
    /// The exception that is thrown when the specified assembly attribute is missing for this assembly.
    /// </summary>
    public class AssemblyAttributeMissedException : ResourceLoaderException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyAttributeMissedException" /> class with default properties.
        /// </summary>
        public AssemblyAttributeMissedException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyAttributeMissedException" /> class with a specific message that describes the current exception.
        /// </summary>
        /// <param name="message">A message that describes the current exception.</param>
        public AssemblyAttributeMissedException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyAttributeMissedException" /> class with a specific message that describes the current exception, an inner exception.
        /// </summary>
        /// <param name="message">A message that describes the current exception.</param>
        /// <param name="innerException">The inner exception.</param>
        public AssemblyAttributeMissedException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// The name of the attribute whose value was attempted to be retrieved.
        /// </summary>
        public string? AttributeName { get; private set; }

        /// <summary>Throws an exception if <paramref name="attribute"/> is null.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="attribute">The argument to validate as non-null.</param>
        /// <param name="attributeName">The name of the parameter with which <paramref name="attribute"/> corresponds.</param>
        /// <exception cref="AssemblyAttributeMissedException"><paramref name="attribute"/> is null.</exception>
        public static void ThrowIfMissed<T>(T? attribute, [CallerArgumentExpression(nameof(attribute))] string? attributeName = null)
        {
            if (attribute is null)
            {
                throw new AssemblyAttributeMissedException($"Assembly attribute '{attributeName ?? "unknown"}' can't be read.")
                {
                    AttributeName = attributeName
                };
            }
        }
    }
}
