// <copyright file="ExtractHistoryMessagingExtensionCard.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.MessagingExtensions
{
    using System.Collections.Generic;
    using System.Linq;
    using AdaptiveCards;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Reports;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Common;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Common.Converters;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Extensions;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Helpers;

    /// <summary>
    /// Implements Report Settings Adaptive card for <see cref="ExtractHistoryMessagingExtension"/> messagin extension.
    /// </summary>
    internal static class ExtractHistoryMessagingExtensionCard
    {
        public const string ChannelScopeInputId = "ChannelScopeInputId";
        public const string TimeRangeInputId = "TimeRangeInputId";
        public const string ReportTypeInputId = "ReportTypeInputId";
        public const string GenerateBtnId = "GenerateBtnId";
        public const string GenerateReportAction = "GenerateReportAction";
        public const string SignOutAction = "SignOutAction";

        public static Attachment Generate(ITurnContext turnContext, List<ReportFormatType> reportFormats)
        {
            AdaptiveCard adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            // Scope choice set for channel/ message for group chat
            var scopeContainer = new AdaptiveContainer() { Items = new List<AdaptiveElement>() };
            var conversationType = turnContext.Activity.Conversation.ConversationType;
            if (conversationType == Constants.ChannelConversationType)
            {
                scopeContainer.Items.Add(new AdaptiveTextBlock(Resources.Strings.MessageExtChannelScopeMessage)
                {
                    Size = AdaptiveTextSize.Medium,
                    Weight = AdaptiveTextWeight.Default,
                    Wrap = true,
                });

                var choiceSet = new AdaptiveChoiceSetInput()
                {
                    Id = ChannelScopeInputId,
                    Style = AdaptiveChoiceInputStyle.Expanded,
                    Choices = new List<AdaptiveChoice>()
                    {
                        new AdaptiveChoice()
                        {
                            Value = Resources.Strings.ChannelHistoryOptionAll,
                            Title = Resources.Strings.ChannelHistoryOptionAll,
                        },
                        new AdaptiveChoice()
                        {
                            Value = Resources.Strings.ChannelHistoryOptionConversation,
                            Title = Resources.Strings.ChannelHistoryOptionConversation,
                        },
                    },
                    Value = Resources.Strings.ChannelHistoryOptionAll,
                };
                scopeContainer.Items.Add(choiceSet);
            }

            adaptiveCard.Body.Add(scopeContainer);

            // Timer range choice set
            var timeRangeContainer = new AdaptiveContainer() { Items = new List<AdaptiveElement>() };
            timeRangeContainer.Items.Add(new AdaptiveTextBlock(Resources.Strings.MessageExtTimeRangeReportMessage)
            {
                Size = AdaptiveTextSize.Medium,
                Weight = AdaptiveTextWeight.Default,
                Wrap = true,
            });
            var timeSet = new AdaptiveChoiceSetInput()
            {
                Id = TimeRangeInputId,
                Style = AdaptiveChoiceInputStyle.Expanded,
                Choices = new List<AdaptiveChoice>()
                    {
                        new AdaptiveChoice()
                        {
                            Value = Resources.Strings.TimePeriodOptionAllTime,
                            Title = Resources.Strings.TimePeriodOptionAllTime,
                        },
                        new AdaptiveChoice()
                        {
                            Value = Resources.Strings.TimePeriodOptionLast7Days,
                            Title = Resources.Strings.TimePeriodOptionLast7Days,
                        },
                        new AdaptiveChoice()
                        {
                            Value = Resources.Strings.TimePeriodOptionLastDay,
                            Title = Resources.Strings.TimePeriodOptionLastDay,
                        },
                    },
                Value = Resources.Strings.TimePeriodOptionAllTime,
            };
            timeRangeContainer.Items.Add(timeSet);
            adaptiveCard.Body.Add(timeRangeContainer);

            // File format choice set
            var fileFormatContainer = new AdaptiveContainer() { Items = new List<AdaptiveElement>() };
            fileFormatContainer.Items.Add(new AdaptiveTextBlock(Resources.Strings.MessageExtFileTypeReportMessage)
            {
                Size = AdaptiveTextSize.Medium,
                Weight = AdaptiveTextWeight.Default,
                Wrap = true,
            });
            var reportFormatChoices = reportFormats.Select(x => new AdaptiveChoice()
            {
                Value = ReportFileFormatConverter.GetReportFormat(x),
                Title = ReportFileFormatConverter.GetReportFormat(x),
            }).ToList();
            var formatSet = new AdaptiveChoiceSetInput()
            {
                Id = ReportTypeInputId,
                Style = AdaptiveChoiceInputStyle.Expanded,
                Choices = reportFormatChoices,
                Value = reportFormatChoices.First().Value,
            };
            fileFormatContainer.Items.Add(formatSet);
            adaptiveCard.Body.Add(fileFormatContainer);

            adaptiveCard.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = Resources.Strings.SignOutButtonText,
                Data = new Dictionary<string, string> { ["action"] = SignOutAction },

            });
            adaptiveCard.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = Resources.Strings.MessageExtExtractButton,
                Id = GenerateBtnId,
                Data = new Dictionary<string, string> { ["action"] = GenerateReportAction },
            });


            return adaptiveCard.ToAttachment();
        }

        public static Attachment GenerateInstallCard(ITurnContext turnContext)
        {
            AdaptiveCard adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            adaptiveCard.Body.Add(new AdaptiveTextBlock(Resources.Strings.InstallBotMessageText)
            {
                Size = AdaptiveTextSize.Medium,
                Weight = AdaptiveTextWeight.Default,
                Wrap = true,
            });
            adaptiveCard.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = Resources.Strings.InstallBotButtonText,
                Data = new Dictionary<string, object>
                {
                    // The magic happens here. This tells Teams to add this bot to the current conversation
                    // https://docs.microsoft.com/en-us/microsoftteams/platform/resources/messaging-extension-v3/create-extensions?tabs=typescript#request-to-install-your-conversational-bot
                    // https://stackoverflow.com/questions/58866100/operation-returned-an-invalid-status-code-forbidden-when-calling-getconversati
                    ["msteams"] = new Dictionary<string, bool>
                    {
                        ["justInTimeInstall"] = true,
                    },
                },
            });

            return adaptiveCard.ToAttachment();
        }
    }
}
