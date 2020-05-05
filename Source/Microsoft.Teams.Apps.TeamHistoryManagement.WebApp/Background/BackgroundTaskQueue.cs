// <copyright file="BackgroundTaskQueue.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.WebApp.Background
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Queue;

    /// <summary>
    /// Represents a background task queue for backround service which base on <see cref="System.Collections.Concurrent.ConcurrentQueue{T}"/>.
    /// </summary>
    internal class BackgroundTaskQueue : IBackgroundQueue
    {
        public const int SemaphoreLimit = 50;

        private ConcurrentQueue<Func<CancellationToken, Task>> taskItems =
            new ConcurrentQueue<Func<CancellationToken, Task>>();

        private SemaphoreSlim signal = new SemaphoreSlim(SemaphoreLimit);

        /// <summary>
        /// Queues the background task.
        /// </summary>
        /// <param name="taskExecutor">The task executor.</param>
        /// <exception cref="ArgumentNullException">taskExecutor.</exception>
        public void QueueBackgroundTask(
            Func<CancellationToken, Task> taskExecutor)
        {
            if (taskExecutor == null)
            {
                throw new ArgumentNullException(nameof(taskExecutor));
            }

            taskItems.Enqueue(taskExecutor);
            signal.Release();
        }

        /// <summary>
        /// Dequeues the background task asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns> .</returns>
        public async Task<Func<CancellationToken, Task>> DequeueAsync(
            CancellationToken cancellationToken)
        {
            await signal.WaitAsync(cancellationToken);
            taskItems.TryDequeue(out var task);

            return task;
        }
    }
}
