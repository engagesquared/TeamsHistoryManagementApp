// <copyright file="BackgroundQueuedService.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.WebApp.Background
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Queue;

    internal class BackgroundQueuedService : BackgroundService, IHostedService
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundQueuedService"/> class.
        /// </summary>
        /// <param name="taskQueue"></param>
        /// <param name="loggerFactory"></param>
        public BackgroundQueuedService(
            IBackgroundQueue taskQueue,
            ILoggerFactory loggerFactory)
        {
            TaskQueue = taskQueue;
            logger = loggerFactory.CreateLogger<BackgroundQueuedService>();
        }

        public IBackgroundQueue TaskQueue { get; }

        /// <inheritdoc/>
        protected override Task ExecuteAsync(
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Queued Hosted Service is starting.");

            List<Action> actions = new List<Action>();
            for (var i = 0; i < BackgroundTaskQueue.SemaphoreLimit; i++)
            {
                actions.Add(async () => await RunTask(cancellationToken));
            }

            Parallel.Invoke(new ParallelOptions { MaxDegreeOfParallelism = BackgroundTaskQueue.SemaphoreLimit }, actions.ToArray());

            logger.LogInformation("Queued Hosted Service is stopping.");

            return Task.CompletedTask;
        }

        private async Task RunTask(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var workItem = await TaskQueue.DequeueAsync(cancellationToken);
                if (workItem != null)
                {
                    try
                    {
                        logger.LogInformation($"background task {nameof(workItem)} started");
                        await workItem(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Error occurred executing {nameof(workItem)}.");
                    }
                }
            }
        }
    }
}
