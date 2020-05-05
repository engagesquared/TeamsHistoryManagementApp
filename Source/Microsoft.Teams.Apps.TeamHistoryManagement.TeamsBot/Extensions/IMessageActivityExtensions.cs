// <copyright file="IMessageActivityExtensions.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Extensions
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Bot.Schema;

    public static class IMessageActivityExtensions
    {
        /// <summary>
        /// Adds the mention text to an existing activity.
        /// </summary>
        /// <typeparam name="T">Message activity type.</typeparam>
        /// <param name="activity">The activity.</param>
        /// <param name="mentionedUser">The mentioned user.</param>
        /// <param name="prependMention">if set to <c>true</c> [prepend mention].</param>
        /// <param name="mentionText">The mention text.</param>
        /// <returns>
        /// Activity with added mention.
        /// </returns>
        /// <exception cref="ArgumentNullException">mentionedUser - Mentioned user and user ID cannot be null.</exception>
        /// <exception cref="ArgumentException">Either mentioned user name or mentionText must have a value.</exception>
        /// <exception cref="T:Microsoft.Rest.ValidationException">Mentioned user name or mentionText must have a value.</exception>
        public static T AddMentionToText<T>(this T activity, ChannelAccount mentionedUser, bool prependMention = true, string mentionText = null)
            where T : IMessageActivity
        {
            if (mentionedUser == null || string.IsNullOrEmpty(mentionedUser.Id))
            {
                throw new ArgumentNullException(nameof(mentionedUser), "Mentioned user and user ID cannot be null");
            }

            if (string.IsNullOrEmpty(mentionedUser.Name) && string.IsNullOrEmpty(mentionText))
            {
                throw new ArgumentException("Either mentioned user name or mentionText must have a value");
            }

            if (!string.IsNullOrWhiteSpace(mentionText))
            {
                mentionedUser.Name = mentionText;
            }

            string str = string.Format("<at>{0}</at>", (object)mentionedUser.Name);
            if (!prependMention)
            {
                activity.Text = $"{activity.Text} {str}";
            }
            else
            {
                activity.Text = $"{str} {activity.Text}";
            }

            if (activity.Entities == null)
            {
                activity.Entities = new List<Entity>();
            }

            IList<Entity> entities = activity.Entities;
            Mention mention = new Mention
            {
                Text = str,
                Mentioned = mentionedUser,
            };

            entities.Add(mention);

            return activity;
        }
    }
}
