// <copyright file="IReportBodyDetails.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Reports
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph;

    /// <summary>
    /// Describes a model for report body generation.
    /// </summary>
    public interface IReportBodyDetails
    {
        /// <summary>
        /// Gets or sets the datetime since which messages should be extracted.
        /// </summary>
        /// <value>
        /// The since.
        /// </value>
        DateTimeOffset? Since { get; set; }

        /// <summary>
        /// Gets or sets the datetime of report request till which messages should be extracted.
        /// </summary>
        /// <value>
        /// The till.
        /// </value>
        DateTimeOffset Till { get; set; }

        /// <summary>
        /// Gets or sets the name of the team for team channels history extraction.
        /// </summary>
        /// <value>
        /// The name of the team.
        /// </value>
        string TeamName { get; set; }

        /// <summary>
        /// Gets or sets the Id of the team group.
        /// </summary>
        /// <value>
        /// The Id of the team group.
        /// </value>
        string TeamId { get; set; }

        /// <summary>
        /// Gets or sets the name of the channel for team channels history extraction.
        /// </summary>
        /// <value>
        /// The name of the channel.
        /// </value>
        string ChannelName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is channel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is channel; otherwise, <c>false</c>.
        /// </value>
        bool IsChannel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is conversation (specified conversation of channel).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is conversation; otherwise, <c>false</c>.
        /// </value>
        bool IsConversation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is group chat.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is group chat; otherwise, <c>false</c>.
        /// </value>
        bool IsGroupChat { get; set; }

        /// <summary>
        /// Gets or sets the author - the person who request history extraction.
        /// </summary>
        /// <value>
        /// The author.
        /// </value>
        string Author { get; set; }

        /// <summary>
        /// Gets or sets the list of extracted messages.
        /// </summary>
        /// <value>
        /// The messages.
        /// </value>
        IEnumerable<IMessageDetails> Messages { get; set; }
    }
}
