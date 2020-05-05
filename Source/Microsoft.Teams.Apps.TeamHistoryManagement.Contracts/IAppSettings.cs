// <copyright file="IAppSettings.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.Contracts
{
    using System.Collections.Generic;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Reports;

    public interface IAppSettings
    {
        string TenantId { get; }

        string MicrosoftAppId { get; }

        string MicrosoftAppPassword { get; }

        string ConnectionName { get; }

        string BlobStorageConnectionString { get; }

        string BlobStorageContainerName { get; }

        bool UseCardUpdating { get; }

        string GraphEndpointBaseUrl { get; }

        int SignInTimeout { get; }

        List<ReportFormatType> ReportFormats { get; }

        string ReportsFolderName { get; }
    }
}
