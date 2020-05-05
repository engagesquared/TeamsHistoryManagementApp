// <copyright file="GraphBaseService.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.MSGraphProvider.Services
{
    using System;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The base class with requests implementation.
    /// </summary>
    internal abstract class GraphBaseService
    {
        protected readonly ILogger Logger;

        public GraphBaseService(ILogger logger)
        {
            this.Logger = logger;
        }

        /// <summary>
        /// Makes get request
        /// </summary>
        /// <param name="tokenProvider">tokenProvider</param>
        /// <param name="url">url</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<HttpWebResponse> Get(Func<Task<string>> tokenProvider, string url)
        {
            return await RequestBaseAsync(tokenProvider, url, "GET");
        }

        private async Task<HttpWebResponse> RequestBaseAsync(Func<Task<string>> tokenProvider, string url, string method, string postData = "", string contentType = "", int maxRetry = 8)
        {
            HttpWebResponse response = null;
            try
            {
                response = await MakeRequest(tokenProvider, url, method, postData, contentType);
                return response;
            }
            catch (WebException ex)
            {
                var statusCode = (ex.Response as HttpWebResponse)?.StatusCode;
                if (maxRetry > 0)
                {
                    var isError5xx = statusCode.HasValue
                        && (int)statusCode.Value >= 500
                        && (int)statusCode.Value <= 600;
                    if (statusCode == HttpStatusCode.TooManyRequests || isError5xx)
                    {
                        var retryHeaderValue = ex.Response.Headers["Retry-After"];
                        var retryAfter = GetDelayMsec(retryHeaderValue);
                        await Task.Delay(retryAfter);
                        return await RequestBaseAsync(tokenProvider, url, method, postData, contentType, maxRetry - 1);
                    }
                }

                var idHeader = ex.Response.Headers["request-id"];
                var timestamp = ex.Response.Headers["timestamp"];
                var diagnostic = ex.Response.Headers["x-ms-ags-diagnostic"];
                Logger.LogError(
                    $"Graph request error. Url: '{url}', method '{method}', " +
                    $"request-id header: '{idHeader}', timestamp header: '{timestamp}', diagnostics header: '{diagnostic}'", ex);
                throw;
            }
        }

        private int GetDelayMsec(string retryAfter)
        {
            int delay = 3000;
            if (int.TryParse(retryAfter, out int afterSeconds))
            {
                delay = afterSeconds * 1000;
            }

            return delay;
        }

        private async Task<HttpWebResponse> MakeRequest(Func<Task<string>> tokenProvider, string url, string method, string postData, string contentType)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            if (!string.IsNullOrWhiteSpace(contentType))
            {
                request.ContentType = contentType;
            }

            if (tokenProvider != null)
            {
                request.Headers.Add("Authorization", "bearer " + await tokenProvider());
            }

            if (method == "POST" || method == "PATCH" || method == "PUT" || method == "DELETE")
            {
                if (!string.IsNullOrWhiteSpace(postData))
                {
                    byte[] postBytes = new ASCIIEncoding().GetBytes(postData);
                    var postStream = request.GetRequestStream();
                    postStream.Write(postBytes, 0, postBytes.Length);
                    postStream.Flush();
                    postStream.Close();
                }
                else
                {
                    request.ContentLength = 0;
                }
            }

            var response = (HttpWebResponse)await request.GetResponseAsync();
            return response;
        }
    }
}
