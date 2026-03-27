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

namespace Eppie.CLI.Services
{
    internal readonly record struct ApplicationListingOptions
    {
        internal const int DefaultPageSize = 20;
        internal const int DefaultLimit = 20;

        public ApplicationListingOptions(int pageSize, int? limit)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

            if (limit.HasValue)
            {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit.Value, nameof(limit));
            }

            PageSize = pageSize;
            Limit = limit ?? DefaultLimit;
        }

        internal int PageSize { get; }

        internal int Limit { get; }

        internal int GetRequestSize(int remainingLimit)
        {
            return Math.Min(PageSize, remainingLimit);
        }
    }
}
