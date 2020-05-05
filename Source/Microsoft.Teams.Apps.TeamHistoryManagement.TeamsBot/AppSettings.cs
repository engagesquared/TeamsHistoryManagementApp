// <copyright file="AppSettings.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Reports;

    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationSettings"/> class.
        /// </summary>
        /// <param name="configuration">configuration</param>
        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;
            ParseConfig();
        }

        public string TenantId { get; private set; }

        public string MicrosoftAppId { get; private set; }

        public string MicrosoftAppPassword { get; private set; }

        public string ConnectionName { get; private set; }

        public string BlobStorageConnectionString { get; private set; }

        public string BlobStorageContainerName { get; private set; }

        public bool UseCardUpdating { get; private set; }

        public string GraphEndpointBaseUrl { get; private set; }

        public int SignInTimeout { get; private set; }

        public List<ReportFormatType> ReportFormats { get; private set; }

        public string ReportsFolderName { get; private set; }

        private void ParseConfig()
        {
            TenantId = configuration["TenantId"] ?? string.Empty;
            MicrosoftAppId = configuration["MicrosoftAppId"] ?? string.Empty;
            MicrosoftAppPassword = configuration["MicrosoftAppPassword"] ?? string.Empty;
            ConnectionName = configuration["ConnectionName"] ?? string.Empty;
            BlobStorageContainerName = configuration["BlobStorageContainerName"] ?? "botstatestorage";
            ReportsFolderName = configuration["ReportsFolderName"] ?? string.Empty;
            GraphEndpointBaseUrl = configuration["GraphEndpointBaseUrl"]?.TrimEnd().TrimEnd('/').ToLowerInvariant();
            BlobStorageConnectionString = configuration.GetConnectionString("BlobStorage");
            UseCardUpdating = ParseBool("UseCardUpdating");

            // User has 5 minutes to login (300 sec) by  default
            SignInTimeout = ParseInt("SignInTimeoutSec", 300);
            ReportFormats = ParseReportFormats("ReportFileFormats");
        }

        private bool ParseBool(string key, bool defaultValue = false)
        {
            var boolStr = (configuration[key] ?? string.Empty).ToLowerInvariant().Trim();
            if (string.IsNullOrWhiteSpace(boolStr))
            {
                return defaultValue;
            }
            else
            {
                return boolStr == "true" || boolStr == "yes" || boolStr == "1";
            }
        }

        private int ParseInt(string key, int defaultValue = 0)
        {
            var intStr = (configuration[key] ?? string.Empty).ToLowerInvariant().Trim();
            if (!string.IsNullOrWhiteSpace(intStr) && int.TryParse(intStr, out int resultInt))
            {
                return resultInt;
            }

            return defaultValue;
        }

        private List<ReportFormatType> ParseReportFormats(string key, ReportFormatType defaultFormat = ReportFormatType.TXT)
        {
            var formatsStr = (configuration[key] ?? string.Empty).ToUpperInvariant().Trim();
            if (!string.IsNullOrWhiteSpace(formatsStr))
            {
                var formatsList = formatsStr.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
                var result = new List<ReportFormatType>();
                formatsList.ForEach(x =>
                {
                    if (Enum.TryParse(x, out ReportFormatType type))
                    {
                        result.Add(type);
                    }
                });
                return result;
            }

            return new List<ReportFormatType>() { defaultFormat };
        }
    }
}
