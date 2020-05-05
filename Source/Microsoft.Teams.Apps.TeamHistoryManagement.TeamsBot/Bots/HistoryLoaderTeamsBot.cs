// <copyright file="HistoryLoaderTeamsBot.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Dialogs;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Extensions;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.MessagingExtensions;

    /// <summary>
    /// Implements exstraction history bot with report configuration dialog and messaging extension.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="TeamsBot.Bots.DialogBot{T}" />
    internal class HistoryLoaderTeamsBot : TeamsActivityHandler
    {
        protected readonly BotState ConversationState;
        protected readonly IHistoryDialog Dialog;
        protected readonly ILogger Logger;
        private readonly ExtractHistoryMessagingExtension extractHistoryMessagingExtension;
        private readonly PrivateChat privateChat;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryLoaderTeamsBot"/> class.
        /// </summary>
        /// <param name="conversationState">State of the conversation.</param>
        /// <param name="dialog">The dialog.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="extension">The extract history message extension.</param>
        /// <param name="privateChat">Private chat instance.</param>
        /// <exception cref="ArgumentNullException">ExtractHistoryMessagingExtension.</exception>
        public HistoryLoaderTeamsBot(ConversationState conversationState, IHistoryDialog dialog, ILogger<Dialog> logger, ExtractHistoryMessagingExtension extension, PrivateChat privateChat)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(ConversationState));
            Dialog = dialog ?? throw new ArgumentNullException(nameof(Dialog));
            Logger = logger ?? throw new ArgumentNullException(nameof(Logger));
            extractHistoryMessagingExtension = extension ?? throw new ArgumentNullException(nameof(extractHistoryMessagingExtension));
            this.privateChat = privateChat;
        }

        /// <summary>
        /// The OnTurnAsync function is called by the Adapter (for example, the <see cref="T:Microsoft.Bot.Builder.BotFrameworkAdapter" />)
        /// at runtime in order to process an inbound Activity.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        /// <summary>
        /// Invoked when a message activity is received from the user when the base behavior of
        /// <see cref="!:OnTurnAsync(ITurnContext&lt;IConversationUpdateActivity&gt;, CancellationToken)" /> is used.
        /// If overridden, this could potentially contain conversational logic.
        /// By default, this method does nothing.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.IsSupportedConversation())
            {
                Logger.LogInformation("Running dialog with Message Activity.");
                var state = ConversationState.CreateProperty<DialogState>(nameof(DialogState));
                await Dialog.RunAsync(turnContext, state, cancellationToken);
            }
            else
            {
                await SendUnsupportedConverationMessage(turnContext);
            }
        }

        /// <summary>
        /// Called asynchronous when request is a signin state verification query.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected override async Task OnTeamsSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.IsSupportedConversation())
            {
                Logger.LogInformation("Running dialog with signin/verifystate from an Invoke Activity.");

                // The OAuth Prompt needs to see the Invoke Activity in order to complete the login process.

                // Run the Dialog with the new Invoke Activity.
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
            }
            else
            {
                await SendUnsupportedConverationMessage(turnContext);
            }
        }

        /// <summary>
        /// Called asynchronous when request is a messaging extension action for fetch task .
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="action">action</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            return await extractHistoryMessagingExtension.OnExtractHistoryFetchTaskAsync(turnContext, action, cancellationToken);
        }

        /// <summary>
        /// Called asynchronous when request is a messaging extension action for submit action.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="action">action</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
             return await extractHistoryMessagingExtension.OnExtractHistorySubmitAction(turnContext, action, cancellationToken);
        }

        /// <summary>
        /// Invoked when members other than this bot (like a user) are added to the conversation when the base behavior of
        /// <see cref="M:Microsoft.Bot.Builder.ActivityHandler.OnConversationUpdateActivityAsync(Microsoft.Bot.Builder.ITurnContext{Microsoft.Bot.Schema.IConversationUpdateActivity},System.Threading.CancellationToken)" /> is used.
        /// If overridden, this could potentially send a greeting message to the user instead of waiting for the user to send a message first.
        /// By default, this method does nothing.
        /// </summary>
        /// <param name="teamsMembersAdded">teamsMembersAdded</param>
        /// <param name="teamInfo">teamInfo</param>
        /// <param name="turnContext">turnContext</param>
        /// <param name="cancellationToken">cancellationToken</param>
        protected override async Task OnTeamsMembersAddedAsync(IList<TeamsChannelAccount> teamsMembersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.IsSupportedConversation())
            {
                if (teamInfo != null)
                {
                    foreach (var member in teamsMembersAdded)
                    {
                        try
                        {
                            if (member.Id == turnContext.Activity.Recipient.Id)
                            {
                                // Bot itself added to the team, skipping it.
                                continue;
                            }

                            var message = MessageFactory.Text($"Hey new member! I am a chat history bot. You have been added to the team '{teamInfo.Name}'. You can ask me to prepare message history reports. To start the process, just mention my name in a channel.");
                            await privateChat.SendPersonalMessageTo(turnContext, message, cancellationToken, member);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.LogError("Welcome message sending error", ex);
                        }
                    }
                }
            }
        }

        private async Task SendUnsupportedConverationMessage(ITurnContext turnContext)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text(Resources.Strings.UnsupportedConversation));
        }
    }
}
