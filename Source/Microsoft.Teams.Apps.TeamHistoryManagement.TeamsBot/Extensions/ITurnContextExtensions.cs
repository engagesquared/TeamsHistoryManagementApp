// <copyright file="ITurnContextExtensions.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Extensions
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Rest;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Common;
    using Newtonsoft.Json.Linq;

    public static class ITurnContextExtensions
    {
        /// <summary>
        /// Gets the user token for specified <see cref="ITurnContext"/> by connectionName asynchronous.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="connectionName">Name of the connection.</param>
        /// <param name="secureCode">Te secure code.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<string> GetUserTokenAsync(this ITurnContext context, string connectionName, string secureCode, CancellationToken cancellationToken)
        {
            var botAdapter = (BotFrameworkAdapter)context.Adapter;
            var response = await botAdapter.GetUserTokenAsync(context, connectionName, secureCode, cancellationToken);
            return response?.Token;
        }

        /// <summary>
        /// Gets the user token for specified <see cref="ITurnContext"/> by connectionName and secure code("Magic code") asynchronous.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="connectionName">Name of the connection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<string> GetUserTokenAsync(this ITurnContext context, string connectionName, CancellationToken cancellationToken)
        {
            return await GetUserTokenAsync(context, connectionName, null, cancellationToken);
        }

        /// <summary>
        /// Gets the action.State code used for verification.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A string,  representing the state code.</returns>
        public static string GetAuthenticationStateCode(this ITurnContext context)
        {
            string state = null;
            if (context.Activity?.Value is JObject)
            {
                state = ((JObject)context.Activity.Value)["state"]?.Value<string>();
            }

            return state;
        }

        /// <summary>
        /// Gets the data object from action value
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A JObject</returns>
        public static JObject GetData(this ITurnContext context)
        {
            JObject data = null;
            if (context.Activity?.Value is JObject)
            {
                data = ((JObject)context.Activity.Value)["data"]?.Value<JObject>();
            }

            return data;
        }

        /// <summary>
        /// Determines whether is conversation support by bot.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        ///   <c>true</c> if is supported conversation  by bot; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSupportedConversation(this ITurnContext context)
        {
            var type = context.Activity.Conversation.ConversationType;
            return type == Constants.ChannelConversationType || type == Constants.ChatConversationType || !string.IsNullOrEmpty(context.Activity.Conversation.Id);
        }

        public static async Task<bool> IsBotAddedToTheConversationAsync(this ITurnContext context)
        {
            try
            {
                // https://docs.microsoft.com/en-us/microsoftteams/platform/resources/messaging-extension-v3/create-extensions?tabs=typescript#request-to-install-your-conversational-bot
                // https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/get-teams-context?tabs=dotnet#fetching-the-roster-or-user-profile
                // https://stackoverflow.com/questions/58866100/operation-returned-an-invalid-status-code-forbidden-when-calling-getconversati
                await TeamsInfo.GetPagedMembersAsync(context);
                return true;
            }
            catch (HttpOperationException ex)
            {
                if (ex.Response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return false;
                }

                throw;
            }
        }
    }
}
