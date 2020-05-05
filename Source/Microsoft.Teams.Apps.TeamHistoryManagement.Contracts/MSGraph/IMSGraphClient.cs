// <copyright file="IMSGraphClient.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Describes a Graph client functionality.
    /// </summary>
    public interface IMSGraphClient
    {
        /// <summary>
        /// Gets the conversation history for specified conversation.
        /// </summary>
        /// <param name="tokenProvider">The user token provider.</param>
        /// <param name="teamId">The team id.</param>
        /// <param name="conversationId">The teams conversation id or channel id.</param>
        /// <param name="messageId">The message id.</param>
        /// <param name="since">Date time since which messages must be requested.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<IEnumerable<IMessageDetails>> GetConversationHistoryAsync(Func<Task<string>> tokenProvider, string teamId, string conversationId, string messageId, DateTimeOffset? since);

        /// <summary>
        /// Downloads all images from the messages collection. Converts images to base64. Updates images in messages with base64 data.
        /// </summary>
        /// <param name="tokenProvider">The user token provider.</param>
        /// <param name="messages">Messages with images to download.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DownloadImages(Func<Task<string>> tokenProvider, IEnumerable<IMessageDetails> messages);

        /// <summary>
        /// Uploads the file in personal one drive in root directory.
        /// </summary>
        /// <param name="tokenProvider">The user token provider.</param>
        /// <param name="stream">The file stream.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<IDriveItem> UploadFileInPersonalOneDrive(Func<Task<string>> tokenProvider, MemoryStream stream, string filePath);

        /// <summary>
        /// Uploads the file in team drive.
        /// </summary>
        /// <param name="tokenProvider">The user token provider.</param>
        /// <param name="stream">The file stream.</param>
        /// <param name="teamId">The id (GUID) of the team.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<IDriveItem> UploadFileInTeamDrive(Func<Task<string>> tokenProvider, MemoryStream stream, string teamId, string filePath);
    }
}
