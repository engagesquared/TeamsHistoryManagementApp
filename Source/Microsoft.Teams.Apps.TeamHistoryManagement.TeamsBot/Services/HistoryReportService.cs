// <copyright file="HistoryReportService.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Reports;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Common;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Extensions;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Models;

    /// <summary>
    /// Implements methods for working with teams/channel messages.
    /// </summary>
    internal class HistoryReportService
    {
        private readonly IReportGenerator reportGenerator;
        private readonly IMSGraphClient graphClient;
        private readonly string connectionName;
        private readonly string reportsFolderName;
        private readonly ILogger<HistoryReportService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryReportService"/> class.
        /// </summary>
        /// <param name="reportGenerator">The report generator service for report body preparing.</param>
        /// <param name="graphClient">The graph client.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="logger">The logger implementation.</param>
        public HistoryReportService(IReportGenerator reportGenerator, IMSGraphClient graphClient, IAppSettings configuration, ILogger<HistoryReportService> logger)
        {
            this.reportGenerator = reportGenerator ?? throw new ArgumentNullException(nameof(reportGenerator));
            this.graphClient = graphClient ?? throw new ArgumentNullException(nameof(graphClient));
            this.connectionName = configuration.ConnectionName ?? throw new ArgumentNullException(nameof(configuration.ConnectionName));
            this.reportsFolderName = configuration.ReportsFolderName;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Prepare report and upload it to current user's OneDrive.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IDriveItem> PrepareReportInOneDrive(ITurnContext context, ReportParameters parameters, CancellationToken cancellationToken)
        {
            var messages = await GetMessagesHistoryAsync(context, parameters, cancellationToken);
            var reportBodyDetails = await GetReportBodyDetailsAsync(context, messages, parameters, cancellationToken);
            var reportContent = await GenerateReportByteArray(context, reportBodyDetails, parameters.Format, cancellationToken);
            var onedriveFile = await UploadReportAsync(context, reportBodyDetails, reportContent, parameters.Format, cancellationToken);
            return onedriveFile;
        }

        private async Task<IEnumerable<IMessageDetails>> GetMessagesHistoryAsync(ITurnContext context, ReportParameters parameters, CancellationToken cancellationToken)
        {
            parameters.Till = DateTimeOffset.Now;

            List<IMessageDetails> details = new List<IMessageDetails>();

            switch (parameters.ReportPeriod)
            {
                case ReportPeriodType.LastDay:
                    {
                        parameters.Since = parameters.Till.AddDays(-1);
                        break;
                    }

                case ReportPeriodType.Last7Days:
                    {
                        parameters.Since = parameters.Till.AddDays(-7);
                        break;
                    }
            }

            // Get messages through Graph API using user token delegate
            var data = await graphClient.GetConversationHistoryAsync(() => context.GetUserTokenAsync(this.connectionName, cancellationToken), parameters.TeamId, parameters.ConversationId, parameters.ReplyToMessageId, parameters.Since);
            details.AddRange(data);

            return details;
        }

        private async Task<IReportBodyDetails> GetReportBodyDetailsAsync(ITurnContext context, IEnumerable<IMessageDetails> messages, ReportParameters parameters, CancellationToken cancellationToken)
        {
            var details = new ReportBodyDetails()
            {
                IsChannel = parameters.ReportType == ReportSourceType.Channel,
                IsConversation = parameters.ReportType == ReportSourceType.Conversation,
                IsGroupChat = parameters.ReportType == ReportSourceType.Chat,
                Messages = messages,
                Since = parameters.Since,
                Till = parameters.Till,
                Author = context.Activity.From?.Name,
            };

            if (context.Activity.Conversation.ConversationType == Constants.ChannelConversationType)
            {
                var channelData = context.Activity.GetChannelData<TeamsChannelData>();
                var teamDetails = new TeamDetails(channelData.Team.Id, channelData.Team.Name);
                details.ChannelName = channelData.Channel.Name;
                if (channelData.Team.Id == channelData.Channel.Id)
                {
                    // The channel ID for the General channel always matches the team ID (from MS documentation).
                    // The name of the default General channel is returned as null to allow for localization. (from MS documentation).
                    details.ChannelName = Resources.Strings.TeamGeneralChannelDefaultTitle;
                }

                try
                {
                    teamDetails = await TeamsInfo.GetTeamDetailsAsync(context, channelData.Team.Id, cancellationToken);
                    if (teamDetails != null)
                    {
                        details.TeamName = teamDetails.Name;
                        details.TeamId = teamDetails.AadGroupId;
                        if (string.IsNullOrEmpty(details.ChannelName))
                        {
                            var channels = await TeamsInfo.GetTeamChannelsAsync(context, channelData.Team.Id, cancellationToken);
                            details.ChannelName = channels?.FirstOrDefault(x => channelData.Channel.Id.Equals(x.Id))?.Name;
                        }
                    }
                }
                catch
                {
                    // Bot is not aaded to the team, Forbidden exception here
                    details.TeamName = channelData.Team.Id; // Team name is not available here
                    details.TeamId = channelData.Team.Id;
                    logger.LogWarning($"Bot is not added to team {channelData.Team.Id}");
                }

                if (string.IsNullOrEmpty(details.ChannelName))
                {
                    // Fill it with channel id if name is not available.
                    details.ChannelName = channelData.Channel.Id;
                }
            }

            return details;
        }

        private async Task<byte[]> GenerateReportByteArray(ITurnContext context, IReportBodyDetails reportBodyDetails, ReportFormatType format, CancellationToken cancellationToken)
        {
            switch (format)
            {
                case ReportFormatType.HTML:
                case ReportFormatType.PDF:
                    {
                        await graphClient.DownloadImages(() => context.GetUserTokenAsync(this.connectionName, cancellationToken), reportBodyDetails.Messages);
                        break;
                    }
            }

            return reportGenerator.PrepareDocument(reportBodyDetails, format);
        }

        private async Task<IDriveItem> UploadReportAsync(ITurnContext context, IReportBodyDetails reportDetails, byte[] reportBytes, ReportFormatType format, CancellationToken cancellationToken)
        {
            IDriveItem file = null;
            var folderName = string.IsNullOrEmpty(reportsFolderName) ? string.Empty : (reportsFolderName + "/");
            var fileName = $"{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}_history_report";

            var reportBytesToUpload = reportBytes;
            switch (format)
            {
                case ReportFormatType.TXT:
                    {
                        fileName = $"{fileName}_.txt";
                        break;
                    }

                case ReportFormatType.JSON:
                    {
                        fileName = $"{fileName}_.json";
                        break;
                    }

                case ReportFormatType.HTML:
                    {
                        fileName = $"{fileName}_.html";
                        break;
                    }

                case ReportFormatType.PDF:
                    {
                        fileName = $"{fileName}_.pdf";
                        break;
                    }
            }

            file = await UploadFile(reportBytesToUpload, reportDetails, $"{folderName}{fileName}", context, cancellationToken);
            file.FileName = fileName;
            return file;
        }

        private async Task<IDriveItem> UploadFile(byte[] byteArray, IReportBodyDetails reportDetails, string fileName, ITurnContext context, CancellationToken cancellationToken)
        {
            using (var stream = new MemoryStream(byteArray))
            {
                stream.Position = 0;
                stream.Flush();
                IDriveItem file =
                    await graphClient.UploadFileInPersonalOneDrive(() => context.GetUserTokenAsync(this.connectionName, cancellationToken), stream, fileName);
                return file;
            }
        }
    }
}
