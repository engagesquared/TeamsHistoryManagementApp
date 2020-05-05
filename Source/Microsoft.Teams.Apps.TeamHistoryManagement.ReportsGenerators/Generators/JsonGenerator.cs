// <copyright file="JsonGenerator.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.ReportsGenerators.Generators
{
    using System.Text;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Reports;
    using Newtonsoft.Json;

    /// <summary>
    /// Implements the JSON file body generation service.
    /// </summary>
    /// <seealso cref="Contracts.Reports.IReportBodyGenerator" />
    public class JsonGenerator : IReportBodyGenerator
    {
        /// <inheritdoc/>
        public ReportFormatType Type => ReportFormatType.JSON;

        /// <summary>
        /// Prepares the report body as byte array by report body details.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <returns></returns>
        public byte[] PrepareDocument(IReportBodyDetails details)
        {
            var jsonStr = JsonConvert.SerializeObject(details.Messages);
            return Encoding.UTF8.GetBytes(jsonStr.ToString());
        }
    }
}
