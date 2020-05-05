// <copyright file="HistoryUpdateDialog.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Queue;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Reports;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Common;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Common.Converters;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Extensions;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Helpers;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Models;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Services;

    /// <summary>
    /// Provides core implementation of history extraction waterfall dialog.
    /// </summary>
    /// <seealso cref="BaseDialog" />
    internal class HistoryUpdateDialog : BaseDialog
    {
        protected readonly ILogger logger;
        protected readonly IAppSettings configuration;
        protected readonly IStatePropertyAccessor<ReportParameters> reportStateAccessor;
        private readonly HistoryReportService historyReportService;
        private readonly PrivateChat privateChat;
        private readonly IBackgroundQueue queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryUpdateDialog"/> class.
        /// </summary>
        public HistoryUpdateDialog(IAppSettings configuration, ILogger<HistoryUpdateDialog> logger, ConversationState conversationState, HistoryReportService historyReportService, PrivateChat privateChat, IBackgroundQueue queue)
            : base(nameof(HistoryUpdateDialog), configuration.ConnectionName)
        {
            this.logger = logger;
            this.privateChat = privateChat;
            this.configuration = configuration;
            this.queue = queue;
            reportStateAccessor = conversationState.CreateProperty<ReportParameters>(nameof(ReportParameters));
            this.historyReportService = historyReportService ?? throw new ArgumentNullException(nameof(HistoryUpdateDialog.historyReportService));

            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = ConnectionName,
                    Text = Resources.Strings.DialogAuthCardText,
                    Title = Resources.Strings.DialogSignInButton,
                    Timeout = configuration.SignInTimeout,
                }));

            // Steps of waterfall dialog
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitStepAsync,
                ReportScopeStepAsync,
                ReportTimeRangeStepAsync,
                ReportFileFormatStepAsync,
                ReportAuthCheckingStepAsync,
                PrepareReportAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Ends the dialog with message and sender of activity asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="endMessage">The end message.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task<DialogTurnResult> EndDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken, string endMessage = null)
        {
            if (string.IsNullOrEmpty(endMessage))
            {
                endMessage = Resources.Strings.DialogsCancelledMessage;
            }

            var state = await reportStateAccessor.GetAsync(stepContext.Context, () => null);
            if (state != null && !string.IsNullOrEmpty(state.MessageId))
            {
                IMessageActivity message = AdaptiveCardsHelper.GetMessage(endMessage);
                message.Id = state.MessageId;
                await stepContext.Context.UpdateActivityAsync(message, cancellationToken);
            }
            else
            {
                IMessageActivity message = MessageFactory.Text(Resources.Strings.DialogsCancelledMessage);
                await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }

            await reportStateAccessor.SetAsync(stepContext.Context, null);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> InitStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // initial message
            var txt = stepContext.Context.Activity.Conversation.ConversationType == Constants.ChatConversationType ?
                Resources.Strings.DialogStartChatHistoryProcessMessage :
                Resources.Strings.DialogStartChannelHistoryProcessMessage;

            var message = AdaptiveCardsHelper.GetConfirmation(txt);
            await stepContext.Context.SendActivityAsync(message, cancellationToken);

            await reportStateAccessor.SetAsync(stepContext.Context, new ReportParameters() { MessageId = message.Id });
            return new DialogTurnResult(DialogTurnStatus.Waiting) { ParentEnded = false };
        }

        private async Task<DialogTurnResult> CancelCurrentAndBeginNew(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await EndDialogAsync(stepContext, cancellationToken, Resources.Strings.DialogsCancelledMessage);
            return await stepContext.BeginDialogAsync(InitialDialogId, null, cancellationToken);
        }

        private async Task<DialogTurnResult> ReportScopeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await reportStateAccessor.GetAsync(stepContext.Context, () => new ReportParameters());
            var previousStepResult = CardSubmitResult<bool>.Get(stepContext);
            if (previousStepResult != null)
            {
                if (previousStepResult.Result)
                {
                    var context = stepContext.Context;
                    var activity = context.Activity;
                    state.IsChannel = activity.Conversation.ConversationType == Constants.ChannelConversationType;
                    if (state.IsChannel)
                    {
                        var choices = new List<string>
                        {
                            Resources.Strings.ChannelHistoryOptionAll,
                            Resources.Strings.ChannelHistoryOptionConversation,
                            Resources.Strings.CancelOption,
                        };

                        var message = AdaptiveCardsHelper.GetChoicesPrompt(Resources.Strings.DialogReportScopeMessage, choices);
                        message.Id = state.MessageId;
                        await stepContext.Context.UpdateActivityAsync(message, cancellationToken);
                        await reportStateAccessor.SetAsync(stepContext.Context, state);
                        return new DialogTurnResult(DialogTurnStatus.Waiting) { ParentEnded = false };
                    }
                    else
                    {
                        await reportStateAccessor.SetAsync(stepContext.Context, state);
                        return await NextAsync(stepContext, cancellationToken);
                    }
                }
                else
                {
                    return await EndDialogAsync(stepContext, cancellationToken, Resources.Strings.DialogsCancelledMessage);
                }
            }
            else
            {
                return await CancelCurrentAndBeginNew(stepContext, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ReportTimeRangeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await reportStateAccessor.GetAsync(stepContext.Context, () => null);

            // check dialog state
            if (state == null)
            {
                return await EndDialogAsync(stepContext, cancellationToken);
            }

            var choices = new List<string> { Resources.Strings.TimePeriodOptionAllTime, Resources.Strings.TimePeriodOptionLast7Days, Resources.Strings.TimePeriodOptionLastDay, Resources.Strings.CancelOption };
            var message = AdaptiveCardsHelper.GetChoicesPrompt(Resources.Strings.DialogTimeRangeReportMessage, choices);
            message.Id = state.MessageId;

            if (state.IsChannel)
            {
                var previousStepResult = CardSubmitResult<string>.Get(stepContext);
                if (previousStepResult != null && !string.IsNullOrEmpty(previousStepResult.Result))
                {
                    if (previousStepResult.Result.Equals(Resources.Strings.ChannelHistoryOptionAll))
                    {
                        state.ReportType = ReportSourceType.Channel;
                        await state.FillRestDetailsFromActivity(stepContext.Context, cancellationToken);
                        await stepContext.Context.UpdateActivityAsync(message, cancellationToken);
                        await reportStateAccessor.SetAsync(stepContext.Context, state);
                        return new DialogTurnResult(DialogTurnStatus.Waiting) { ParentEnded = false };
                    }
                    else if (previousStepResult.Result.Equals(Resources.Strings.ChannelHistoryOptionConversation))
                    {
                        state.ReportType = ReportSourceType.Conversation;
                        await state.FillRestDetailsFromActivity(stepContext.Context, cancellationToken);
                        await reportStateAccessor.SetAsync(stepContext.Context, state);

                        return await NextAsync(stepContext, cancellationToken);
                    }
                }
                else
                {
                    return await CancelCurrentAndBeginNew(stepContext, cancellationToken);
                }
            }
            else
            {
                state.ReportType = ReportSourceType.Chat;
                await state.FillRestDetailsFromActivity(stepContext.Context, cancellationToken);
                await stepContext.Context.UpdateActivityAsync(message, cancellationToken);
                await reportStateAccessor.SetAsync(stepContext.Context, state);
                return new DialogTurnResult(DialogTurnStatus.Waiting) { ParentEnded = false };
            }

            return await EndDialogAsync(stepContext, cancellationToken, Resources.Strings.DialogsCancelledMessage);
        }

        private async Task<DialogTurnResult> NextAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Context.Activity.Value = null;
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> ReportFileFormatStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await reportStateAccessor.GetAsync(stepContext.Context, () => null);
            if (state == null)
            {
                return await EndDialogAsync(stepContext, cancellationToken);
            }

            var previousStepResult = CardSubmitResult<string>.Get(stepContext);
            if (state.ReportType == ReportSourceType.Conversation)
            {
                state.ReportPeriod = ReportPeriodType.AllTime;
            }
            else
            {
                if (previousStepResult != null)
                {
                    if (ReportPeriodConverter.TryParse(previousStepResult.Result, out ReportPeriodType period))
                    {
                        state.ReportPeriod = period;
                    }
                    else
                    {
                        return await EndDialogAsync(stepContext, cancellationToken, Resources.Strings.DialogTimePeriodErrorMessage);
                    }
                }
                else
                {
                    return await CancelCurrentAndBeginNew(stepContext, cancellationToken);
                }
            }

            var formats = configuration.ReportFormats.Select(x => ReportFileFormatConverter.GetReportFormat(x)).ToList();
            var message = AdaptiveCardsHelper.GetChoicesPrompt(Resources.Strings.DialogFileTypeReportMessage, formats);
            message.Id = state.MessageId;

            await stepContext.Context.UpdateActivityAsync(message, cancellationToken);
            await reportStateAccessor.SetAsync(stepContext.Context, state);
            return new DialogTurnResult(DialogTurnStatus.Waiting) { ParentEnded = false };
        }

        private async Task<DialogTurnResult> ReportAuthCheckingStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await reportStateAccessor.GetAsync(stepContext.Context, () => null);
            if (state == null)
            {
                return await EndDialogAsync(stepContext, cancellationToken);
            }

            var previousStepResult = CardSubmitResult<string>.Get(stepContext);
            if (previousStepResult != null)
            {
                if (ReportFileFormatConverter.TryParse(previousStepResult.Result, out ReportFormatType format))
                {
                    state.Format = format;
                }
                else
                {
                    return await EndDialogAsync(stepContext, cancellationToken, Resources.Strings.DialogReportFormatErrorMessage);
                }
            }
            else
            {
                return await CancelCurrentAndBeginNew(stepContext, cancellationToken);
            }

            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> PrepareReportAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse != null)
            {
                var context = stepContext.Context;
                var state = await reportStateAccessor.GetAsync(stepContext.Context, () => null);
                if (state == null)
                {
                    return await EndDialogAsync(stepContext, cancellationToken);
                }

                // Send text message as card, because updating card message to plain message is shown in a Teams after a page update (switching team/channel for desktop). Card-to-card update works well.
                var message = AdaptiveCardsHelper.GetMessage(Resources.Strings.DialogFilePreparingMessage);
                message.Id = state.MessageId;
                await stepContext.Context.UpdateActivityAsync(message, cancellationToken);

                queue.QueueBackgroundTask(async token =>
                {
                    try
                    {
                        var file = await historyReportService.PrepareReportInOneDrive(context, state, token);
                        var fileIsReadyMessage = AdaptiveCardsHelper.GetPersonalFileCard(file, Resources.Strings.DialogReportReadyMessage);
                        fileIsReadyMessage.AddMentionToText(context.Activity.From);
                        await privateChat.SendPersonalMessage(context, fileIsReadyMessage, token);

                        message = AdaptiveCardsHelper.GetMessage(MessageHelper.BuildUserExportedHistoryMessage(context, state));
                        message.Id = state.MessageId;
                        await stepContext.Context.UpdateActivityAsync(message, token);
                        await reportStateAccessor.SetAsync(stepContext.Context, null);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Report generation error.", ex);
                        await EndDialogAsync(stepContext, cancellationToken, Resources.Strings.SomethingWentWrong);
                        throw;
                    }
                });

                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }

            return await EndDialogAsync(stepContext, cancellationToken, Resources.Strings.DialogAuthErrorMessage);
        }
    }
}
