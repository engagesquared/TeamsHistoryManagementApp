// <copyright file="AdaptiveCardExtensions.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;

    public static class AdaptiveCardExtensions
    {
        public static Attachment ToAttachment(this AdaptiveCard adaptiveCard)
        {
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard,
            };
            return adaptiveCardAttachment;
        }
    }
}
