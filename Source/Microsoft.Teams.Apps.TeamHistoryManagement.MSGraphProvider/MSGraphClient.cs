// <copyright file="MSGraphClient.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.MSGraphProvider
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using AngleSharp;
    using AngleSharp.Html.Parser;
    using Microsoft.Extensions.Logging;
    using Microsoft.Graph;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph;
    using Microsoft.Teams.Apps.TeamHistoryManagement.MSGraphProvider.Services;

    /// <summary>
    /// Implements the ms graph client for working with teams.
    /// </summary>
    /// <seealso cref="Contracts.MSGraph.IMSGraphClient" />
    public class MSGraphClient : IMSGraphClient
    {
        private readonly string graphBaseEndpoint;
        private readonly string graphEndpoint;
        private readonly ImagesService imagesService;
        private readonly TeamsHistoryService teamsHistoryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSGraphClient"/> class.
        /// </summary>
        /// <param name="settings">settings</param>
        /// <param name="logger">logger</param>
        public MSGraphClient(IAppSettings settings, ILogger<MSGraphClient> logger)
        {
            graphBaseEndpoint = settings.GraphEndpointBaseUrl;
            graphEndpoint = settings.GraphEndpointBaseUrl + "/beta";
            imagesService = new ImagesService(logger);
            teamsHistoryService = new TeamsHistoryService(logger);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IMessageDetails>> GetConversationHistoryAsync(Func<Task<string>> tokenProvider, string teamId, string conversationId, string messageId, DateTimeOffset? since)
        {
            return await teamsHistoryService.GetConversationHistory(tokenProvider, teamId, conversationId, messageId, since, graphEndpoint) as IEnumerable<IMessageDetails>;
        }

        public async Task DownloadImages(Func<Task<string>> tokenProvider, IEnumerable<IMessageDetails> messages)
        {
            if (messages.Any(x => x?.Body?.ContentType == "html"))
            {
                var config = Configuration.Default;
                var context = BrowsingContext.New(config);

                var tasks = new List<Task>();
                foreach (var message in messages)
                {
                    if (message.Body.ContentType == "html")
                    {
                        tasks.Add(PrepareImageForMessage(tokenProvider, message, context));
                    }
                }

                await Task.WhenAll(tasks.ToArray());
            }
        }

        /// <inheritdoc/>
        public async Task<IDriveItem> UploadFileInPersonalOneDrive(Func<Task<string>> tokenProvider, MemoryStream stream, string filePath)
        {
            GraphServiceClient client = CreateGraphClient(tokenProvider);
            var res = await OneDriveService.UploadToPersonalOneDrive(client, stream, filePath);
            var item = MapResult(res);
            return item;
        }

        /// <inheritdoc/>
        public async Task<IDriveItem> UploadFileInTeamDrive(Func<Task<string>> tokenProvider, MemoryStream stream, string teamId, string filePath)
        {
            GraphServiceClient client = CreateGraphClient(tokenProvider);
            var res = await OneDriveService.UploadToChannelFolder(client, teamId, stream, filePath);
            var item = MapResult(res);
            return item;
        }

        private GraphServiceClient CreateGraphClient(Func<Task<string>> tokenProvider)
        {
            return new GraphServiceClient(
              graphEndpoint,
              new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        string accessToken = await tokenProvider();

                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                    }));
        }

        private async Task PrepareImageForMessage(Func<Task<string>> tokenProvider, IMessageDetails message, IBrowsingContext context)
        {
            var parser = context.GetService<IHtmlParser>();
            var document = parser.ParseDocument(message.Body.Content);
            var images = document.QuerySelectorAll("img[src]");
            var urls = new List<string>();
            foreach (var img in images)
            {
                urls.Add(img.GetAttribute("src"));

                // fixing max-width for big images
                var style = (img.GetAttribute("style") ?? string.Empty) + "; max-width:100%!important;";
                img.SetAttribute("style", style);
            }

            foreach (var url in urls)
            {
                var base64 = await imagesService.GetBase64Image(tokenProvider, url, graphBaseEndpoint);
                message.Body.Content = document.ToHtml();
                message.Body.Content = message.Body.Content.Replace(url, base64);
            }
        }

        private IDriveItem MapResult(DriveItem item)
        {
            var result = new DriveItemResponse()
            {
                Id = item.Id,
                ContentUrl = item.WebUrl,
                MimeType = item.File.MimeType,
            };
            var downloadUrl = string.Empty;
            if (item.AdditionalData.ContainsKey("@content.downloadUrl"))
            {
                downloadUrl = item.AdditionalData["@content.downloadUrl"].ToString();
            }

            if (item.AdditionalData.ContainsKey("@microsoft.graph.downloadUrl"))
            {
                downloadUrl = item.AdditionalData["@microsoft.graph.downloadUrl"].ToString();
            }

            if (!string.IsNullOrEmpty(downloadUrl))
            {
                var url = new Uri(downloadUrl);
                var queryDictionary = System.Web.HttpUtility.ParseQueryString(url.Query);
                result.UniqueId = queryDictionary.Get("UniqueId") ?? string.Empty;
                result.DownloadUrl = $"{url.GetLeftPart(UriPartial.Path)}?UniqueId={result.UniqueId}";
            }
            else
            {
                var reg = new Regex(@"{[a-zA-Z0-9-]+}");
                var matshes = reg.Matches(item.ETag);
                if (matshes.Count > 0)
                {
                    result.UniqueId = matshes[0].Value?.Replace("{", string.Empty)?.Replace("}", string.Empty) ?? string.Empty;
                }
            }

            return result;
        }

        private class DriveItemResponse : IDriveItem
        {
            public string ContentUrl { get; set; }

            public string DownloadUrl { get; set; }

            public string FileName { get; set; }

            public string UniqueId { get; set; }

            public string MimeType { get; set; }

            public string Id { get; set; }
        }
    }
}
