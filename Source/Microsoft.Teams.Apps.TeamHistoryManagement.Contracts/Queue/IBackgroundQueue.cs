// <copyright file="IBackgroundQueue.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Queue
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IBackgroundQueue
    {
        void QueueBackgroundTask(Func<CancellationToken, Task> taskExecutor);

        /// <summary>
        ///
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }
}
