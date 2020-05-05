// <copyright file="OneDriveService.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.MSGraphProvider.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Graph;

    /// <summary>
    /// Implements the service for working with personal OneDrive.
    /// </summary>
    internal class OneDriveService
    {
        public const int SmallFileSizeLimit = 4 * 1024 * 1024;

        /// <summary>
        ///
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="stream"></param>
        /// <param name="uploadPath"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<DriveItem> UploadToPersonalOneDrive(GraphServiceClient graphClient, MemoryStream stream, string uploadPath)
        {
            if (stream.Length > SmallFileSizeLimit)
            {
                var session = await graphClient.Me.Drive.Root.ItemWithPath(uploadPath).CreateUploadSession().Request().PostAsync();
                return await Upload(graphClient, session, stream, uploadPath);
            }
            else
            {
                return await graphClient.Me.Drive.Root.ItemWithPath(uploadPath).Content.Request().PutAsync<DriveItem>(stream);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="graphClient"></param>
        /// <param name="groupId"></param>
        /// <param name="stream"></param>
        /// <param name="uploadPath"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<DriveItem> UploadToChannelFolder(GraphServiceClient graphClient, string groupId, MemoryStream stream, string uploadPath)
        {
            if (stream.Length > SmallFileSizeLimit)
            {
                var session = await graphClient.Groups[groupId].Drive.Root.ItemWithPath(uploadPath).CreateUploadSession().Request().PostAsync();
                return await Upload(graphClient, session, stream, uploadPath);
            }
            else
            {
                return await graphClient.Groups[groupId].Drive.Root.ItemWithPath(uploadPath).Content.Request().PutAsync<DriveItem>(stream);
            }
        }

        private static async Task<DriveItem> Upload(GraphServiceClient graphClient, UploadSession session, MemoryStream stream, string uploadPath, bool first = true)
        {
            var maxSizeChunk = 320 * 4 * 1024;
            var provider = new ChunkedUploadProvider(session, graphClient, stream, maxSizeChunk);
            var chunckRequests = provider.GetUploadChunkRequests();
            var exceptions = new List<Exception>();

            // var readBuffer = new byte[maxSizeChunk];
            DriveItem itemResult = null;

            // upload the chunks
            foreach (var request in chunckRequests)
            {
                var result = await provider.GetChunkRequestResponseAsync(request, exceptions);

                if (result.UploadSucceeded)
                {
                    itemResult = result.ItemResponse;
                }
            }

            // Check that upload succeeded
            if (itemResult == null)
            {
                if (first)
                {
                    return await Upload(graphClient, session, stream, uploadPath, false);
                }
                else
                {
                    throw new Exception("Can't upload file into onedrive");
                }
            }

            return itemResult;
        }
    }
}
