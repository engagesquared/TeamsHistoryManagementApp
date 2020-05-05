// <copyright file="MessageReaction.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.MSGraphProvider.Models
{
    using System;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph;
    using Microsoft.Teams.Apps.TeamHistoryManagement.MSGraphProvider.Helpers;
    using Newtonsoft.Json;

    internal class MessageReaction : IMessageReaction
    {
        /// <inheritdoc/>
        public DateTimeOffset CreatedDateTime { get; set; }

        /// <inheritdoc/>
        public string ReactionType { get; set; }

        /// <inheritdoc/>
        [JsonConverter(typeof(ConcreteConverter<MessageIdentitySet>))]
        public IMessageIdentitySet User { get; set; }
    }
}
