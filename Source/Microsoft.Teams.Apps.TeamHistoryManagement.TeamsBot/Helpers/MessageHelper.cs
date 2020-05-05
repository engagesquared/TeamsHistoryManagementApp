// <copyright file="MessageHelper.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Helpers
{
    using Microsoft.Bot.Builder;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Models;

    internal static class MessageHelper
    {
        internal static string BuildUserExportedHistoryMessage(ITurnContext context, ReportParameters report)
        {
            var reportPeriods = string.Empty;
            switch (report.ReportPeriod)
            {
                case ReportPeriodType.AllTime:
                    reportPeriods = "all conversations";
                    break;
                case ReportPeriodType.Last7Days:
                    reportPeriods = "conversations from the last 7 days";
                    break;
                case ReportPeriodType.LastDay:
                    reportPeriods = "conversations from the last 24 hours";
                    break;
            }

            var reportScope = string.Empty;
            switch (report.ReportType)
            {
                case ReportSourceType.Chat:
                    reportScope = "this chat";
                    break;
                case ReportSourceType.Conversation:
                    reportScope = "this conversation";
                    break;
                case ReportSourceType.Channel:
                    reportScope = "this channel";
                    break;
            }

            var reportFormat = string.Empty;
            switch (report.Format)
            {
                case Contracts.Reports.ReportFormatType.HTML:
                    reportFormat = "an HTML file";
                    break;
                case Contracts.Reports.ReportFormatType.PDF:
                    reportFormat = "a PDF document";
                    break;
                case Contracts.Reports.ReportFormatType.JSON:
                    reportFormat = "a JSON file";
                    break;
                case Contracts.Reports.ReportFormatType.TXT:
                    reportFormat = "a text file";
                    break;
            }

            var message = $"{context.Activity.From.Name} extracted {reportPeriods} from {reportScope} into {reportFormat} to their personal OneDrive.";
            return message;
        }
    }
}
