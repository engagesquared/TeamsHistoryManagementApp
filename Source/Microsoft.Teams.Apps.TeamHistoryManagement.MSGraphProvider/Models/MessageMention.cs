// <copyright file="MessageMention.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.MSGraphProvider.Models
{
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph;
    using Microsoft.Teams.Apps.TeamHistoryManagement.MSGraphProvider.Helpers;
    using Newtonsoft.Json;

    internal class MessageMention : IMessageMention
    {
        /// <inheritdoc/>
        public int Id { get; set; }

        /// <inheritdoc/>
        public string MentionText { get; set; }

        /// <inheritdoc/>
        [JsonConverter(typeof(ConcreteConverter<MessageIdentitySet>))]
        public IMessageIdentitySet Mentioned { get; set; }
    }
}
