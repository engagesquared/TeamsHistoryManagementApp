// <copyright file="ReportFileFormatConverter.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Common.Converters
{
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Reports;

    public static class ReportFileFormatConverter
    {
        public static string GetReportFormat(ReportFormatType type)
        {
            switch (type)
            {
                case ReportFormatType.HTML:
                    return Resources.Strings.ReportHtmlFormat;
                case ReportFormatType.PDF:
                    return Resources.Strings.ReportPdfFormat;
                case ReportFormatType.JSON:
                    return Resources.Strings.ReportJsonFormat;
                case ReportFormatType.TXT:
                    return Resources.Strings.ReportTextFormat;
                default:
                    break;
            }

            return null;
        }

        public static bool TryParse(string str, out ReportFormatType type)
        {
            var parsed = true;
            type = ReportFormatType.TXT;
            str = str ?? string.Empty;

            if (str.Equals(Resources.Strings.ReportHtmlFormat))
            {
                type = ReportFormatType.HTML;
            }
            else if (str.Equals(Resources.Strings.ReportPdfFormat))
            {
                type = ReportFormatType.PDF;
            }
            else if (str.Equals(Resources.Strings.ReportJsonFormat))
            {
                type = ReportFormatType.JSON;
            }
            else if (str.Equals(Resources.Strings.ReportTextFormat))
            {
                type = ReportFormatType.TXT;
            }
            else
            {
                parsed = false;
            }

            return parsed;
        }
    }
}
