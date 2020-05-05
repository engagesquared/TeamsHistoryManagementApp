// <copyright file="ReportParameters.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Models
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Reports;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Implements report configuration for working futher history report generation.
    /// </summary>
    internal class ReportParameters
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is channel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is channel; otherwise, <c>false</c>.
        /// </value>
        public bool IsChannel { get; set; }

        /// <summary>
        /// Gets or sets the team identifier.
        /// </summary>
        /// <value>
        /// The team identifier.
        /// </value>
        public string TeamId { get; set; }

        /// <summary>
        /// Gets or sets the chat/channel conversation identifier.
        /// </summary>
        /// <value>
        /// The conversation identifier.
        /// </value>
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or sets the first message in the conversation thread which user selected to export history from.
        /// </summary>
        /// <value>
        /// The message Id.
        /// </value>
        public string ReplyToMessageId { get; set; }

        /// <summary>
        /// Gets or sets the first bot response message id. It is used in update messages approach.
        /// </summary>
        /// <value>
        /// The message Id.
        /// </value>
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets the type of the report - scope of messages.
        /// </summary>
        /// <value>
        /// The type of the report.
        /// </value>
        public ReportSourceType ReportType { get; set; }

        /// <summary>
        /// Gets or sets the report period.
        /// </summary>
        /// <value>
        /// The report period.
        /// </value>
        public ReportPeriodType ReportPeriod { get; set; }

        /// <summary>
        /// Gets or sets the since datetime.
        /// Messages are created later than this date must be included in report.
        /// </summary>
        /// <value>
        /// The since.
        /// </value>
        public DateTimeOffset? Since { get; set; }

        /// <summary>
        /// Gets or sets the till datetime - datetime where report was requested. (now).
        /// </summary>
        /// <value>
        /// The till.
        /// </value>
        public DateTimeOffset Till { get; set; }

        /// <summary>
        /// Gets or sets the format.
        /// </summary>
        /// <value>
        /// The format.
        /// </value>
        public ReportFormatType Format { get; set; }

        public async Task FillRestDetailsFromActivity(ITurnContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (ReportType == ReportSourceType.Chat)
            {
                ConversationId = context.Activity.Conversation.Id;
                IsChannel = false;
            }
            else
            {
                IsChannel = true;
                var channelData = context.Activity.GetChannelData<TeamsChannelData>();
                TeamId = await GetTeamAadId(context, channelData.Team.Id, cancellationToken);
                ConversationId = channelData.Channel.Id;
                if (ReportType == ReportSourceType.Conversation)
                {
                    // need to extract messageId from conversation id. Value example: 19:0e27135a6abf40ae801bbe3164f58d2a@thread.skype;messageid=1584018137394
                    var cId = context.Activity.Conversation.Id;
                    const string token = ";messageid=";
                    var messageId = cId.Substring(cId.IndexOf(token) + token.Length);
                    ReplyToMessageId = messageId;
                }
            }
        }

        private async Task<string> GetTeamAadId(ITurnContext turnContext, string channelDataTeamId, CancellationToken cancellationToken)
        {
            try
            {
                var teamDetails = await TeamsInfo.GetTeamDetailsAsync(turnContext, channelDataTeamId, cancellationToken);
                if (teamDetails != null)
                {
                    return teamDetails.AadGroupId;
                }
            }
            catch
            {
                // Bot is not aaded to the team, Forbidden exception here
            }

            return string.Empty;
        }
    }
}
