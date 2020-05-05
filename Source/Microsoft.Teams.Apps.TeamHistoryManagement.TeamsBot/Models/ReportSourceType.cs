// <copyright file="ReportSourceType.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Models
{
    /// <summary>
    /// Scopes of report.
    /// </summary>
    public enum ReportSourceType
    {
        /// <summary>
        /// Messages from current group chat
        /// </summary>
        Chat = 1,

        /// <summary>
        /// Messages from current conversation of channel
        /// </summary>
        Conversation = 2,

        /// <summary>
        /// Messages from all channel conversations
        /// </summary>
        Channel = 3,
    }
}
