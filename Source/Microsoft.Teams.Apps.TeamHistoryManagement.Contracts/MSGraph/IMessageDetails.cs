// <copyright file="IMessageDetails.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an individual chat message within a channel or chat.
    /// </summary>
    public interface IMessageDetails
    {
        /// <summary>
        /// Gets the identifier. Read-only. Unique Id of the message.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        string Id { get; }

        /// <summary>
        /// Gets the reply to identifier. Read-only. Id of the parent chat message or root chat message of the thread. (Only applies to chat messages in channels not chats).
        /// </summary>
        /// <value>
        /// The reply to identifier.
        /// </value>
        string ReplyToId { get; }

        /// <summary>
        /// Gets the etag. Read-only. Version number of the chat message.
        /// </summary>
        /// <value>
        /// The etag.
        /// </value>
        string Etag { get; }

        /// <summary>
        /// Gets or sets the timestamp of when the chat message was created. Read only.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        DateTimeOffset CreatedDateTime { get; }

        /// <summary>
        /// Gets the timestamp of when the chat message is created or edited,
        /// including when a reply is made (if it's a root chat message in a channel) or a reaction is added or removed.
        /// </summary>
        /// <value>
        /// The last modified date time.
        /// </value>
        DateTimeOffset? LastModifiedDateTime { get; }

        /// <summary>
        /// Gets the timestamp at which the chat message was deleted, or null if not deleted.
        /// </summary>
        /// <value>
        /// The deleted date time.
        /// </value>
        DateTimeOffset? DeletedDateTime { get; }

        /// <summary>
        /// Gets or sets the subject of the chat message, in plaintext.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        string Subject { get; set; }

        /// <summary>
        /// Gets or sets the summary text of the chat message that could be used for push notifications and summary views or fall back views.
        /// Only applies to channel chat messages, not chat messages in a chat.
        /// </summary>
        /// <value>
        /// The summary.
        /// </value>
        string Summary { get; set; }

        /// <summary>
        /// Gets or sets the the importance of the chat message. The possible values are: normal, high, urgent.
        /// </summary>
        /// <value>
        /// The importance.
        /// </value>
        string Importance { get; set; }

        /// <summary>
        /// Gets or sets the locale of the chat message set by the client.
        /// </summary>
        /// <value>
        /// The locale.
        /// </value>
        string Locale { get; set; }

        /// <summary>
        /// Gets details of the sender of the chat message. Read only.
        /// </summary>
        /// <value>
        /// From.
        /// </value>
        IMessageIdentitySet From { get; }

        /// <summary>
        /// Gets or sets the plaintext/HTML representation of the content of the chat message. Representation is specified by the contentType inside the body.
        /// </summary>
        /// <value>
        /// The body.
        /// </value>
        IMessageBody Body { get; set; }

        /// <summary>
        /// Gets or sets the attachments.
        /// </summary>
        /// <value>
        /// The attachments.
        /// </value>
        IEnumerable<IMessageAttachment> Attachments { get; set; }

        /// <summary>
        /// Gets or sets a list of entities mentioned in the chat message.
        /// </summary>
        /// <value>
        /// The mentions.
        /// </value>
        IEnumerable<IMessageMention> Mentions { get; set; }

        /// <summary>
        /// Gets or sets the reactions for this chat message.
        /// </summary>
        /// <value>
        /// The reactions.
        /// </value>
        IEnumerable<IMessageReaction> Reactions { get; set; }
    }
}
