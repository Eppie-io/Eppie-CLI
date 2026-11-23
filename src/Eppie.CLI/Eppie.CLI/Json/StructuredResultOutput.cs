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

using Tuvi.Core.Entities;

namespace Eppie.CLI.Json
{
    internal abstract record StructuredResultOutput<TData, TMeta>(string Code, TData Data, TMeta Meta) : StructuredOutput("result")
    { }

    internal abstract record StructuredResultOutput<TData>(string Code, TData Data) : StructuredResultOutput<TData, object?>(Code, Data, null)
    { }

    internal sealed record AccountsStructuredResultOutput : StructuredResultOutput<IEnumerable<AccountsStructuredResultOutput.AccountData>>
    {
        internal record AccountData(int Id, string Address, string AccountType);

        private AccountsStructuredResultOutput(IEnumerable<AccountData> data)
            : base("accounts", data)
        { }

        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredResultOutputJsonContext.Default.AccountsStructuredResultOutput;
        }

        public static AccountsStructuredResultOutput Create(AccountsOutput accountsOutput)
        {
            ArgumentNullException.ThrowIfNull(accountsOutput);

            IEnumerable<AccountData> data = accountsOutput.Accounts.Select(static account => new AccountData(
                account.Id,
                account.Email.Address,
                account.Type.ToString()
            ));

            return new AccountsStructuredResultOutput(data);
        }
    }

    internal sealed record ContactsStructuredResultOutput : StructuredResultOutput<IEnumerable<ContactsStructuredResultOutput.ContactData>>
    {
        internal record ContactData(int Id, string Address, string FullName, int UnreadCount);

        private ContactsStructuredResultOutput(IEnumerable<ContactData> data)
            : base("contacts", data)
        { }

        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredResultOutputJsonContext.Default.ContactsStructuredResultOutput;
        }

        public static ContactsStructuredResultOutput Create(ContactsOutput contactsOutput)
        {
            ArgumentNullException.ThrowIfNull(contactsOutput);

            IEnumerable<ContactData> data = contactsOutput.Contacts.Select(static contact => new ContactData(
                contact.Id,
                contact.Email.Address,
                contact.FullName,
                contact.UnreadCount
            ));
            return new ContactsStructuredResultOutput(data);
        }
    }

    internal sealed record FoldersStructuredResultOutput : StructuredResultOutput<IEnumerable<FoldersStructuredResultOutput.FolderData>,
                                                                                  FoldersStructuredResultOutput.AccountMeta>
    {
        internal record FolderData(string FullName, int UnreadCount, int TotalCount, IEnumerable<string> Roles);
        internal record AccountMeta(string Account);

        private FoldersStructuredResultOutput(IEnumerable<FolderData> data, AccountMeta meta)
            : base("folders", data, meta)
        { }

        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredResultOutputJsonContext.Default.FoldersStructuredResultOutput;
        }

        public static FoldersStructuredResultOutput Create(FoldersOutput foldersOutput)
        {
            ArgumentNullException.ThrowIfNull(foldersOutput);

            IEnumerable<FolderData> data = foldersOutput.Folders.Select(static folder => new FolderData(
                folder.FullName,
                folder.UnreadCount,
                folder.TotalCount,
                GetFolderRoles(folder)
            ));
            AccountMeta meta = new(foldersOutput.AccountAddress);
            return new FoldersStructuredResultOutput(data, meta);
        }

        private static string[] GetFolderRoles(Folder folder)
        {
            ArgumentNullException.ThrowIfNull(folder);

            List<string> roles = [];

            if (folder.IsInbox)
            {
                roles.Add("inbox");
            }

            if (folder.IsDraft)
            {
                roles.Add("draft");
            }

            if (folder.IsJunk)
            {
                roles.Add("junk");
            }

            if (folder.IsTrash)
            {
                roles.Add("trash");
            }

            if (folder.IsSent)
            {
                roles.Add("sent");
            }

            if (folder.IsImportant)
            {
                roles.Add("important");
            }

            if (folder.IsAll)
            {
                roles.Add("all");
            }

            return [.. roles];
        }
    }

    internal sealed record MessageStructuredResultOutput : StructuredResultOutput<MessageStructuredResultOutput.MessageData>
    {
        internal record MessageData(uint Id, int Pk, DateTimeOffset Date,
                                    IEnumerable<string> To, IEnumerable<string> From,
                                    IEnumerable<string> Cc, IEnumerable<string> Bcc,
                                    string Folder, string Subject, string TextBody, string HtmlBody,
                                    int AttachmentsCount);
        private MessageStructuredResultOutput(MessageData data)
            : base("message", data)
        { }

        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredResultOutputJsonContext.Default.MessageStructuredResultOutput;
        }

        public static MessageStructuredResultOutput Create(MessageOutput messageOutput)
        {
            ArgumentNullException.ThrowIfNull(messageOutput);

            MessageData data = new(
                messageOutput.Message.Id,
                messageOutput.Message.Pk,
                messageOutput.Message.Date,
                messageOutput.Message.To.Select(static to => to.Address),
                messageOutput.Message.From.Select(static from => from.Address),
                messageOutput.Message.Cc.Select(static cc => cc.Address),
                messageOutput.Message.Bcc.Select(static bcc => bcc.Address),
                messageOutput.Message.Folder.FullName,
                messageOutput.Message.Subject,
                messageOutput.Message.TextBody,
                messageOutput.Message.HtmlBody,
                messageOutput.Message.Attachments.Count
            );

            return new MessageStructuredResultOutput(data);
        }
    }

    internal record CompactMessageData(uint Id, int Pk, DateTimeOffset Date,
                                       IEnumerable<string> To, IEnumerable<string> From,
                                       string Folder, string Subject)
    {
        public static CompactMessageData Create(Message message)
        {
            ArgumentNullException.ThrowIfNull(message);

            return new CompactMessageData(
                message.Id,
                message.Pk,
                message.Date,
                message.To.Select(static to => to.Address),
                message.From.Select(static from => from.Address),
                message.Folder.FullName,
                message.Subject
            );
        }
    }

    internal sealed record CompactMessageStructuredResultOutput : StructuredResultOutput<CompactMessageData>
    {
        private CompactMessageStructuredResultOutput(CompactMessageData data)
            : base("message", data)
        { }

        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredResultOutputJsonContext.Default.CompactMessageStructuredResultOutput;
        }

        public static CompactMessageStructuredResultOutput Create(MessageOutput messageOutput)
        {
            ArgumentNullException.ThrowIfNull(messageOutput);
            return new CompactMessageStructuredResultOutput(CompactMessageData.Create(messageOutput.Message));
        }
    }

    internal sealed record MessagesStructuredResultOutput : StructuredResultOutput<IEnumerable<CompactMessageData>,
                                                                                   MessagesStructuredResultOutput.MessagesMeta?>
    {
        internal record MessagesMeta(string Header, int? Returned, bool? HasMore);

        private MessagesStructuredResultOutput(IEnumerable<CompactMessageData> data, MessagesMeta? meta)
            : base("messages", data, meta)
        { }

        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredResultOutputJsonContext.Default.MessagesStructuredResultOutput;
        }

        public static MessagesStructuredResultOutput Create(MessagesOutput messagesOutput)
        {
            ArgumentNullException.ThrowIfNull(messagesOutput);
            IEnumerable<CompactMessageData> data = messagesOutput.Messages
                                                                 .Select(static message => CompactMessageData.Create(message));

            MessagesMeta? meta = null;
            if (!string.IsNullOrWhiteSpace(messagesOutput.Header))
            {
                meta = new(messagesOutput.Header, messagesOutput.Paging?.Returned, messagesOutput.Paging?.HasMore);
            }

            return new MessagesStructuredResultOutput(data, meta);
        }
    }

    internal sealed record AccountAddedStructuredResultOutput : StructuredResultOutput<AccountAddedStructuredResultOutput.NewAccountData>
    {
        internal record NewAccountData(string Address, string AccountType);
        private AccountAddedStructuredResultOutput(NewAccountData data)
            : base("accountAdded", data)
        { }

        protected override JsonTypeInfo GetJsonTypeInfo()
        {
            return StructuredResultOutputJsonContext.Default.AccountAddedStructuredResultOutput;
        }

        public static AccountAddedStructuredResultOutput Create(AccountAddedOutput accountAddedOutput)
        {
            ArgumentNullException.ThrowIfNull(accountAddedOutput);

            NewAccountData data = new(accountAddedOutput.Address, accountAddedOutput.AccountType);

            return new AccountAddedStructuredResultOutput(data);
        }
    }

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
                                 PropertyNameCaseInsensitive = true,
                                 NumberHandling = JsonNumberHandling.AllowReadingFromString)]
    [JsonSerializable(typeof(AccountsStructuredResultOutput))]
    [JsonSerializable(typeof(ContactsStructuredResultOutput))]
    [JsonSerializable(typeof(FoldersStructuredResultOutput))]
    [JsonSerializable(typeof(MessageStructuredResultOutput))]
    [JsonSerializable(typeof(CompactMessageStructuredResultOutput))]
    [JsonSerializable(typeof(MessagesStructuredResultOutput))]
    [JsonSerializable(typeof(AccountAddedStructuredResultOutput))]
    internal partial class StructuredResultOutputJsonContext : JsonSerializerContext
    { }
}
