// <copyright file="IDriveItem.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph
{
    public interface IDriveItem
    {
        /// <summary>
        /// Direct file link (like https://site/library/file.ext).
        /// </summary>
        string ContentUrl { get; set; }

        /// <summary>
        /// Download file link (like https://site/_layouts/15/download.aspx?uniqueId=id).
        /// </summary>
        string DownloadUrl { get; set; }

        string FileName { get; set; }

        /// <summary>
        /// Sharepoint GUID-like id.
        /// </summary>
        string UniqueId { get; set; }

        string MimeType { get; set; }

        /// <summary>
        /// OneDrive id.
        /// </summary>
        string Id { get; set; }
    }
}
