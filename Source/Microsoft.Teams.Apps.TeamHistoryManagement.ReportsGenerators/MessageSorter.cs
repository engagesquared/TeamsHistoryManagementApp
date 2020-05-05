// <copyright file="MessageSorter.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.ReportsGenerators
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph;
    using Microsoft.Teams.Apps.TeamHistoryManagement.ReportsGenerators.Models;

    /// <summary>
    /// Implements the message sorting helper for grouping messages by threads and ordering by datetime.
    /// </summary>
    internal static class MessageSorter
    {
        /// <summary>
        /// Processes the messages. Grouping by threads/Ordering by datetime.
        /// </summary>
        /// <param name="details">The details.</param>
        /// <returns></returns>
        public static List<ThreadDetails> ProcessMessages(IEnumerable<IMessageDetails> details)
        {
            var threads = new List<ThreadDetails>();

            // Threads Initial/First Messages
            var roots = details.Where(c => c.ReplyToId == null).ToList();
            roots.ForEach(c => threads.Add(new ThreadDetails()
            {
                Messages = new List<IMessageDetails>() { c },
                IsFull = string.IsNullOrEmpty(c.ReplyToId),
                LastMessageCreationTime = c.CreatedDateTime,
            }));

            // Related messages which has root message
            var children = details.Where(c => details.Where(x => x.Id == c.ReplyToId).Any()).ToList();

            if (children.Any())
            {
                foreach (var thread in threads)
                {
                    // Get all relevant child messages for current thread
                    var results = children.Where(c => c.ReplyToId == thread.Messages.First().Id).ToList();
                    foreach (var m in results)
                    {
                        thread.Messages.Add(m);
                        children.Remove(m);
                    }

                    // Order by created date
                    thread.Messages = thread.Messages.OrderBy(c => c.CreatedDateTime).ToList();
                    thread.LastMessageCreationTime = thread.Messages.Max(c => c.CreatedDateTime);
                }
            }

            // Child messages which doesn't have root message in current collection
            var childrenWithoutRoot = details.Where(c => c.ReplyToId != null && !details.Where(x => x.Id == c.ReplyToId).Any()).ToList();
            if (childrenWithoutRoot.Any())
            {
                // group messages by ID of root message
                var groups = childrenWithoutRoot.GroupBy(c => c.ReplyToId);

                foreach (var group in groups)
                {
                    var thread = new ThreadDetails()
                    {
                        Messages = group.OrderBy(c => c.CreatedDateTime).ToList(),
                        IsFull = false,
                        LastMessageCreationTime = group.Max(c => c.CreatedDateTime),
                    };
                    threads.Add(thread);
                }
            }

            threads = threads.OrderBy(c => c.LastMessageCreationTime).ToList();

            return threads;
        }
    }
}
