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

using Eppie.CLI.Services;
using Eppie.CLI.Tests.TestDoubles;

using NUnit.Framework;

using Tuvi.Core.Entities;

namespace Eppie.CLI.Tests.Services
{
    [TestFixture]
    public class ApplicationOutputCoordinatorTests
    {
        [Test]
        public void WriteContactsWhenJsonOutputAggregatesAllContactsIntoSingleOutput()
        {
            FakeApplicationOutputWriter outputWriter = new();
            ApplicationOutputCoordinator coordinator = new(new ApplicationPagingPolicy(TestApplicationFactory.CreateLaunchOptionsOptions(output: "json")), outputWriter);
            List<Contact> contacts = [TestDataFactory.CreateContact(1), TestDataFactory.CreateContact(2), TestDataFactory.CreateContact(3)];
            bool askMoreCalled = false;

            coordinator.WriteContacts(new ApplicationListingOptions(2, null), contacts, () =>
            {
                askMoreCalled = true;
                return false;
            });

            Assert.Multiple(() =>
            {
                Assert.That(outputWriter.Outputs, Has.Count.EqualTo(1));
                Assert.That(askMoreCalled, Is.False);
                Assert.That(outputWriter.Outputs[0], Is.TypeOf<ContactsOutput>());
                Assert.That(((ContactsOutput)outputWriter.Outputs[0]).Contacts.Select(static contact => contact.Id), Is.EqualTo([1, 2, 3]));
            });
        }

        [Test]
        public void WriteContactsWhenNonInteractiveTextOutputWritesSinglePageWithoutPrompt()
        {
            FakeApplicationOutputWriter outputWriter = new();
            ApplicationOutputCoordinator coordinator = new(new ApplicationPagingPolicy(TestApplicationFactory.CreateLaunchOptionsOptions(nonInteractive: true)), outputWriter);
            List<Contact> contacts = [TestDataFactory.CreateContact(1), TestDataFactory.CreateContact(2), TestDataFactory.CreateContact(3), TestDataFactory.CreateContact(4), TestDataFactory.CreateContact(5)];
            bool askMoreCalled = false;

            coordinator.WriteContacts(new ApplicationListingOptions(2, null), contacts, () =>
            {
                askMoreCalled = true;
                return true;
            });

            Assert.Multiple(() =>
            {
                Assert.That(askMoreCalled, Is.False);
                Assert.That(outputWriter.Outputs, Has.Count.EqualTo(1));
                Assert.That(((ContactsOutput)outputWriter.Outputs[0]).Contacts.Select(static contact => contact.Id), Is.EqualTo([1, 2]));
            });
        }

        [Test]
        public void WriteContactsWhenLimitIsNotSpecifiedUsesDefaultLimit()
        {
            FakeApplicationOutputWriter outputWriter = new();
            ApplicationOutputCoordinator coordinator = new(new ApplicationPagingPolicy(TestApplicationFactory.CreateLaunchOptionsOptions(output: "json")), outputWriter);
            List<Contact> contacts = Enumerable.Range(1, 30).Select(TestDataFactory.CreateContact).ToList();

            coordinator.WriteContacts(new ApplicationListingOptions(50, null), contacts, () => true);

            Assert.That(((ContactsOutput)outputWriter.Outputs[0]).Contacts, Has.Count.EqualTo(ApplicationListingOptions.DefaultLimit));
        }

        [Test]
        public void WriteContactsWhenContactsAreEmptyWritesSingleEmptyOutput()
        {
            FakeApplicationOutputWriter outputWriter = new();
            ApplicationOutputCoordinator coordinator = new(new ApplicationPagingPolicy(TestApplicationFactory.CreateLaunchOptionsOptions()), outputWriter);
            bool askMoreCalled = false;

            coordinator.WriteContacts(new ApplicationListingOptions(2, null), [], () =>
            {
                askMoreCalled = true;
                return true;
            });

            Assert.Multiple(() =>
            {
                Assert.That(askMoreCalled, Is.False);
                Assert.That(outputWriter.Outputs, Has.Count.EqualTo(1));
                Assert.That(((ContactsOutput)outputWriter.Outputs[0]).Contacts, Is.Empty);
            });
        }

        [Test]
        public void WriteContactsWhenInteractiveTextOutputAndAskMoreReturnsTrueWritesMultiplePages()
        {
            FakeApplicationOutputWriter outputWriter = new();
            ApplicationOutputCoordinator coordinator = new(new ApplicationPagingPolicy(TestApplicationFactory.CreateLaunchOptionsOptions()), outputWriter);
            List<Contact> contacts = [TestDataFactory.CreateContact(1), TestDataFactory.CreateContact(2), TestDataFactory.CreateContact(3)];
            int askMoreCallCount = 0;

            coordinator.WriteContacts(new ApplicationListingOptions(2, null), contacts, () =>
            {
                askMoreCallCount++;
                return true;
            });

            Assert.Multiple(() =>
            {
                Assert.That(askMoreCallCount, Is.EqualTo(1));
                Assert.That(outputWriter.Outputs, Has.Count.EqualTo(2));
                Assert.That(((ContactsOutput)outputWriter.Outputs[0]).Contacts.Select(static contact => contact.Id), Is.EqualTo([1, 2]));
                Assert.That(((ContactsOutput)outputWriter.Outputs[1]).Contacts.Select(static contact => contact.Id), Is.EqualTo([3]));
            });
        }

        [Test]
        public async Task WriteMessagesAsyncWhenJsonOutputAggregatesAllPagesAndWritesHeaderOnce()
        {
            FakeApplicationOutputWriter outputWriter = new();
            ApplicationOutputCoordinator coordinator = new(new ApplicationPagingPolicy(TestApplicationFactory.CreateLaunchOptionsOptions(output: "json")), outputWriter);
            Message firstMessage = TestDataFactory.CreateMessage(1, TestConstants.Subject1);
            Message secondMessage = TestDataFactory.CreateMessage(2, TestConstants.Subject2);
            Message thirdMessage = TestDataFactory.CreateMessage(3, TestConstants.Subject3);
            int sourceCallCount = 0;
            bool askMoreCalled = false;

            await coordinator.WriteMessagesAsync(TestConstants.Inbox, new ApplicationListingOptions(2, null), (pageSize, lastItem) =>
            {
                sourceCallCount++;

                return Task.FromResult<IEnumerable<Message>>(sourceCallCount switch
                {
                    1 => [firstMessage, secondMessage],
                    2 => [thirdMessage],
                    _ => []
                });
            }, () =>
            {
                askMoreCalled = true;
                return false;
            }).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(askMoreCalled, Is.False);
                Assert.That(outputWriter.Outputs, Has.Count.EqualTo(1));
                Assert.That(outputWriter.Outputs[0], Is.TypeOf<MessagesOutput>());

                MessagesOutput output = (MessagesOutput)outputWriter.Outputs[0];
                Assert.That(output.Header, Is.EqualTo(TestConstants.Inbox));
                Assert.That(output.Messages.Select(static message => message.Id), Is.EqualTo([1u, 2u, 3u]));
                Assert.That(output.Paging?.HasMore, Is.False);
            });
        }

        [Test]
        public async Task WriteMessagesAsyncWhenNonInteractiveTextOutputWritesSinglePageWithHeader()
        {
            FakeApplicationOutputWriter outputWriter = new();
            ApplicationOutputCoordinator coordinator = new(new ApplicationPagingPolicy(TestApplicationFactory.CreateLaunchOptionsOptions(nonInteractive: true)), outputWriter);
            Message firstMessage = TestDataFactory.CreateMessage(1, TestConstants.Subject1);
            Message secondMessage = TestDataFactory.CreateMessage(2, TestConstants.Subject2);
            Message thirdMessage = TestDataFactory.CreateMessage(3, TestConstants.Subject3);
            int sourceCallCount = 0;

            await coordinator.WriteMessagesAsync(TestConstants.Inbox, new ApplicationListingOptions(2, null), (pageSize, lastItem) =>
            {
                sourceCallCount++;

                return Task.FromResult<IEnumerable<Message>>(sourceCallCount switch
                {
                    1 => [firstMessage, secondMessage],
                    2 => [thirdMessage],
                    _ => []
                });
            }, () => true).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(outputWriter.Outputs, Has.Count.EqualTo(1));
                Assert.That(((MessagesOutput)outputWriter.Outputs[0]).Header, Is.EqualTo(TestConstants.Inbox));
                Assert.That(((MessagesOutput)outputWriter.Outputs[0]).Messages.Select(static message => message.Id), Is.EqualTo([1u, 2u]));
            });
        }

        [Test]
        public async Task WriteMessagesAsyncWhenInteractiveTextOutputWritesHeaderOnlyOnFirstPage()
        {
            FakeApplicationOutputWriter outputWriter = new();
            ApplicationOutputCoordinator coordinator = new(new ApplicationPagingPolicy(TestApplicationFactory.CreateLaunchOptionsOptions()), outputWriter);
            Message firstMessage = TestDataFactory.CreateMessage(1, TestConstants.Subject1);
            Message secondMessage = TestDataFactory.CreateMessage(2, TestConstants.Subject2);
            Message thirdMessage = TestDataFactory.CreateMessage(3, TestConstants.Subject3);
            int sourceCallCount = 0;
            int askMoreCallCount = 0;

            await coordinator.WriteMessagesAsync(TestConstants.Inbox, new ApplicationListingOptions(2, null), (pageSize, lastItem) =>
            {
                sourceCallCount++;

                return Task.FromResult<IEnumerable<Message>>(sourceCallCount switch
                {
                    1 => [firstMessage, secondMessage],
                    2 => [thirdMessage],
                    _ => []
                });
            }, () =>
            {
                askMoreCallCount++;
                return true;
            }).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(askMoreCallCount, Is.EqualTo(1));
                Assert.That(outputWriter.Outputs, Has.Count.EqualTo(2));
                Assert.That(((MessagesOutput)outputWriter.Outputs[0]).Header, Is.EqualTo(TestConstants.Inbox));
                Assert.That(((MessagesOutput)outputWriter.Outputs[1]).Header, Is.Null);
                Assert.That(((MessagesOutput)outputWriter.Outputs[0]).Messages.Select(static message => message.Id), Is.EqualTo([1u, 2u]));
                Assert.That(((MessagesOutput)outputWriter.Outputs[1]).Messages.Select(static message => message.Id), Is.EqualTo([3u]));
            });
        }

        [Test]
        public async Task WriteMessagesAsyncWhenNextPageIsEmptyAfterFullFirstPageDoesNotWriteTrailingEmptyPage()
        {
            FakeApplicationOutputWriter outputWriter = new();
            ApplicationOutputCoordinator coordinator = new(new ApplicationPagingPolicy(TestApplicationFactory.CreateLaunchOptionsOptions()), outputWriter);
            Message firstMessage = TestDataFactory.CreateMessage(1, TestConstants.Subject1);
            Message secondMessage = TestDataFactory.CreateMessage(2, TestConstants.Subject2);
            int sourceCallCount = 0;
            int askMoreCallCount = 0;

            await coordinator.WriteMessagesAsync(TestConstants.Inbox, new ApplicationListingOptions(2, null), (pageSize, lastItem) =>
            {
                sourceCallCount++;

                return Task.FromResult<IEnumerable<Message>>(sourceCallCount switch
                {
                    1 => [firstMessage, secondMessage],
                    _ => []
                });
            }, () =>
            {
                askMoreCallCount++;
                return true;
            }).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(sourceCallCount, Is.EqualTo(2));
                Assert.That(askMoreCallCount, Is.EqualTo(1));
                Assert.That(outputWriter.Outputs, Has.Count.EqualTo(1));
                Assert.That(((MessagesOutput)outputWriter.Outputs[0]).Header, Is.EqualTo(TestConstants.Inbox));
                Assert.That(((MessagesOutput)outputWriter.Outputs[0]).Messages.Select(static message => message.Id), Is.EqualTo([1u, 2u]));
            });
        }

        [Test]
        public async Task WriteMessagesAsyncWhenSourceProvidesMultiplePagesPassesLastItemToNextPage()
        {
            FakeApplicationOutputWriter outputWriter = new();
            ApplicationOutputCoordinator coordinator = new(new ApplicationPagingPolicy(TestApplicationFactory.CreateLaunchOptionsOptions(output: "json")), outputWriter);
            Message firstMessage = TestDataFactory.CreateMessage(1, TestConstants.Subject1);
            Message secondMessage = TestDataFactory.CreateMessage(2, TestConstants.Subject2);
            Message thirdMessage = TestDataFactory.CreateMessage(3, TestConstants.Subject3);
            int sourceCallCount = 0;
            Message? secondCallLastItem = null;

            await coordinator.WriteMessagesAsync(TestConstants.Inbox, new ApplicationListingOptions(2, null), (pageSize, lastItem) =>
            {
                sourceCallCount++;

                if (sourceCallCount == 2)
                {
                    secondCallLastItem = lastItem;
                }

                return Task.FromResult<IEnumerable<Message>>(sourceCallCount switch
                {
                    1 => [firstMessage, secondMessage],
                    2 => [thirdMessage],
                    _ => []
                });
            }, () => false).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(secondCallLastItem, Is.Not.Null);
                Assert.That(secondCallLastItem!.Id, Is.EqualTo(2u));
                Assert.That(((MessagesOutput)outputWriter.Outputs[0]).Messages.Select(static message => message.Id), Is.EqualTo([1u, 2u, 3u]));
            });
        }

        [Test]
        public async Task WriteMessagesAsyncWhenLimitIsNotSpecifiedUsesDefaultLimit()
        {
            FakeApplicationOutputWriter outputWriter = new();
            ApplicationOutputCoordinator coordinator = new(new ApplicationPagingPolicy(TestApplicationFactory.CreateLaunchOptionsOptions(output: "json")), outputWriter);
            int sourceCallCount = 0;

            await coordinator.WriteMessagesAsync(TestConstants.Inbox, new ApplicationListingOptions(10, null), (pageSize, lastItem) =>
            {
                sourceCallCount++;
                int start = ((sourceCallCount - 1) * pageSize) + 1;
                return Task.FromResult<IEnumerable<Message>>(Enumerable.Range(start, pageSize).Select(id => TestDataFactory.CreateMessage((uint)id, $"Subject {id}")));
            }, () => true).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(sourceCallCount, Is.EqualTo(2));
                Assert.That(((MessagesOutput)outputWriter.Outputs[0]).Messages, Has.Count.EqualTo(ApplicationListingOptions.DefaultLimit));
            });
        }

        [Test]
        public async Task WriteMessagesAsyncWhenSourceReturnsNullItemsFiltersThemOut()
        {
            FakeApplicationOutputWriter outputWriter = new();
            ApplicationOutputCoordinator coordinator = new(new ApplicationPagingPolicy(TestApplicationFactory.CreateLaunchOptionsOptions(output: "json")), outputWriter);
            Message firstMessage = TestDataFactory.CreateMessage(1, TestConstants.Subject1);
            Message secondMessage = TestDataFactory.CreateMessage(2, TestConstants.Subject2);

            await coordinator.WriteMessagesAsync(TestConstants.Inbox, new ApplicationListingOptions(5, null), (pageSize, lastItem) => Task.FromResult<IEnumerable<Message>>([firstMessage, null!, secondMessage]), () => false).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(outputWriter.Outputs, Has.Count.EqualTo(1));
                Assert.That(((MessagesOutput)outputWriter.Outputs[0]).Messages.Select(static message => message.Id), Is.EqualTo([1u, 2u]));
            });
        }

        [Test]
        public async Task WriteMessagesAsyncWhenFirstPageIsEmptyWritesSingleEmptyOutputWithHeaderWithoutPrompt()
        {
            FakeApplicationOutputWriter outputWriter = new();
            ApplicationOutputCoordinator coordinator = new(new ApplicationPagingPolicy(TestApplicationFactory.CreateLaunchOptionsOptions()), outputWriter);
            int askMoreCallCount = 0;
            int sourceCallCount = 0;

            await coordinator.WriteMessagesAsync(TestConstants.Inbox, new ApplicationListingOptions(2, null), (pageSize, lastItem) =>
            {
                sourceCallCount++;
                return Task.FromResult<IEnumerable<Message>>([]);
            }, () =>
            {
                askMoreCallCount++;
                return true;
            }).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(sourceCallCount, Is.EqualTo(1));
                Assert.That(askMoreCallCount, Is.Zero);
                Assert.That(outputWriter.Outputs, Has.Count.EqualTo(1));
                Assert.That(((MessagesOutput)outputWriter.Outputs[0]).Header, Is.EqualTo(TestConstants.Inbox));
                Assert.That(((MessagesOutput)outputWriter.Outputs[0]).Messages, Is.Empty);
            });
        }

        [Test]
        public void WriteContactsWhenLimitIsSetWritesOnlyLimitedContacts()
        {
            FakeApplicationOutputWriter outputWriter = new();
            ApplicationOutputCoordinator coordinator = new(new ApplicationPagingPolicy(TestApplicationFactory.CreateLaunchOptionsOptions(output: "json")), outputWriter);
            List<Contact> contacts = [TestDataFactory.CreateContact(1), TestDataFactory.CreateContact(2), TestDataFactory.CreateContact(3)];
            bool askMoreCalled = false;

            coordinator.WriteContacts(new ApplicationListingOptions(20, 2), contacts, () =>
            {
                askMoreCalled = true;
                return true;
            });

            Assert.Multiple(() =>
            {
                Assert.That(askMoreCalled, Is.False);
                Assert.That(outputWriter.Outputs, Has.Count.EqualTo(1));
                Assert.That(((ContactsOutput)outputWriter.Outputs[0]).Contacts.Select(static contact => contact.Id), Is.EqualTo([1, 2]));
            });
        }

        [Test]
        public async Task WriteMessagesAsyncWhenLimitIsSetStopsAfterRequestedNumberOfMessages()
        {
            FakeApplicationOutputWriter outputWriter = new();
            ApplicationOutputCoordinator coordinator = new(new ApplicationPagingPolicy(TestApplicationFactory.CreateLaunchOptionsOptions(output: "json")), outputWriter);
            Message firstMessage = TestDataFactory.CreateMessage(1, TestConstants.Subject1);
            Message secondMessage = TestDataFactory.CreateMessage(2, TestConstants.Subject2);
            Message thirdMessage = TestDataFactory.CreateMessage(3, TestConstants.Subject3);
            int sourceCallCount = 0;
            bool askMoreCalled = false;

            await coordinator.WriteMessagesAsync(TestConstants.Inbox, new ApplicationListingOptions(2, 3), (pageSize, lastItem) =>
            {
                sourceCallCount++;

                return Task.FromResult<IEnumerable<Message>>(sourceCallCount switch
                {
                    1 => [firstMessage, secondMessage],
                    2 => [thirdMessage],
                    _ => [TestDataFactory.CreateMessage(4, TestConstants.Subject4)]
                });
            }, () =>
            {
                askMoreCalled = true;
                return true;
            }).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(sourceCallCount, Is.EqualTo(2));
                Assert.That(askMoreCalled, Is.False);
                Assert.That(outputWriter.Outputs, Has.Count.EqualTo(1));
                Assert.That(((MessagesOutput)outputWriter.Outputs[0]).Messages.Select(static message => message.Id), Is.EqualTo([1u, 2u, 3u]));
                Assert.That(((MessagesOutput)outputWriter.Outputs[0]).Paging?.HasMore, Is.True);
            });
        }
    }
}
