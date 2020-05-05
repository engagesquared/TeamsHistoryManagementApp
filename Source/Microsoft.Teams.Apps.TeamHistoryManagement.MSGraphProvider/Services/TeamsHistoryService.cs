// <copyright file="TeamsHistoryService.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.MSGraphProvider.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.TeamHistoryManagement.MSGraphProvider.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// Implements teams history service.
    /// </summary>
    /// <seealso cref="MSGraphProvider.Services.GraphBaseService" />
    internal class TeamsHistoryService : GraphBaseService
    {
        private const string MaxPageSizeQuery = "?&top=50";

        public TeamsHistoryService(ILogger logger)
            : base(logger)
        {
        }

        /// <summary>
        /// GetConversationHistory
        /// </summary>
        /// <param name="tokenProvider">tokenProvider</param>
        /// <param name="teamId">teamId</param>
        /// <param name="conversationId">conversationId</param>
        /// <param name="messageId">message id</param>
        /// <param name="date">date</param>
        /// <param name="graphEndpointUrl">graphEndpointUrl</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<List<MessageDetails>> GetConversationHistory(Func<Task<string>> tokenProvider, string teamId, string conversationId, string messageId, DateTimeOffset? date, string graphEndpointUrl)
        {
            if (date.HasValue && date.Value > DateTimeOffset.Now)
            {
                return new List<MessageDetails>();
            }

            if (string.IsNullOrEmpty(teamId))
            {
                return await GetFromChatsApi(tokenProvider, conversationId, date, graphEndpointUrl);
            }
            else
            {
                return await GetDromTeamsApi(tokenProvider,teamId, conversationId, messageId, date, graphEndpointUrl);
            }
        }

        private async Task<List<MessageDetails>> GetDromTeamsApi(Func<Task<string>> tokenProvider, string teamId, string conversationId, string messageId, DateTimeOffset? date, string graphEndpointUrl)
        {
            var results = new List<MessageDetails>();
            var url = $"{graphEndpointUrl}/teams/{teamId}/channels/{conversationId}/messages";

            // load channel root messages
            if (!string.IsNullOrEmpty(messageId))
            {
                url = url + $"/{messageId}";
                results.Add(await GetMessageDetails(tokenProvider, url));
            }
            else
            {
                url = url + MaxPageSizeQuery;
                results.AddRange(await GetMessagesDetailsPaged(tokenProvider, url, date));
            }

            // expand message replies
            var messagesToExpand = results.ToList();
            foreach (var messageToExpan in messagesToExpand)
            {
                url = $"{graphEndpointUrl}/teams/{teamId}/channels/{conversationId}/messages/{messageToExpan.Id}/replies/{MaxPageSizeQuery}";
                results.AddRange(await GetMessagesDetailsPaged(tokenProvider, url, null));
            }

            return results;
        }

        private async Task<List<MessageDetails>> GetFromChatsApi(Func<Task<string>> tokenProvider, string conversationId, DateTimeOffset? date, string graphEndpointUrl)
        {
            var url = $"{graphEndpointUrl}/chats/{conversationId}/messages{MaxPageSizeQuery}";
            return await GetMessagesDetailsPaged(tokenProvider, url, date);
        }


        private async Task<MessageDetails> GetMessageDetails(Func<Task<string>> tokenProvider, string url)
        {
            using (var response = await Get(tokenProvider, url))
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        var str = reader.ReadToEnd();
                        var res = JsonConvert.DeserializeObject<MessageDetails>(str);
                        return res;
                    }
                }
            }
        }

        private async Task<List<MessageDetails>> ExpandMessageRepliesPaged(Func<Task<string>> tokenProvider, string url)
        {
            var results = new List<MessageDetails>();
            var nextLink = url;
            var shouldRequestNextPage = true;
            do
            {
                using (var response = await Get(tokenProvider, nextLink))
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            var str = reader.ReadToEnd();

                            var res = JsonConvert.DeserializeObject<MessageDetailsResponse>(str);
                            nextLink = res.NextLink;
                        }
                    }
                }
            }
            while (!string.IsNullOrEmpty(nextLink) && shouldRequestNextPage);

            return results;
        }

        private async Task<List<MessageDetails>> GetMessagesDetailsPaged(Func<Task<string>> tokenProvider, string url, DateTimeOffset? date)
        {
            var results = new List<MessageDetails>();
            var nextLink = url;
            var shouldRequestNextPage = true;
            do
            {
                using (var response = await Get(tokenProvider, nextLink))
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            var str = reader.ReadToEnd();

                            var res = JsonConvert.DeserializeObject<MessageDetailsResponse>(str);
                            nextLink = res.NextLink;

                            if (date.HasValue)
                            {
                                var filtered = res.Messages.Where(c => DateTimeOffset.Compare(c.CreatedDateTime, date.Value) >= 0).ToList();
                                shouldRequestNextPage = res.Messages.Any() && !res.Messages.Where(c => DateTimeOffset.Compare(c.CreatedDateTime, date.Value) < 0).Any();
                                results.AddRange(filtered);
                            }
                            else
                            {
                                shouldRequestNextPage = res.Messages.Any();
                                results.AddRange(res.Messages);
                            }
                        }
                    }
                }
            }
            while (!string.IsNullOrEmpty(nextLink) && shouldRequestNextPage);

            return results;
        }
    }
}
