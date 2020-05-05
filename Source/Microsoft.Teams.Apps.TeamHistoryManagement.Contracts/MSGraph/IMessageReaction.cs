// <copyright file="IMessageReaction.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph
{
    using System;

    /// <summary>
    /// Represents a reaction to a messasge entity.
    /// </summary>
    public interface IMessageReaction
    {
        /// <summary>
        /// Gets or sets the Timestamp type represents date and time of reaction.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        DateTimeOffset CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the type of the reaction.
        /// </summary>
        /// <value>
        /// The type of the reaction type. Supported values are like, angry, sad, laugh, heart, surprised.
        /// </value>
        string ReactionType { get; set; }

        /// <summary>
        /// Gets or sets the user who reacted to the message.
        /// </summary>
        /// <value>
        /// The user.
        /// </value>
        IMessageIdentitySet User { get; set; }
    }
}
