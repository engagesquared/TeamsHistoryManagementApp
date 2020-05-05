// <copyright file="HtmlGenerator.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.ReportsGenerators.Generators
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using HtmlAgilityPack;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Reports;

    /// <summary>
    /// Implements the HTML file body generation service.
    /// </summary>
    /// <seealso cref="Contracts.Reports.IReportBodyGenerator" />
    public class HtmlGenerator : IReportBodyGenerator
    {
        /// <inheritdoc/>
        public ReportFormatType Type => ReportFormatType.HTML;

        /// <summary>
        /// Prepares the report body as byte array by report body details.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <returns></returns>
        public byte[] PrepareDocument(IReportBodyDetails details)
        {
            var html = PrepareHtml(details);
            return Encoding.UTF8.GetBytes(html);
        }

        protected string PrepareHtml(IReportBodyDetails details)
        {
            var template = File.ReadAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Templates\ReportTemplateMain.html");
            var messageTemplate = File.ReadAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Templates\ReportTemplateMessage.html");
            HtmlDocument report = new HtmlDocument();
            report.LoadHtml(template);
            PrepareHeader(details, report);
            PrepareBody(details, report, messageTemplate);
            return report.DocumentNode.OuterHtml;
        }

        /// <summary>
        /// Prepares the header.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <param name="report">The report.</param>
        protected void PrepareHeader(IReportBodyDetails details, HtmlDocument report)
        {
            string headerTypeText = string.Empty;
            if (details.IsChannel)
            {
                headerTypeText = $"History bot backup from {details.TeamName}, {details.ChannelName} Channel";
            }

            if (details.IsConversation)
            {
                headerTypeText = $"History bot backup of conversation(thread) from {details.TeamName}, {details.ChannelName} Channel";
            }

            if (details.IsGroupChat)
            {
                headerTypeText = $"History bot backup from group chat";
            }

            report.GetElementbyId("report-type").AppendChild(HtmlNode.CreateNode(headerTypeText));

            var headerRange = string.Empty;
            if (details.Since.HasValue)
            {
                headerRange = $"Showing all messages posted between {details.Since.ToInternational()} and {details.Till.ToInternational()}";
            }
            else
            {
                headerRange = $"Showing all messages posted till {details.Till.ToInternational()}";
            }

            report.GetElementbyId("report-time-range").AppendChild(HtmlNode.CreateNode(headerRange));
            report.GetElementbyId("report-generation-details").AppendChild(HtmlNode.CreateNode($"Generated at {DateTimeOffset.Now.ToInternational()} by {details.Author}"));
        }

        /// <summary>
        /// Prepares the body.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <param name="doc">The document.</param>
        /// <param name="messageTemplate">The message template.</param>
        protected void PrepareBody(IReportBodyDetails details, HtmlDocument doc, string messageTemplate)
        {
            var threadsNode = doc.GetElementbyId("history");

            var threads = MessageSorter.ProcessMessages(details.Messages);

            foreach (var thread in threads)
            {
                threadsNode.SetAttributeValue("class", "threads-container");

                if (details.IsChannel)
                {
                    if (thread.IsFull)
                    {
                        threadsNode.AppendChild(PrepareMessageDetails(thread.Messages.First(), true, messageTemplate, details.IsChannel));
                    }
                    else
                    {
                        if (details.Since.HasValue)
                        {
                            threadsNode.AppendChild(CreateConversationWarning(details.Since.Value));
                        }

                        threadsNode.AppendChild(PrepareMessageDetails(thread.Messages.First(), false, messageTemplate, details.IsChannel));
                    }

                    thread.Messages.Skip(1).ToList().ForEach(c => threadsNode.AppendChild(PrepareMessageDetails(c, false, messageTemplate, details.IsChannel)));
                }
                else
                {
                    if (!thread.IsFull && details.Since.HasValue)
                    {
                        threadsNode.AppendChild(CreateConversationWarning(details.Since.Value));
                    }

                    thread.Messages.ForEach(c => threadsNode.AppendChild(PrepareMessageDetails(c, false, messageTemplate, details.IsChannel)));
                }
            }
        }

        /// <summary>
        /// Creates the conversation warning.
        /// </summary>
        /// <param name="since">The since.</param>
        /// <returns></returns>
        protected HtmlNode CreateConversationWarning(DateTimeOffset since)
        {
            return HtmlNode.CreateNode($"<div class=\"conversation-warn\">Conversation started before {since.ToInternational()}</div>");
        }

        /// <summary>
        /// Prepares the message details.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="isRoot">if set to <c>true</c> [is root].</param>
        /// <param name="messageTemplate">The message template.</param>
        /// <param name="threadView">if set to <c>true</c> [thread view].</param>
        /// <returns></returns>
        protected HtmlNode PrepareMessageDetails(IMessageDetails message, bool isRoot, string messageTemplate, bool threadView = false)
        {
            HtmlDocument messageHtml = new HtmlDocument();
            messageHtml.LoadHtml(messageTemplate);

            // Root element of message
            var messageNode = messageHtml.GetElementbyId("message-ctn");
            if (messageNode != null)
            {
                // type of message -> for class
                string messageNodeClass = string.Empty;
                if (isRoot && threadView)
                {
                    messageNodeClass = "root-thread-message";
                }
                else if (threadView)
                {
                    messageNodeClass = "thread-message";
                }
                else
                {
                    messageNodeClass = "no-thread-message";
                }

                messageNode.SetAttributeValue("class", messageNodeClass);
                messageNode.Attributes["id"].Remove();

                var messageContentNode = messageHtml.GetElementbyId("message-content");
                if (messageContentNode != null)
                {
                    messageContentNode.SetAttributeValue("class", $"message-content-{message.Importance}");
                    messageContentNode.Attributes["id"].Remove();
                }

                // Setup message author in html
                var authorNode = messageHtml.GetElementbyId("message-author");
                if (authorNode != null)
                {
                    var author = " ";
                    if (message.From?.Application != null)
                    {
                        author = message.From.Application.DisplayName;
                    }

                    if (message.From?.Conversation != null)
                    {
                        author = message.From.Conversation.DisplayName;
                    }

                    if (message.From?.User != null)
                    {
                        author = message.From.User.DisplayName;
                    }

                    if (message.From?.Device != null)
                    {
                        author = message.From.Device.DisplayName;
                    }

                    authorNode.AppendChild(HtmlNode.CreateNode(author));
                    authorNode.Attributes["id"].Remove();
                }

                // Setup message creation date in html
                var createdDateNode = messageHtml.GetElementbyId("message-created-time");
                if (createdDateNode != null)
                {
                    createdDateNode.AppendChild(HtmlNode.CreateNode(message.CreatedDateTime.ToInternational()));
                    createdDateNode.Attributes["id"].Remove();
                }

                var modifiedDateNode = messageHtml.GetElementbyId("message-modified-time");
                if (modifiedDateNode != null)
                {
                    if (message.LastModifiedDateTime.HasValue)
                    {
                        modifiedDateNode.AppendChild(HtmlNode.CreateNode(message.LastModifiedDateTime.ToInternational()));
                        modifiedDateNode.Attributes["id"].Remove();
                    }
                    else
                    {
                        modifiedDateNode.Remove();
                    }
                }

                var deletedDateNode = messageHtml.GetElementbyId("message-deleted-time");
                if (deletedDateNode != null)
                {
                    if (message.LastModifiedDateTime.HasValue)
                    {
                        deletedDateNode.AppendChild(HtmlNode.CreateNode(message.LastModifiedDateTime.ToInternational()));
                        deletedDateNode.Attributes["id"].Remove();
                    }
                    else
                    {
                        deletedDateNode.Remove();
                    }
                }

                var messageTextNode = messageHtml.GetElementbyId("message-text");
                if (messageTextNode != null)
                {
                    if (!string.IsNullOrEmpty(message.Body.Content))
                    {
                        messageTextNode.AppendChild(HtmlNode.CreateNode(message.Body.Content));
                        messageTextNode.Attributes["id"].Remove();
                    }
                    else
                    {
                        messageTextNode.Remove();
                    }
                }

                var reactionsNode = messageHtml.GetElementbyId("message-reactions");
                if (reactionsNode != null)
                {
                    if (message.Reactions != null && message.Reactions.Any())
                    {
                        var groups = message.Reactions.GroupBy(c => c.ReactionType).ToList();
                        reactionsNode.AppendChild(HtmlNode.CreateNode($"Reactions: {string.Join(", ", groups.Select(c => $"{c.Key}({c.Count()})").ToList())}"));
                        reactionsNode.Attributes["id"].Remove();
                    }
                    else
                    {
                        reactionsNode.Remove();
                    }
                }

                var attacmentsNode = messageHtml.GetElementbyId("message-attachments");
                if (attacmentsNode != null)
                {
                    if (message.Attachments != null && message.Attachments.Any())
                    {
                        message.Attachments.ToList().ForEach(c =>
                        {
                            var attachment = HtmlNode.CreateNode($"<span class=\"attachment\"></span>");

                            var ctp = !string.IsNullOrEmpty(c.ContentType) ? $"ContentType: {c.ContentType}" : string.Empty;
                            var content = !string.IsNullOrEmpty(c.Content) ?
                                Regex.Replace($"Content: {c.Content?.Replace(Environment.NewLine, " ")} ", @"\s+", " ", RegexOptions.Multiline) :
                                string.Empty;
                            var url = !string.IsNullOrEmpty(c.ContentUrl) ? $"ContentUrl: {c.ContentUrl} " : string.Empty;

                            var name = !string.IsNullOrEmpty(c.Name) ? $"Name: {c.Name} " : string.Empty;
                            var thumbnailUrl = !string.IsNullOrEmpty(c.ThumbnailUrl) ? $"ThumbnailUrl: {c.ThumbnailUrl} " : string.Empty;

                            attachment.AppendChild(HtmlNode.CreateNode($"{url}{content}{ctp}{name}{thumbnailUrl}"));
                            attacmentsNode.AppendChild(attachment);
                        });

                        attacmentsNode.Attributes["id"].Remove();
                    }
                    else
                    {
                        attacmentsNode.Remove();
                    }
                }
            }

            return messageHtml.DocumentNode;
        }
    }
}
