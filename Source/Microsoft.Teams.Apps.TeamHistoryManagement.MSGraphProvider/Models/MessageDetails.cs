// <copyright file="MessageDetails.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.MSGraphProvider.Models
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph;
    using Microsoft.Teams.Apps.TeamHistoryManagement.MSGraphProvider.Helpers;
    using Newtonsoft.Json;

    internal class MessageDetails : IMessageDetails
    {
        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public string ReplyToId { get; set; }

        /// <inheritdoc/>
        public string Etag { get; set; }

        /// <inheritdoc/>
        public DateTimeOffset CreatedDateTime { get; set; }

        /// <inheritdoc/>
        public DateTimeOffset? LastModifiedDateTime { get; set; }

        /// <inheritdoc/>
        public DateTimeOffset? DeletedDateTime { get; set; }

        /// <inheritdoc/>
        public string Subject { get; set; }

        /// <inheritdoc/>
        public string Summary { get; set; }

        /// <inheritdoc/>
        public string Importance { get; set; }

        /// <inheritdoc/>
        public string Locale { get; set; }

        /// <inheritdoc/>
        [JsonConverter(typeof(ConcreteConverter<MessageIdentitySet>))]
        public IMessageIdentitySet From { get; set; }

        /// <inheritdoc/>
        [JsonConverter(typeof(ConcreteConverter<MessageBody>))]
        public IMessageBody Body { get; set; }

        /// <inheritdoc/>
        [JsonConverter(typeof(ConcreteConverter<List<MessageAttachment>>))]
        public IEnumerable<IMessageAttachment> Attachments { get; set; }

        /// <inheritdoc/>
        [JsonConverter(typeof(ConcreteConverter<List<MessageMention>>))]
        public IEnumerable<IMessageMention> Mentions { get; set; }

        /// <inheritdoc/>
        [JsonConverter(typeof(ConcreteConverter<List<MessageReaction>>))]
        public IEnumerable<IMessageReaction> Reactions { get; set; }
    }
}
