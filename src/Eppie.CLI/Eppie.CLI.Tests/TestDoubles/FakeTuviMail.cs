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

using Tuvi.Core;
using Tuvi.Core.DataStorage;
using Tuvi.Core.Entities;
using Tuvi.Core.Utils;

namespace Eppie.CLI.Tests.TestDoubles
{
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated directly by tests as a reusable fake.")]
    [SuppressMessage("Style", "IDE0025:Use expression body for property", Justification = "The test fake keeps block bodies consistently for members that throw NotSupportedException.")]
    internal sealed class FakeTuviMail(TuviMailInvocationState state) : ITuviMail
    {
        private readonly TuviMailInvocationState _state = state;

        public ICredentialsManager CredentialsManager
        {
            get
            {
                throw CreateNotSupportedException();
            }
        }

        public event EventHandler<MessagesReceivedEventArgs>? MessagesReceived
        {
            add { }
            remove { }
        }

        public event EventHandler<UnreadMessagesReceivedEventArgs>? UnreadMessagesReceived
        {
            add { }
            remove { }
        }

        public event EventHandler<MessageDeletedEventArgs>? MessageDeleted
        {
            add { }
            remove { }
        }

        public event EventHandler<MessagesAttributeChangedEventArgs>? MessagesIsReadChanged
        {
            add { }
            remove { }
        }

        public event EventHandler<MessagesAttributeChangedEventArgs>? MessagesIsFlaggedChanged
        {
            add { }
            remove { }
        }

        public event EventHandler<AccountEventArgs>? AccountAdded
        {
            add { }
            remove { }
        }

        public event EventHandler<AccountEventArgs>? AccountUpdated
        {
            add { }
            remove { }
        }

        public event EventHandler<AccountEventArgs>? AccountDeleted
        {
            add { }
            remove { }
        }

        public event EventHandler<FolderCreatedEventArgs>? FolderCreated
        {
            add { }
            remove { }
        }

        public event EventHandler<FolderDeletedEventArgs>? FolderDeleted
        {
            add { }
            remove { }
        }

        public event EventHandler<FolderRenamedEventArgs>? FolderRenamed
        {
            add { }
            remove { }
        }

        public event EventHandler<ExceptionEventArgs>? ExceptionOccurred
        {
            add { }
            remove { }
        }

        public event EventHandler<ContactAddedEventArgs>? ContactAdded
        {
            add { }
            remove { }
        }

        public event EventHandler<ContactChangedEventArgs>? ContactChanged
        {
            add { }
            remove { }
        }

        public event EventHandler<ContactDeletedEventArgs>? ContactDeleted
        {
            add { }
            remove { }
        }

        public event EventHandler<EventArgs>? WipeAllDataNeeded
        {
            add { }
            remove { }
        }

        public Task TestMailServerAsync(string serverAddress, int serverPort, MailProtocol protocol, ICredentialsProvider credentialsProvider, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task<bool> InitializeApplicationAsync(string password, CancellationToken cancellationToken = default)
        {
            return _state.HandleInitializeApplicationAsync(password, cancellationToken);
        }

        public Task<bool> IsFirstApplicationStartAsync(CancellationToken cancellationToken = default)
        {
            return _state.HandleIsFirstApplicationStartAsync(cancellationToken);
        }

        public Task ResetApplicationAsync()
        {
            throw CreateNotSupportedException();
        }

        public Task<bool> ChangeApplicationPasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task<bool> ExistsAccountWithEmailAddressAsync(EmailAddress email, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task<Account> GetAccountAsync(EmailAddress email, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task<List<Account>> GetAccountsAsync(CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task<IReadOnlyList<CompositeAccount>> GetCompositeAccountsAsync(CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task<IAccountService> GetAccountServiceAsync(EmailAddress email, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task AddAccountAsync(Account account, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task DeleteAccountAsync(Account account, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task UpdateAccountAsync(Account account, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task CreateHybridAccountAsync(Account account, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task CheckForNewMessagesInFolderAsync(CompositeFolder folder, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task CheckForNewInboxMessagesAsync(CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task<IEnumerable<Contact>> GetContactsAsync(CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task<IReadOnlyList<Contact>> GetContactsAsync(int count, Contact lastContact, ContactsSortOrder sortOrder, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task SetContactNameAsync(EmailAddress contactEmail, string newName, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task SetContactAvatarAsync(EmailAddress contactEmail, byte[] avatarBytes, int avatarWidth, int avatarHeight, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task RemoveContactAsync(EmailAddress contactEmail, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task<IReadOnlyList<Message>> GetAllEarlierMessagesAsync(int count, Message lastMessage, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task<IReadOnlyList<Message>> GetContactEarlierMessagesAsync(EmailAddress contactEmail, int count, Message lastMessage, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task<IReadOnlyList<Message>> GetFolderEarlierMessagesAsync(Folder folder, int count, Message lastMessage, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task<IReadOnlyList<Message>> GetFolderEarlierMessagesAsync(CompositeFolder folder, int count, Message lastMessage, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task<int> GetUnreadCountForAllAccountsInboxAsync(CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task<IReadOnlyDictionary<EmailAddress, int>> GetUnreadMessagesCountByContactAsync(CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task DeleteMessagesAsync(IReadOnlyList<Message> messages, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task RestoreFromBackupIfNeededAsync(Uri downloadUri)
        {
            throw CreateNotSupportedException();
        }

        public Task MarkMessagesAsReadAsync(IEnumerable<Message> messages, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task MarkMessagesAsUnReadAsync(IEnumerable<Message> messages, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task FlagMessagesAsync(IEnumerable<Message> messages, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task UnflagMessagesAsync(IEnumerable<Message> messages, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task<Message> GetMessageBodyAsync(Message message, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task<Message> GetMessageBodyHighPriorityAsync(Message message, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task SendMessageAsync(Message message, bool encrypt, bool sign, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task<Message> CreateDraftMessageAsync(Account account, Message message, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task<Message> UpdateDraftMessageAsync(uint id, Message message, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task MoveMessagesAsync(IReadOnlyList<Message> messages, CompositeFolder targetFolder, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task UpdateMessageProcessingResultAsync(Message message, string result, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task<Folder> CreateFolderAsync(EmailAddress accountEmail, string folderName, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task DeleteFolderAsync(EmailAddress accountEmail, Folder folder, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task<Folder> RenameFolderAsync(EmailAddress accountEmail, Folder folder, string newName, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public Task<string> ClaimDecentralizedNameAsync(string name, Account account, CancellationToken cancellationToken = default)
        {
            throw CreateNotSupportedException();
        }

        public ISecurityManager GetSecurityManager()
        {
            throw CreateNotSupportedException();
        }

        public IBackupManager GetBackupManager()
        {
            throw CreateNotSupportedException();
        }

        public ITextUtils GetTextUtils()
        {
            throw CreateNotSupportedException();
        }

        public IAIAgentsStorage GetAIAgentsStorage()
        {
            throw CreateNotSupportedException();
        }

        public void Dispose()
        {
        }

        private static NotSupportedException CreateNotSupportedException()
        {
            return new NotSupportedException("The member is not required by this test fake.");
        }
    }

    internal sealed class TuviMailInvocationState
    {
        internal bool IsFirstApplicationStartResult { get; init; }
        internal bool InitializeApplicationResult { get; init; } = true;
        internal int InitializeApplicationCallCount { get; private set; }
        internal string? LastPassword { get; private set; }
        internal CancellationToken LastIsFirstApplicationStartCancellationToken { get; private set; }
        internal CancellationToken LastInitializeApplicationCancellationToken { get; private set; }

        internal Task<bool> HandleIsFirstApplicationStartAsync(CancellationToken cancellationToken)
        {
            LastIsFirstApplicationStartCancellationToken = cancellationToken;
            return Task.FromResult(IsFirstApplicationStartResult);
        }

        internal Task<bool> HandleInitializeApplicationAsync(string password, CancellationToken cancellationToken)
        {
            InitializeApplicationCallCount++;
            LastPassword = password;
            LastInitializeApplicationCancellationToken = cancellationToken;

            return Task.FromResult(InitializeApplicationResult);
        }
    }
}
