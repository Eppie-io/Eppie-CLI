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

using Tuvi.Core.Entities;

namespace Eppie.CLI.Services
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is instantiated via dependency injection")]
    internal sealed class ApplicationOutputCoordinator(
        IApplicationPagingPolicy pagingPolicy,
        IApplicationOutputWriter outputWriter) : IApplicationOutputCoordinator
    {
        private readonly IApplicationPagingPolicy _pagingPolicy = pagingPolicy;
        private readonly IApplicationOutputWriter _outputWriter = outputWriter;

        public void WriteContacts(ApplicationListingOptions options, IEnumerable<Contact> contacts, Func<bool> askMore)
        {
            ArgumentNullException.ThrowIfNull(contacts);
            ArgumentNullException.ThrowIfNull(askMore);

            IReadOnlyCollection<Contact> remainingContacts = ApplyLimit(contacts, options.Limit);

            if (_pagingPolicy.ShouldAggregatePagesBeforeWrite)
            {
                _outputWriter.Write(new ContactsOutput(remainingContacts));
                return;
            }

            IReadOnlyCollection<Contact> page = [.. remainingContacts.Take(options.PageSize)];

            if (page.Count == 0)
            {
                _outputWriter.Write(new ContactsOutput([]));
                return;
            }

            do
            {
                _outputWriter.Write(new ContactsOutput(page));
                remainingContacts = [.. remainingContacts.Skip(page.Count)];
                page = [.. remainingContacts.Take(options.PageSize)];
            }
            while (_pagingPolicy.ShouldContinue(page.Count > 0, askMore));
        }

        public async Task WriteMessagesAsync(string header, ApplicationListingOptions options, Func<int, Message, Task<IEnumerable<Message>>> source, Func<bool> askMore)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(header);
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(askMore);

            List<Message> allMessages = [];
            Message lastItem = null!;
            bool anyPageProcessed = false;
            int remainingLimit = options.Limit;
            bool hasMore = false;

            while (true)
            {
                int requestedCount = options.GetRequestSize(remainingLimit);

                IReadOnlyCollection<Message> page = [.. (await source(requestedCount, lastItem).ConfigureAwait(false))
                    .Where(static item => item is not null)
                    .Take(requestedCount)];

                if (page.Count == 0 && anyPageProcessed)
                {
                    break;
                }

                foreach (Message item in page)
                {
                    lastItem = item;
                }

                if (_pagingPolicy.ShouldAggregatePagesBeforeWrite)
                {
                    allMessages.AddRange(page);
                }
                else
                {
                    _outputWriter.Write(new MessagesOutput(anyPageProcessed ? null : header, page, Paging: null));
                }

                anyPageProcessed = true;
                remainingLimit -= page.Count;

                bool pageFull = page.Count >= requestedCount;
                bool shouldStop = remainingLimit <= 0 || !_pagingPolicy.ShouldContinue(pageFull, askMore);

                if (shouldStop)
                {
                    hasMore = pageFull;
                    break;
                }
            }

            if (_pagingPolicy.ShouldAggregatePagesBeforeWrite)
            {
                _outputWriter.Write(new MessagesOutput(header, allMessages, Paging: new PagingInfo(allMessages.Count, hasMore)));
            }
        }

        private static IReadOnlyCollection<T> ApplyLimit<T>(IEnumerable<T> items, int limit)
        {
            ArgumentNullException.ThrowIfNull(items);

            return [.. items.Take(limit)];
        }
    }
}
