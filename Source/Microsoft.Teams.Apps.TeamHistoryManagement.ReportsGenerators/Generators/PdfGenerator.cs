// <copyright file="PdfGenerator.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.ReportsGenerators.Generators
{
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Reports;
    using Wkhtmltopdf.NetCore;

    /// <summary>
    /// Implements the PDF file body generation service. Uses Html generator as a base to build PDF.
    /// </summary>
    /// <seealso cref="Contracts.Reports.IReportBodyGenerator" />
    public class PdfGenerator : HtmlGenerator, IReportBodyGenerator
    {
        private readonly IGeneratePdf generatePdf;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfGenerator"/> class.
        /// </summary>
        /// <param name="generatePdf">generatePdf</param>
        public PdfGenerator(IGeneratePdf generatePdf)
        {
            this.generatePdf = generatePdf;
        }

        public new ReportFormatType Type => ReportFormatType.PDF;

        /// <inheritdoc/>
        ReportFormatType IReportBodyGenerator.Type => ReportFormatType.PDF;

        /// <inheritdoc/>
        public new byte[] PrepareDocument(IReportBodyDetails details)
        {
            var html = PrepareHtml(details);
            return generatePdf.GetPDF(html);
        }
    }
}
