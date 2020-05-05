// <copyright file="MessageAttachment.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.MSGraphProvider.Models
{
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph;

    internal class MessageAttachment : IMessageAttachment
    {
        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public string ContentType { get; set; }

        /// <inheritdoc/>
        public string ContentUrl { get; set; }

        /// <inheritdoc/>
        public string Content { get; set; }

        /// <inheritdoc/>
        public string Name { get; set; }

        /// <inheritdoc/>
        public string ThumbnailUrl { get; set; }
    }
}
