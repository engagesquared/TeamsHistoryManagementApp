// <copyright file="MessageIdentity.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.MSGraphProvider.Models
{
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph;

    internal class MessageIdentity : IIdentity
    {
        /// <inheritdoc/>
        public string DisplayName { get; set; }

        /// <inheritdoc/>
        public string Id { get; set; }
    }
}
