// <copyright file="ThreadDetails.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.ReportsGenerators.Models
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph;

    /// <summary>
    /// Describes thread details.
    /// </summary>
    internal class ThreadDetails
    {
        /// <summary>
        /// Gets or sets a value indicating whether this thread  is fully extracted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is fully extracted; otherwise, <c>false</c>.
        /// </value>
        public bool IsFull { get; set; }

        /// <summary>
        /// Gets or sets the thread messages.
        /// </summary>
        /// <value>
        /// The messages.
        /// </value>
        public List<IMessageDetails> Messages { get; set; }

        /// <summary>
        /// Gets or sets the last message creation time.
        /// </summary>
        /// <value>
        /// The last message creation time.
        /// </value>
        public DateTimeOffset LastMessageCreationTime { get; set; }
    }
}
