// <copyright file="ReportPeriodType.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Models
{
    /// <summary>
    /// Kinds of datetime range report.
    /// </summary>
    public enum ReportPeriodType
    {
        /// <summary>
        /// Messages during last day
        /// </summary>
        LastDay = 1,

        /// <summary>
        /// Messages during last 7 days
        /// </summary>
        Last7Days = 2,

        /// <summary>
        /// All messages
        /// </summary>
        AllTime = 3,
    }
}
