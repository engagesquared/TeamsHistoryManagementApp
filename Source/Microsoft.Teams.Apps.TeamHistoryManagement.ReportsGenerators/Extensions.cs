// <copyright file="Extensions.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.ReportsGenerators
{
    using System;

    internal static class Extensions
    {
        /// <summary>
        /// To the international time format.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public static string ToInternational(this DateTimeOffset date)
        {
            return date.ToString("dd MMMM yyyy, h:mm:ss tt zzz");
        }

        /// <summary>
        /// To the international time format.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public static string ToInternational(this DateTimeOffset? date)
        {
            if (date.HasValue)
            {
                return ToInternational(date.Value);
            }

            return string.Empty;
        }
    }
}