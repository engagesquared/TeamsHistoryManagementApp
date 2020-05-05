// <copyright file="ExtractHistoryMessagingExtension.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.MessagingExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AdaptiveCards;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Queue;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Reports;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Common;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Common.Converters;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Dialogs;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Extensions;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Helpers;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Models;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Services;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Implements extract history messaging extension which fetches task module and shows cards for making history report.
    /// Shows Authentication card if this form is required.
    /// Shows Configuration card.
    /// Runs report preparing as background task.
    /// </summary>
    internal class ExtractHistoryMessagingExtension
    {
        private const string CommandId = "extractHistory";
        private readonly string connectionName;
        private readonly PrivateChat privateChat;
        private readonly IBackgroundQueue queue;
        private readonly IAppSettings configuration;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<ExtractHistoryMessagingExtension> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractHistoryMessagingExtension"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="historyReportService">The history report service.</param>
        /// <param name="serviceScopeFactory">The service scope factory for getting scope inside background task.</param>
        /// <param name="privateChat">privateChat</param>
        /// <param name="queue">The queue.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">
        /// HistoryReportService
        /// or
        /// ConnectionName.
        /// </exception>
        public ExtractHistoryMessagingExtension(
            IAppSettings configuration,
            HistoryReportService historyReportService,
            IServiceScopeFactory serviceScopeFactory,
            PrivateChat privateChat,
            IBackgroundQueue queue,
            ILogger<ExtractHistoryMessagingExtension> logger)
        {
            HistoryReportService = historyReportService ?? throw new ArgumentNullException(nameof(HistoryReportService));
            this.privateChat = privateChat;
            this.connectionName = configuration.ConnectionName ?? throw new ArgumentNullException("ConnectionName");
            this.queue = queue;
            this.serviceScopeFactory = serviceScopeFactory;
            this.configuration = configuration;
            this.logger = logger;
        }

        private HistoryReportService HistoryReportService { get; }

        /// <summary>
        /// OnExtractHistoryFetchTaskAsync
        /// </summary>
        /// <param name="turnContext">turnContext</param>
        /// <param name="action">action</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<MessagingExtensionActionResponse> OnExtractHistoryFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            var attachment = AdaptiveTextCard(Resources.Strings.UnsupportedConversation);
            if (turnContext.IsSupportedConversation())
            {
                if (action.CommandId == CommandId)
                {
                    // Auth checking. Login form will be opened if auth required
                    var isAuthenticated = await IsAuthenticated(turnContext, cancellationToken);
                    if (!isAuthenticated)
                    {
                        return await Authenticate(turnContext, action, cancellationToken);
                    }
                    else if (await turnContext.IsBotAddedToTheConversationAsync())
                    {
                        attachment = ExtractHistoryMessagingExtensionCard.Generate(turnContext, configuration.ReportFormats);
                    }
                    else
                    {
                        attachment = ExtractHistoryMessagingExtensionCard.GenerateInstallCard(turnContext);
                    }
                }
            }

            return new MessagingExtensionActionResponse
            {
                Task = this.TaskModuleReportCardTask(turnContext, attachment),
            };
        }

        /// <summary>
        /// This is handler for MessagingExtensionSubmitAction event inside bot for current extension.
        /// Should be called inside bot OnTurnAsync method for MessagingExtensionSubmitAction event.
        /// This methods check a commandId parameter inside context and runs task module if commandId matches current extension commandId.
        /// Triggers report preparing after report settings card submit.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="action">Action.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<MessagingExtensionActionResponse> OnExtractHistorySubmitAction(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            var attachment = AdaptiveTextCard(Resources.Strings.UnsupportedConversation);

            // is this supported conversation
            if (turnContext.IsSupportedConversation())
            {
                if (action.CommandId == CommandId)
                {
                    if (IsBotJustInstalled(turnContext))
                    {
                        attachment = ExtractHistoryMessagingExtensionCard.Generate(turnContext, configuration.ReportFormats);
                    }
                    else
                    {
                        var isignOutAction = turnContext.GetData()?["action"]?.Value<string>() == ExtractHistoryMessagingExtensionCard.SignOutAction;
                        if (isignOutAction)
                        {
                            return await SignOut(turnContext, cancellationToken);
                        }

                        var reportParameters = await GetReportParametersAsync(turnContext);
                        attachment = AdaptiveTextCard(Resources.Strings.MessageExtFilePreparingMessage);

                        // run report preparing as background task because we need close task module for teams.
                        this.queue.QueueBackgroundTask(async token =>
                        {
                            using (var scope = this.serviceScopeFactory.CreateScope())
                            {
                                try
                                {
                                    var file = await HistoryReportService.PrepareReportInOneDrive(turnContext, reportParameters, cancellationToken);
                                    var fileIsReadyMessage = AdaptiveCardsHelper.GetPersonalFileCard(file, Resources.Strings.DialogReportReadyMessage);
                                    fileIsReadyMessage.AddMentionToText(turnContext.Activity.From);
                                    await privateChat.SendPersonalMessage(turnContext, fileIsReadyMessage, cancellationToken);

                                    var updMessage = MessageFactory.Text(MessageHelper.BuildUserExportedHistoryMessage(turnContext, reportParameters));
                                    await turnContext.SendActivityAsync(updMessage, cancellationToken);
                                }
                                catch (Exception ex)
                                {
                                    logger.LogError("Report generation error", ex);
                                    var message = MessageFactory.Text("Error happened during report genaration. Please, try again.");
                                    message.AddMentionToText(turnContext.Activity.From);
                                    await privateChat.SendPersonalMessage(turnContext, message, cancellationToken);
                                }
                            }
                        });
                    }
                }
            }

            return new MessagingExtensionActionResponse
            {
                Task = this.TaskModuleReportCardTask(turnContext, attachment),
            };
        }

        private bool IsBotJustInstalled(ITurnContext<IInvokeActivity> turnContext)
        {
            var data = ((JObject)turnContext.Activity.Value)["data"]?["msteams"]?["justInTimeInstall"]?.Value<bool>();
            return data == true;
        }

        private async Task<bool> IsAuthenticated(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            // When the Bot Service Auth flow completes, the action.State will contain a magic code used for verification.
            var magicCode = turnContext.GetAuthenticationStateCode();
            string authToken = await turnContext.GetUserTokenAsync(this.connectionName, magicCode, cancellationToken);
            var isAuthenticated = !string.IsNullOrEmpty(authToken);
            return isAuthenticated;
        }

        private async Task<MessagingExtensionActionResponse> Authenticate(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction query, CancellationToken cancellationToken)
        {
            // Send the login response with the auth link.
            var botAdapter = (BotFrameworkAdapter)turnContext.Adapter;
            string link = await botAdapter.GetOauthSignInLinkAsync(turnContext, this.connectionName, cancellationToken);
            var response = new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult()
                {
                    Type = "auth",
                    SuggestedActions = new MessagingExtensionSuggestedAction()
                    {
                        Actions = new List<CardAction>()
                        {
                            new CardAction(type: ActionTypes.OpenUrl, title: Resources.Strings.MessageExtSignInTitle, value: link),
                        },
                    },
                },
            };
            return response;
        }

        private async Task<MessagingExtensionActionResponse> SignOut(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var botAdapter = (BotFrameworkAdapter)turnContext.Adapter;
            await botAdapter.SignOutUserAsync(turnContext, configuration.ConnectionName);
            return new MessagingExtensionActionResponse
            {
                Task = this.TaskModuleReportCardTask(turnContext, AdaptiveTextCard(Resources.Strings.SignedOutMessage)),
            };
        }

        private TaskModuleResponseBase TaskModuleReportCardTask(ITurnContext<IInvokeActivity> turnContext, Attachment card)
        {
            return new TaskModuleContinueResponse()
            {
                Type = "continue",
                Value = new TaskModuleTaskInfo()
                {
                    Title = Resources.Strings.MessageExtReportParameters,
                    Card = card,
                },
            };
        }

        private async Task<ReportParameters> GetReportParametersAsync(ITurnContext<IInvokeActivity> turnContext)
        {
            var settings = ((JObject)turnContext.Activity.Value)["data"].Value<JObject>();
            var reportParams = new ReportParameters();

            if (turnContext.Activity.Conversation.ConversationType == Constants.ChannelConversationType)
            {
                var reportType = settings[ExtractHistoryMessagingExtensionCard.ChannelScopeInputId]?.Value<string>() ?? string.Empty;
                if (reportType.Equals(Resources.Strings.ChannelHistoryOptionConversation))
                {
                    reportParams.ReportType = ReportSourceType.Conversation;
                }
                else if (reportType.Equals(Resources.Strings.ChannelHistoryOptionAll))
                {
                    reportParams.ReportType = ReportSourceType.Channel;
                }
            }
            else
            {
                reportParams.ReportType = ReportSourceType.Chat;
            }

            await reportParams.FillRestDetailsFromActivity(turnContext);

            var timeRange = settings[ExtractHistoryMessagingExtensionCard.TimeRangeInputId]?.Value<string>();
            if (ReportPeriodConverter.TryParse(timeRange, out ReportPeriodType period))
            {
                reportParams.ReportPeriod = period;
            }
            else
            {
                SendUnsupportedMessage(turnContext).Wait();
            }

            var formatType = settings[ExtractHistoryMessagingExtensionCard.ReportTypeInputId]?.Value<string>();
            if (ReportFileFormatConverter.TryParse(formatType, out ReportFormatType type))
            {
                reportParams.Format = type;
            }
            else
            {
                SendUnsupportedMessage(turnContext).Wait();
            }

            return reportParams;
        }

        private async Task SendUnsupportedMessage(ITurnContext<IInvokeActivity> turnContext)
        {
            var invRes = new InvokeResponse
            {
                Status = 200,
                Body = new TaskModuleResponse
                {
                    Task = this.TaskModuleReportCardTask(turnContext, AdaptiveTextCard(Resources.Strings.UnsupportedConversation)),
                },
            };

            await turnContext.SendActivityAsync(new Activity
            {
                Value = invRes,
                Type = ActivityTypesEx.InvokeResponse,
            }).ConfigureAwait(false);
        }

        private Attachment AdaptiveTextCard(string text)
        {
            AdaptiveCard adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            adaptiveCard.Body.Add(new AdaptiveTextBlock(text)
            {
                Size = AdaptiveTextSize.Medium,
                Weight = AdaptiveTextWeight.Bolder,
                Wrap = true,
            });

            return adaptiveCard.ToAttachment();
        }
    }
}
