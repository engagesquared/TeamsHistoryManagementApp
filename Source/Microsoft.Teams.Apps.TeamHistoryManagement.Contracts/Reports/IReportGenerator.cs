// <copyright file="IReportGenerator.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Reports
{
    /// <summary>
    /// Describes a report body generator service.
    /// </summary>
    public interface IReportGenerator
    {
        /// <summary>
        /// Prepares the report body as byte array by report body details.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <returns></returns>
        byte[] PrepareDocument(IReportBodyDetails details, ReportFormatType format);
    }
}
