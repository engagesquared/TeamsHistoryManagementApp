// <copyright file="ReportGeneratorsProvider.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.ReportsGenerators.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Reports;
    using Microsoft.Teams.Apps.TeamHistoryManagement.ReportsGenerators.Generators;
    using Wkhtmltopdf.NetCore;

    /// <summary>
    /// Provides methods for generate report.
    /// </summary>
    public class ReportGeneratorsProvider : IReportGenerator
    {
        private readonly List<IReportBodyGenerator> generators;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportGeneratorsProvider"/> class.
        /// </summary>
        /// <param name="pdfEngine">pdf engine</param>
        public ReportGeneratorsProvider(IGeneratePdf pdfEngine)
        {
            generators = new List<IReportBodyGenerator>()
            {
                new HtmlGenerator(),
                new PdfGenerator(pdfEngine),
                new TxtGenerator(),
                new JsonGenerator(),
            };
        }

        /// <inheritdoc/>
        public byte[] PrepareDocument(IReportBodyDetails details, ReportFormatType format)
        {
            return GetGenerator(format).PrepareDocument(details);
        }

        private IReportBodyGenerator GetGenerator(ReportFormatType format)
        {
            var generator = generators.FirstOrDefault(x => x.Type == format);
            if (generator == null)
            {
                throw new InvalidOperationException($"Can't find {format} report genarator. It is not registered in {this.GetType().FullName}.");
            }

            return generator;
        }
    }
}
