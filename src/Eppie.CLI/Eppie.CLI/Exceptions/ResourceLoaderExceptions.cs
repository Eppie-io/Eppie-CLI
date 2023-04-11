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

namespace Eppie.CLI.Exceptions
{
    internal class ResourceNotFoundException : ArgumentException
    {
        public ResourceNotFoundException()
        {
        }

        public ResourceNotFoundException(string? message)
            : base(message)
        {
        }

        public ResourceNotFoundException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        public ResourceNotFoundException(string? message, string? paramName, Exception? innerException)
            : base(message, paramName, innerException)
        {
        }

        public ResourceNotFoundException(string? message, string? paramName)
            : base(message, paramName)
        {
        }

        public static void ThrowIfResourceNotFound(LocalizedString stringLocalizer, string? paramName = null)
        {
            if (stringLocalizer.ResourceNotFound)
            {
                throw new ResourceNotFoundException($"Resource '{stringLocalizer.Name}' not found in location '{stringLocalizer.SearchedLocation}'.", paramName);
            }
        }
    }
}
