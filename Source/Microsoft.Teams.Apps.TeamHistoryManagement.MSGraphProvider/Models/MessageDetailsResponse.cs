// <copyright file="MessageDetailsResponse.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.MSGraphProvider.Models
{
    using System.Collections.Generic;
    using Microsoft.Teams.Apps.TeamHistoryManagement.MSGraphProvider.Helpers;
    using Newtonsoft.Json;

    internal class MessageDetailsResponse
    {
        [JsonProperty("@odata.nextLink")]
        public string NextLink { get; set; }

        [JsonProperty("value")]
        public List<MessageDetails> Messages { get; set; }
    }
}
