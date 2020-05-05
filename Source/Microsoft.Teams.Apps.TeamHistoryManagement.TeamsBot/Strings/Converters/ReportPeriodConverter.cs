// <copyright file="ReportPeriodConverter.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Common.Converters
{
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Models;

    public static class ReportPeriodConverter
    {
        public static string GetReportPeriod(ReportPeriodType type)
        {
            switch (type)
            {
                case ReportPeriodType.AllTime:
                    return Resources.Strings.TimePeriodOptionAllTime;
                case ReportPeriodType.Last7Days:
                    return Resources.Strings.TimePeriodOptionLast7Days;
                case ReportPeriodType.LastDay:
                    return Resources.Strings.TimePeriodOptionLastDay;
                default:
                    break;
            }

            return null;
        }

        public static bool TryParse(string str, out ReportPeriodType type)
        {
            var parsed = true;
            type = ReportPeriodType.LastDay;
            str = str ?? string.Empty;

            if (str.Equals(Resources.Strings.TimePeriodOptionAllTime))
            {
                type = ReportPeriodType.AllTime;
            }
            else if (str.Equals(Resources.Strings.TimePeriodOptionLast7Days))
            {
                type = ReportPeriodType.Last7Days;
            }
            else if (str.Equals(Resources.Strings.TimePeriodOptionLastDay))
            {
                type = ReportPeriodType.LastDay;
            }
            else
            {
                parsed = false;
            }

            return parsed;
        }
    }
}
