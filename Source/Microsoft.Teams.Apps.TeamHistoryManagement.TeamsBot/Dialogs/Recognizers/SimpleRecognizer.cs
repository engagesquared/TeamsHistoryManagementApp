// <copyright file="SimpleRecognizer.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Dialogs.Recognizers
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Common;

    /// <summary>
    /// Implementation of simple recognizer for bot
    /// Recognizer uses a Regular expression for checking text message and ignores case.
    /// </summary>
    /// <seealso cref="Microsoft.Bot.Builder.IRecognizer" />
    internal class SimpleRecognizer : IRecognizer
    {
        /// <summary>
        /// Extracting plain text without mentions from teams activity.
        /// </summary>
        /// <param name="turnContext">turnContext</param>
        public static string GetPlainText(ITurnContext turnContext)
        {
            var activity = turnContext.Activity;
            return activity.RemoveMentionText(activity.Recipient.Id);
        }

        /// <summary>
        /// Runs an utterance through a recognizer and returns a generic recognizer result.
        /// </summary>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// Analysis of utterance.
        /// </returns>
        public async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
              => await RecognizeInternalAsync(turnContext, cancellationToken);

        /// <summary>
        /// Runs an utterance through a recognizer and returns a strongly-typed recognizer result.
        /// </summary>
        /// <typeparam name="T">The recognition result type.</typeparam>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// Analysis of utterance.
        /// </returns>
        public async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            result.Convert(await RecognizeInternalAsync(turnContext, cancellationToken));
            return result;
        }

        private Task<RecognizerResult> RecognizeInternalAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            string text = GetPlainText(turnContext);

            var emptyResult = Task.FromResult(new RecognizerResult()
            {
                Text = string.Empty,
                Intents = new Dictionary<string, IntentScore> { { "None", new IntentScore() { Score = 1.0, } }, },
            });

            if (string.IsNullOrWhiteSpace(text))
            {
                return emptyResult;
            }
            else
            {
                foreach (var intent in IntentStrings.All)
                {
                    var regex = new Regex($@"^{intent}$", RegexOptions.IgnoreCase);
                    var match = regex.Match(text);

                    if (match.Success)
                    {
                        var score = new IntentScore() { Score = 1.0 };
                        var intents = new Dictionary<string, IntentScore> { { intent, score } };

                        return Task.FromResult(new RecognizerResult()
                        {
                            Text = text,
                            Intents = intents,
                        });
                    }
                }

                return emptyResult;
            }
        }
    }
}
