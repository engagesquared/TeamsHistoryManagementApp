// <copyright file="TxtGenerator.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.ReportsGenerators.Generators
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Reports;
    using Microsoft.Teams.Apps.TeamHistoryManagement.ReportsGenerators.Models;

    /// <summary>
    /// Implements TEXT file body generation service.
    /// </summary>
    /// <seealso cref="Contracts.Reports.IReportBodyGenerator" />
    public class TxtGenerator : IReportBodyGenerator
    {
        /// <inheritdoc/>
        public ReportFormatType Type => ReportFormatType.TXT;

        /// <summary>
        /// Prepares the report body as byte array by report body details.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <returns></returns>
        public byte[] PrepareDocument(IReportBodyDetails details)
        {
            var threads = MessageSorter.ProcessMessages(details.Messages);
            var report = new StringBuilder();

            var header = PrepareHeaderPart(details);

            report.AppendLine(header).AppendLine(string.Empty);

            threads.ForEach(c => report.AppendLine(PrepareContentPart(c, details.Since, details.IsChannel)));
            return Encoding.UTF8.GetBytes(report.ToString());
        }

        /// <summary>
        /// Prepares the header part.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <returns></returns>
        private string PrepareHeaderPart(IReportBodyDetails details)
        {
            var header = new StringBuilder();
            if (details.IsChannel)
            {
                header.AppendLine($"History bot backup from '{details.TeamName}' team, '{details.ChannelName}' channel");
            }

            if (details.IsConversation)
            {
                header.AppendLine($"History bot backup of conversation(thread) from '{details.TeamName}' team, '{details.ChannelName}' channel");
            }

            if (details.IsGroupChat)
            {
                header.AppendLine($"History bot backup from group chat");
            }

            if (details.Since.HasValue)
            {
                header.AppendLine($"Showing all messages posted between {details.Since.ToInternational()} and {details.Till.ToInternational()}");
            }
            else
            {
                header.AppendLine($"Showing all messages posted till {details.Till.ToInternational()}");
            }

            header.AppendLine($"Generated at {DateTimeOffset.Now.ToInternational()} by {details.Author}");

            return header.ToString();
        }

        /// <summary>
        /// Prepares the content part.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="since">The since.</param>
        /// <param name="threadView">if set to <c>true</c> show messages as thread view.</param>
        /// <returns></returns>
        private string PrepareContentPart(ThreadDetails thread, DateTimeOffset? since, bool threadView = false)
        {
            var mBody = new StringBuilder();
            if (threadView)
            {
                if (thread.IsFull)
                {
                    mBody.AppendLine(MessageDetails(thread.Messages.First(), true, threadView));
                }
                else
                {
                    if (since.HasValue)
                    {
                        mBody.AppendLine(ConversationWarning(since.Value));
                    }

                    mBody.AppendLine(MessageDetails(thread.Messages.First(), false, threadView));
                }

                thread.Messages.Skip(1).ToList().ForEach(c => mBody.AppendLine(MessageDetails(c, false, threadView)));
            }
            else
            {
                if (!thread.IsFull && since.HasValue)
                {
                    mBody.AppendLine(ConversationWarning(since.Value));
                }

                thread.Messages.ForEach(c => mBody.AppendLine(MessageDetails(c, false, threadView)));
            }

            return mBody.ToString();
        }

        private string ConversationWarning(DateTimeOffset since)
        {
            return new StringBuilder()
                .AppendLine($"<<Conversation started before {since.ToInternational()}>>")
                .AppendLine(string.Empty)
                .ToString();
        }

        private string MessageDetails(IMessageDetails message, bool isRoot, bool threadView = false)
        {
            var prefix = isRoot ? string.Empty : threadView ? "            " : "    ";
            var messageTextPrefix = isRoot ? string.Empty : threadView ? ">>          " : ">>  ";

            var str = new StringBuilder();

            if (isRoot && !string.IsNullOrEmpty(message.Subject))
            {
                str.AppendLine($"{prefix}Subject: {message.Subject}");
            }

            str.AppendLine($"{messageTextPrefix}Message text: {message.Body.Content}");
            str.AppendLine($"{prefix}- posted at {message.CreatedDateTime.ToInternational()}");

            if (message.LastModifiedDateTime.HasValue)
            {
                str.AppendLine($"{prefix}- last modified at {message.LastModifiedDateTime.ToInternational()}");
            }

            if (message.DeletedDateTime.HasValue)
            {
                str.AppendLine($"{prefix}- deleted at {message.DeletedDateTime.ToInternational()}");
            }

            if (message.From.Application != null)
            {
                str.AppendLine($"{prefix}- by {message.From.Application.DisplayName}");
            }

            if (message.From.Conversation != null)
            {
                str.AppendLine($"{prefix}- by {message.From.Conversation.DisplayName}");
            }

            if (message.From.User != null)
            {
                str.AppendLine($"{prefix}- by {message.From.User.DisplayName}");
            }

            if (message.From.Device != null)
            {
                str.AppendLine($"{prefix}- by {message.From.Device.DisplayName}");
            }

            str.AppendLine($"{prefix}- priority - {message.Importance}");

            if (message.Reactions != null && message.Reactions.Any())
            {
                var groups = message.Reactions.GroupBy(c => c.ReactionType).ToList();
                str.AppendLine($"{prefix}- reactions: {string.Join(", ", groups.Select(c => $"{c.Key}({c.Count()})").ToList())}");
            }

            if (message.Attachments != null && message.Attachments.Any())
            {
                str.AppendLine($"{prefix}- attachments:");
                message.Attachments.ToList().ForEach(c =>
                {
                    var ctp = !string.IsNullOrEmpty(c.ContentType) ? $"ContentType: {c.ContentType}" : string.Empty;
                    var content = !string.IsNullOrEmpty(c.Content) ?
                        Regex.Replace($"Content: {c.Content?.Replace(Environment.NewLine, " ")} ", @"\s+", " ", RegexOptions.Multiline) :
                        string.Empty;
                    var url = !string.IsNullOrEmpty(c.ContentUrl) ? $"ContentUrl: {c.ContentUrl} " : string.Empty;

                    var name = !string.IsNullOrEmpty(c.Name) ? $"Name: {c.Name} " : string.Empty;
                    var thumbnailUrl = !string.IsNullOrEmpty(c.ThumbnailUrl) ? $"ThumbnailUrl: {c.ThumbnailUrl} " : string.Empty;

                    str.AppendLine($"{prefix}{prefix}{url}{content}{ctp}{name}{thumbnailUrl}");
                });
            }

            return str.ToString();
        }
    }
}
