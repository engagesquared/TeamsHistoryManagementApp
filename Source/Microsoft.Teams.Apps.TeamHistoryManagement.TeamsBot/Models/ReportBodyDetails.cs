// <copyright file="ReportBodyDetails.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Models
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Reports;

    /// <summary>
    /// Implements report body details model for file body generation.
    /// </summary>
    /// <seealso cref="IReportBodyDetails" />
    public class ReportBodyDetails : IReportBodyDetails
    {
        /// <inheritdoc/>
        public DateTimeOffset? Since { get; set; }

        /// <inheritdoc/>
        public DateTimeOffset Till { get; set; }

        /// <inheritdoc/>
        public string TeamName { get; set; }

        /// <inheritdoc/>
        public string TeamId { get; set; }

        /// <inheritdoc/>
        public string ChannelName { get; set; }

        /// <inheritdoc/>
        public bool IsChannel { get; set; }

        /// <inheritdoc/>
        public string Author { get; set; }

        /// <inheritdoc/>
        public IEnumerable<IMessageDetails> Messages { get; set; }

        /// <inheritdoc/>
        public bool IsConversation { get; set; }

        /// <inheritdoc/>
        public bool IsGroupChat { get; set; }
    }
}
