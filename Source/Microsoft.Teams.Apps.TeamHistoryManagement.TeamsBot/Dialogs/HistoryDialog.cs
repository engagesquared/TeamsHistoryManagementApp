// <copyright file="HistoryDialog.cs" company="Engage Squared">
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
    using Microsoft.Bot.Builder.Dialogs.Choices;
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
    internal class HistoryDialog : BaseDialog
    {
        protected readonly ILogger logger;
        protected readonly IAppSettings configuration;
        protected readonly IStatePropertyAccessor<ReportParameters> reportStateAccessor;
        private readonly PrivateChat privateChat;
        private readonly IBackgroundQueue queue;

        public HistoryDialog(IAppSettings configuration, ILogger<HistoryDialog> logger, ConversationState conversationState, HistoryReportService historyReportService, PrivateChat privateChat, IBackgroundQueue queue)
            : base(nameof(HistoryDialog), configuration.ConnectionName)
        {
            this.logger = logger;
            this.privateChat = privateChat;
            this.configuration = configuration;
            this.queue = queue;
            reportStateAccessor = conversationState.CreateProperty<ReportParameters>(nameof(ReportParameters));
            HistoryReportService = historyReportService ?? throw new ArgumentNullException(nameof(HistoryReportService));

            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = ConnectionName,
                    Text = Resources.Strings.DialogAuthCardText,
                    Title = Resources.Strings.DialogSignInButton,
                    Timeout = configuration.SignInTimeout,
                }));

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

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

        private HistoryReportService HistoryReportService { get; }

        /// <summary>
        /// Ends the dialog with message and sender of activity asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="endMessage">The end message.</param>
        /// <param name="mentionRecipient">if set to <c>true</c> than sender will be mentioned in reply.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task<DialogTurnResult> EndDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken, string endMessage = null, bool mentionRecipient = false)
        {
            var message = MessageFactory.Text(Resources.Strings.DialogsCancelledMessage);
            if (!string.IsNullOrEmpty(endMessage))
            {
                message = MessageFactory.Text(endMessage);
                if (mentionRecipient)
                {
                    message.AddMentionToText(stepContext.Context.Activity.From);
                }
            }

            await stepContext.Context.SendActivityAsync(message, cancellationToken);
            await this.reportStateAccessor.SetAsync(stepContext.Context, null);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        /// <summary>
        /// Initializes the report configuration waterfall dialog asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task<DialogTurnResult> InitStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var txt = stepContext.Context.Activity.Conversation.ConversationType == Constants.ChatConversationType ?
                Resources.Strings.DialogStartChatHistoryProcessMessage :
                Resources.Strings.DialogStartChannelHistoryProcessMessage;

            return await stepContext.PromptAsync(
                nameof(ConfirmPrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text(txt),
                }, cancellationToken);
        }

        /// <summary>
        /// Initializes the report scope selection step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task<DialogTurnResult> ReportScopeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Check result of previous step
            var confirm = (bool)stepContext.Result;
            if (confirm)
            {
                var context = stepContext.Context;
                var activity = context.Activity;

                if (activity.Conversation.ConversationType == Constants.ChannelConversationType)
                {
                    // This is a channel
                    await this.reportStateAccessor.SetAsync(stepContext.Context, new ReportParameters() { IsChannel = true });

                    return await stepContext.PromptAsync(
                        nameof(ChoicePrompt),
                        new PromptOptions
                        {
                            Prompt = MessageFactory.Text(Resources.Strings.DialogReportScopeMessage),
                            Choices = ChoiceFactory.ToChoices(new List<string>
                            {
                                Resources.Strings.ChannelHistoryOptionAll,
                                Resources.Strings.ChannelHistoryOptionConversation,
                                Resources.Strings.CancelOption,
                            }),
                            Style = ListStyle.HeroCard,
                        },
                        cancellationToken);
                }
                else if (activity.Conversation.ConversationType == Constants.ChatConversationType)
                {
                    // This is a group chat
                    await this.reportStateAccessor.SetAsync(stepContext.Context, new ReportParameters() { IsChannel = false });
                    return await stepContext.NextAsync(true, cancellationToken);
                }
            }

            return await EndDialogAsync(stepContext, cancellationToken);
        }

        /// <summary>
        /// Initializes the history time range selection step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task<DialogTurnResult> ReportTimeRangeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // load dialog state
            var state = await this.reportStateAccessor.GetAsync(stepContext.Context, () => null);

            // check dialog state
            if (state == null)
            {
                return await EndDialogAsync(stepContext, cancellationToken);
            }

            var choices = new List<string> { Resources.Strings.TimePeriodOptionAllTime, Resources.Strings.TimePeriodOptionLast7Days, Resources.Strings.TimePeriodOptionLastDay, Resources.Strings.CancelOption };
            var prompt = new PromptOptions
            {
                Prompt = MessageFactory.Text(Resources.Strings.DialogTimeRangeReportMessage),
                Choices = ChoiceFactory.ToChoices(choices),
                Style = ListStyle.HeroCard,
            };

            // process chat Confirm prompt result
            if (!state.IsChannel && stepContext.Result is bool res)
            {
                // Chat confirmPrompt result
                if (res)
                {
                    state.ReportType = ReportSourceType.Chat;
                    await state.FillRestDetailsFromActivity(stepContext.Context, cancellationToken);
                    await this.reportStateAccessor.SetAsync(stepContext.Context, state);
                    return await stepContext.PromptAsync(nameof(ChoicePrompt), prompt, cancellationToken);
                }
            }

            // process channel ChoicePrompt (Report Scope) result
            if (state.IsChannel && stepContext.Result is FoundChoice choice && !string.IsNullOrEmpty(choice.Value))
            {
                if (choice.Value.Equals(Resources.Strings.ChannelHistoryOptionAll))
                {
                    state.ReportType = ReportSourceType.Channel;
                    await state.FillRestDetailsFromActivity(stepContext.Context, cancellationToken);
                    await this.reportStateAccessor.SetAsync(stepContext.Context, state);
                    return await stepContext.PromptAsync(nameof(ChoicePrompt), prompt, cancellationToken);
                }
                else if (choice.Value.Equals(Resources.Strings.ChannelHistoryOptionConversation))
                {
                    state.ReportType = ReportSourceType.Conversation;
                    await state.FillRestDetailsFromActivity(stepContext.Context, cancellationToken);
                    await this.reportStateAccessor.SetAsync(stepContext.Context, state);
                    return await stepContext.NextAsync();
                }
            }

            return await EndDialogAsync(stepContext, cancellationToken);
        }

        /// <summary>
        /// Initializes the history time range selection step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task<DialogTurnResult> ReportFileFormatStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // load dialog state
            var state = await this.reportStateAccessor.GetAsync(stepContext.Context, () => null);

            // check dialog state
            if (state == null)
            {
                return await EndDialogAsync(stepContext, cancellationToken);
            }

            // process ChoicePrompt (time range) result  from previous step
            if (stepContext.Result is FoundChoice choice)
            {
                if (ReportPeriodConverter.TryParse(choice.Value, out ReportPeriodType period))
                {
                    state.ReportPeriod = period;
                }
                else
                {
                    return await EndDialogAsync(stepContext, cancellationToken, Resources.Strings.DialogTimePeriodErrorMessage);
                }
            }

            // if result is not from ChoicePrompt then check report type
            else if (state.ReportType == ReportSourceType.Conversation)
            {
                state.ReportPeriod = ReportPeriodType.AllTime;
            }
            else
            {
                return await EndDialogAsync(stepContext, cancellationToken, Resources.Strings.DialogTimePeriodErrorMessage, true);
            }

            var formats = configuration.ReportFormats.Select(x => ReportFileFormatConverter.GetReportFormat(x)).ToList();
            var prompt = new PromptOptions
            {
                Prompt = MessageFactory.Text(Resources.Strings.DialogFileTypeReportMessage),
                Choices = ChoiceFactory.ToChoices(formats),
                Style = ListStyle.HeroCard,
            };

            await this.reportStateAccessor.SetAsync(stepContext.Context, state);
            return await stepContext.PromptAsync(nameof(ChoicePrompt), prompt, cancellationToken);
        }

        /// <summary>
        /// Initializes the authentication checking step asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task<DialogTurnResult> ReportAuthCheckingStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // load dialog state
            var state = await this.reportStateAccessor.GetAsync(stepContext.Context, () => null);

            // check dialog state
            if (state == null)
            {
                return await EndDialogAsync(stepContext, cancellationToken);
            }

            if (stepContext.Result is FoundChoice choice && ReportFileFormatConverter.TryParse(choice.Value, out ReportFormatType format))
            {
                state.Format = format;
            }
            else
            {
                return await EndDialogAsync(stepContext, cancellationToken, Resources.Strings.DialogReportFormatErrorMessage, true);
            }

            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        /// <summary>
        /// Initializes report file preparing by report configuration asynchronous.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task<DialogTurnResult> PrepareReportAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse != null)
            {
                var context = stepContext.Context;
                var state = await this.reportStateAccessor.GetAsync(stepContext.Context, () => null);
                if (state == null)
                {
                    return await EndDialogAsync(stepContext, cancellationToken);
                }

                var message = MessageFactory.Text(Resources.Strings.DialogFilePreparingMessage);
                await stepContext.Context.SendActivityAsync(message, cancellationToken);

                queue.QueueBackgroundTask(async token =>
                {
                    try
                    {
                        var file = await HistoryReportService.PrepareReportInOneDrive(context, state, token);
                        var fileIsReadyMessage = AdaptiveCardsHelper.GetPersonalFileCard(file, Resources.Strings.DialogReportReadyMessage);
                        fileIsReadyMessage.AddMentionToText(context.Activity.From);
                        await privateChat.SendPersonalMessage(context, fileIsReadyMessage, token);

                        var updMessage = MessageFactory.Text(MessageHelper.BuildUserExportedHistoryMessage(context, state));
                        updMessage.Id = message.Id;
                        await stepContext.Context.UpdateActivityAsync(updMessage, token);
                        await reportStateAccessor.SetAsync(stepContext.Context, null);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Report generation error.", ex);
                        await EndDialogAsync(stepContext, cancellationToken, Resources.Strings.SomethingWentWrong, true);
                        throw;
                    }
                });

                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            return await EndDialogAsync(stepContext, cancellationToken, Resources.Strings.DialogAuthErrorMessage, true);
        }
    }
}
