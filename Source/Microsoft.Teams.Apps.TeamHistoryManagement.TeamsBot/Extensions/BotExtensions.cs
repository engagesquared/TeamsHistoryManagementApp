// <copyright file="BotExtensions.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Helpers
{
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;

    /// <summary>
    ///Implements extension methods for Bot.
    /// </summary>
    public static class BotExtensions
    {
        public static Attachment ToAttachment(this FileInfoCard card, string filename, string contentUrl)
        {
            return new Attachment
            {
                Content = card,
                ContentType = FileInfoCard.ContentType,
                Name = filename,
                ContentUrl = contentUrl,
            };
        }
    }
}
