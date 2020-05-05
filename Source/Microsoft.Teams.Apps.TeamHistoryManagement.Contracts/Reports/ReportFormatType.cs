// <copyright file="ReportFormatType.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Reports
{
    using System.ComponentModel;

    /// <summary>
    /// Provided history report file formats.
    /// </summary>
    public enum ReportFormatType
    {
        /// <summary>
        /// The HTML (.html) format
        /// </summary>
        HTML,

        /// <summary>
        /// The PDF (.pdf) format
        /// </summary>
        PDF,

        /// <summary>
        /// The JSON (.json) format
        /// </summary>
        JSON,

        /// <summary>
        /// The TXT (.txt) format
        /// </summary>
        TXT,
    }
}
