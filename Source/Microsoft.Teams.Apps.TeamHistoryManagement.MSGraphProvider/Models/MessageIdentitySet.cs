// <copyright file="MessageIdentitySet.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.MSGraphProvider.Models
{
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph;
    using Microsoft.Teams.Apps.TeamHistoryManagement.MSGraphProvider.Helpers;
    using Newtonsoft.Json;

    internal class MessageIdentitySet : IMessageIdentitySet
    {
        /// <inheritdoc/>
        [JsonConverter(typeof(ConcreteConverter<MessageIdentity>))]
        public IIdentity Application { get; set; }

        /// <inheritdoc/>
        [JsonConverter(typeof(ConcreteConverter<MessageIdentity>))]
        public IIdentity Conversation { get; set; }

        /// <inheritdoc/>
        [JsonConverter(typeof(ConcreteConverter<MessageIdentity>))]
        public IIdentity Device { get; set; }

        /// <inheritdoc/>
        [JsonConverter(typeof(ConcreteConverter<MessageIdentity>))]
        public IIdentity User { get; set; }
    }
}
