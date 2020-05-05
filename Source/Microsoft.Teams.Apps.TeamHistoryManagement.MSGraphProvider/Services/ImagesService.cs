// <copyright file="ImagesService.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.MSGraphProvider.Services
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Downloads images as Base64 encoded srings.
    /// </summary>
    /// <seealso cref="MSGraphProvider.Services.GraphBaseService" />
    internal class ImagesService : GraphBaseService
    {
        public ImagesService(ILogger logger)
            : base(logger)
        {
        }

        public async Task<string> GetBase64Image(Func<Task<string>> tokenProvider, string imageUrl, string graphEndpointBaseUrl)
        {
            HttpWebResponse response = null;
            try
            {
                if (imageUrl.StartsWith(graphEndpointBaseUrl, StringComparison.OrdinalIgnoreCase))
                {
                    response = await Get(tokenProvider, imageUrl);
                }
                else
                {
                    response = await Get(null, imageUrl);
                }
            }
            catch (WebException)
            {
                var status = response?.StatusCode;

                // sometime graph image links are in broken format, fixing it:
                if (status == HttpStatusCode.NotFound && imageUrl.StartsWith(graphEndpointBaseUrl, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var regex = new Regex(@"@thread\.skype;messageid=\d+\/messages\/");
                        var updatedUrl = regex.Replace(imageUrl, "@thread.skype/messages/");
                        response = await Get(tokenProvider, updatedUrl);
                    }
                    catch
                    {
                        // invalid image
                    }
                }
                else
                {
                    // invalid image
                }
            }
            catch
            {
                // invalid image
            }

            if (response != null)
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        responseStream.CopyTo(ms);
                        return $"data:{response.ContentType};base64," + Convert.ToBase64String(ms.ToArray());
                    }
                }
            }

            return imageUrl;
        }
    }
}
