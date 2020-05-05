// <copyright file="IMessageIdentitySet.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph
{
    /// <summary>
    /// Describeds the message identity set.
    /// </summary>
    public interface IMessageIdentitySet
    {
        /// <summary>
        /// Gets or sets the application identity.
        /// </summary>
        /// <value>
        /// The application.
        /// </value>
        IIdentity Application { get; set; }

        /// <summary>
        /// Gets or sets the conversation identity.
        /// </summary>
        /// <value>
        /// The conversation.
        /// </value>
        IIdentity Conversation { get; set; }

        /// <summary>
        /// Gets or sets the device identity.
        /// </summary>
        /// <value>
        /// The device.
        /// </value>
        IIdentity Device { get; set; }

        /// <summary>
        /// Gets or sets the user identity.
        /// </summary>
        /// <value>
        /// The user.
        /// </value>
        IIdentity User { get; set; }
    }
}
