// <copyright file="AdaptiveCardsHelper.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Helpers
{
    using System.Collections.Generic;
    using System.IO;
    using AdaptiveCards;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Common;

    public class AdaptiveCardsHelper
    {
        public static IMessageActivity GetConfirmation(string text)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            card.Body.Add(new AdaptiveTextBlock(text) { Wrap = true });
            card.Actions.Add(new AdaptiveSubmitAction() { Title = "Yes", Data = new CardSubmitResult<bool> { Result = true } });
            card.Actions.Add(new AdaptiveSubmitAction() { Title = "No", Data = new CardSubmitResult<bool> { Result = false } });
            return ToMessage(card);
        }

        public static IMessageActivity GetChoicesPrompt(string text, List<string> choices)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            card.Body.Add(new AdaptiveTextBlock(text) { Wrap = true });
            choices.ForEach(ch =>
            {
                card.Actions.Add(new AdaptiveSubmitAction() { Title = ch, Data = new CardSubmitResult<string> { Result = ch } });
            });
            return ToMessage(card);
        }

        public static Activity GetPersonalFileCard(IDriveItem file, string text)
        {
            var card = new FileInfoCard()
            {
                FileType = Path.GetExtension(file.FileName).Replace(".", string.Empty),
                UniqueId = file.UniqueId,
            };
            var attachment = card.ToAttachment(file.FileName, file.ContentUrl);
            var message = MessageFactory.Text(Resources.Strings.DialogReportReadyMessage);
            message.Attachments.Add(attachment);
            return message;
        }

        public static IMessageActivity GetMessage(string text)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            card.Body.Add(new AdaptiveTextBlock(text) { Wrap = true });
            return ToMessage(card);
        }

        private static IMessageActivity ToMessage(AdaptiveCard card)
        {
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
            return MessageFactory.Attachment(adaptiveCardAttachment);
        }
    }
}
