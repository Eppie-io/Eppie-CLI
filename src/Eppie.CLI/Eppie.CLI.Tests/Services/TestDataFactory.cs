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

using Tuvi.Core.Entities;

namespace Eppie.CLI.Tests.Services
{
    internal static class TestDataFactory
    {
        internal static Contact CreateContact(int id)
        {
            return new Contact($"Contact {id}", new EmailAddress($"{TestConstants.ContactAddressPrefix}{id}{TestConstants.ContactAddressDomain}"))
            {
                Id = id,
                UnreadCount = id,
            };
        }

        internal static Message CreateMessage(uint id = 1, string subject = TestConstants.Subject1)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(subject);

            Message message = new()
            {
                Id = id,
                Pk = (int)id,
                Date = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero),
                Folder = new Folder(TestConstants.Inbox, FolderAttributes.Inbox),
                Subject = subject,
                TextBody = TestConstants.TextBody,
            };

            message.To.Add(new EmailAddress(TestConstants.ToAddress));
            message.From.Add(new EmailAddress(TestConstants.FromAddress));

            return message;
        }

        internal static Folder CreateFolder(string fullName, FolderAttributes attributes, int unreadCount, int totalCount)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

            return new Folder(fullName, attributes)
            {
                UnreadCount = unreadCount,
                TotalCount = totalCount,
            };
        }
    }
}
