// <copyright file="MessageBody.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.MSGraphProvider.Models
{
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph;

    internal class MessageBody : IMessageBody
    {
        /// <inheritdoc/>
        public string Content { get; set; }

        /// <inheritdoc/>
        public string ContentType { get; set; }
    }
}
