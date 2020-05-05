// <copyright file="IHistoryDialog.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Dialogs
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;

    public interface IHistoryDialog
    {
        /// <summary>
        /// Run Async
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="accessor"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task RunAsync(ITurnContext turnContext, IStatePropertyAccessor<DialogState> accessor, CancellationToken cancellationToken);
    }
}
