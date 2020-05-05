// <copyright file="PrivateChat.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Dialogs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts;

    public class PrivateChat
    {
        private readonly MicrosoftAppCredentials microsoftAppCredentials;
        private readonly TenantInfo tenantInfo;
        private readonly IAppSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateChat"/> class.
        /// </summary>
        /// <param name="microsoftAppCredentials">microsoftAppCredentials</param>
        /// <param name="tenantInfo">tenantInfo</param>
        /// <param name="settings">IAppSettings</param>
        public PrivateChat(MicrosoftAppCredentials microsoftAppCredentials, TenantInfo tenantInfo, IAppSettings settings)
        {
            this.microsoftAppCredentials = microsoftAppCredentials;
            this.tenantInfo = tenantInfo;
            this.settings = settings;
        }

        /// <summary>
        /// Sends the personal message using bot context and message activity for a user
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="message">The message.</param>
        /// <param name="cancellationToken">Cancelation token</param>
        /// <param name="to">Recipient</param>
        public async Task SendPersonalMessageTo(ITurnContext context, Activity message, CancellationToken cancellationToken, ChannelAccount to)
        {
            var teamsChannelId = context.Activity.ChannelId;
            var botAdapter = (BotFrameworkAdapter)context.Adapter;
            var serviceUrl = context.Activity.ServiceUrl;
            var parameters = new ConversationParameters
            {
                IsGroup = false,
                Bot = context.Activity.Recipient,
                Members = new[] { to },
                TenantId = tenantInfo.Id,
            };

            BotCallbackHandler createConversationcallback = async (createConvContext, createConvCancellationToken) =>
            {
                await createConvContext.SendActivityAsync(message, cancellationToken);
            };
            await botAdapter.CreateConversationAsync(teamsChannelId, serviceUrl, microsoftAppCredentials, parameters, createConversationcallback, cancellationToken);
        }

        /// <summary>
        /// Sends the personal message using bot context and message activity for current user who initiated the action.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="message">The message.</param>
        /// <param name="cancellationToken">Cancelation token</param>
        public async Task SendPersonalMessage(ITurnContext context, Activity message, CancellationToken cancellationToken)
        {
            await SendPersonalMessageTo(context, message, cancellationToken, context.Activity.From);
        }
    }
}
