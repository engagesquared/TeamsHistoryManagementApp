// <copyright file="BaseDialog.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Dialogs
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Common;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Dialogs.Recognizers;

    public class BaseDialog : ComponentDialog, IHistoryDialog
    {
        private readonly SimpleRecognizer simpleRecognizer = new SimpleRecognizer();

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDialog"/> class.
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="connectionName">connectionName</param>
        public BaseDialog(string id, string connectionName)
            : base(id)
        {
            ConnectionName = connectionName;
        }

        protected string ConnectionName { get; }

        /// <inheritdoc/>
        public Task RunAsync(ITurnContext turnContext, IStatePropertyAccessor<DialogState> accessor, CancellationToken cancellationToken)
        {
            return DialogExtensions.RunAsync(this, turnContext, accessor, cancellationToken);
        }

        /// <inheritdoc/>
        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }

        /// <inheritdoc/>
        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        private async Task<DialogTurnResult> InterruptAsync(DialogContext dialogContext, CancellationToken cancellationToken = default)
        {
            var activity = dialogContext.Context.Activity;
            if (activity.Type == ActivityTypes.Message && !string.IsNullOrEmpty(activity.Text))
            {
                var intent = (await this.simpleRecognizer.RecognizeAsync(dialogContext.Context, cancellationToken)).GetTopScoringIntent().intent;

                if (IntentStrings.LogoutCommands.Contains(intent))
                {
                    var botAdapter = (BotFrameworkAdapter)dialogContext.Context.Adapter;
                    await botAdapter.SignOutUserAsync(dialogContext.Context, ConnectionName, null, cancellationToken);
                    await dialogContext.Context.SendActivityAsync(MessageFactory.Text(Resources.Strings.SignedOutMessage), cancellationToken);
                    return await dialogContext.CancelAllDialogsAsync(cancellationToken);
                }

                if (IntentStrings.HelpCommands.Contains(intent))
                {
                    await dialogContext.Context.SendActivityAsync(MessageFactory.Text(Resources.Strings.DialogHelpMessage), cancellationToken);
                    return await dialogContext.CancelAllDialogsAsync(cancellationToken);
                }

                if (IntentStrings.CancelCommands.Contains(intent))
                {
                    await dialogContext.Context.SendActivityAsync(MessageFactory.Text(Resources.Strings.DialogsCancelledMessage), cancellationToken);
                    return await dialogContext.CancelAllDialogsAsync(cancellationToken);
                }
            }

            return null;
        }
    }
}
