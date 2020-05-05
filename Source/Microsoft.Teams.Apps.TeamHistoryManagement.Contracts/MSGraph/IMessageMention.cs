// <copyright file="IMessageMention.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph
{
    /// <summary>
    /// Represents a mention in message.
    /// </summary>
    public interface IMessageMention
    {
        /// <summary>
        /// Gets or sets the index of an entity being mentioned in the specified message.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        int Id { get; set; }

        /// <summary>
        /// Gets or sets the string used to represent the mention.
        /// </summary>
        /// <value>
        /// The mention text.
        /// </value>
        string MentionText { get; set; }

        /// <summary>
        /// Gets or sets the entity (user, application, team, or channel) that was mentioned.
        /// </summary>
        /// <value>
        /// The mentioned.
        /// </value>
        IMessageIdentitySet Mentioned { get; set; }
    }
}
