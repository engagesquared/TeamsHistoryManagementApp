// <copyright file="IReportBodyGenerator.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.ReportsGenerators.Generators
{
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Reports;

    public interface IReportBodyGenerator
    {
        ReportFormatType Type { get; }

        byte[] PrepareDocument(IReportBodyDetails details);
    }
}
