﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Knapcode.ExplorePackages.Worker
{
    public class QueueStorageEnqueuer : IRawMessageEnqueuer
    {
        private readonly IWorkerQueueFactory _workerQueueFactory;
        private readonly IOptions<ExplorePackagesWorkerSettings> _options;
        private readonly ILogger<QueueStorageEnqueuer> _logger;

        public QueueStorageEnqueuer(
            IWorkerQueueFactory workerQueueFactory,
            IOptions<ExplorePackagesWorkerSettings> options,
            ILogger<QueueStorageEnqueuer> logger)
        {
            _workerQueueFactory = workerQueueFactory;
            _options = options;
            _logger = logger;
            BulkEnqueueStrategy = _options.Value.UseBulkEnqueueStrategy
                ? BulkEnqueueStrategy.Enabled(_options.Value.BulkEnqueueThreshold)
                : BulkEnqueueStrategy.Disabled();
        }

        public int MaxMessageSize => 65536;
        public BulkEnqueueStrategy BulkEnqueueStrategy { get; }

        public async Task InitializeAsync()
        {
            await _workerQueueFactory.InitializeAsync();
        }

        public Task<int> GetApproximateMessageCountAsync()
        {
            return GetApproximateMessageCountAsync(_workerQueueFactory.GetQueue());
        }

        public Task<int> GetAvailableMessageCountLowerBoundAsync(int messageCount)
        {
            return GetAvailableMessageCountLowerBoundAsync(_workerQueueFactory.GetQueue(), messageCount);
        }

        public Task<int> GetPoisonApproximateMessageCountAsync()
        {
            return GetApproximateMessageCountAsync(_workerQueueFactory.GetPoisonQueue());
        }

        public Task<int> GetPoisonAvailableMessageCountLowerBoundAsync(int messageCount)
        {
            return GetAvailableMessageCountLowerBoundAsync(_workerQueueFactory.GetPoisonQueue(), messageCount);
        }

        private static async Task<int> GetApproximateMessageCountAsync(CloudQueue queue)
        {
            await queue.FetchAttributesAsync();
            return queue.ApproximateMessageCount.Value;
        }

        private static async Task<int> GetAvailableMessageCountLowerBoundAsync(CloudQueue queue, int messageCount)
        {
            var messages = await queue.PeekMessagesAsync(messageCount);
            return messages.Count();
        }

        public async Task AddAsync(IReadOnlyList<string> messages)
        {
            await AddAsync(messages, TimeSpan.Zero);
        }

        public async Task AddAsync(IReadOnlyList<string> messages, TimeSpan visibilityDelay)
        {
            await AddAsync(_workerQueueFactory.GetQueue, messages, visibilityDelay);
        }

        public async Task AddPoisonAsync(IReadOnlyList<string> messages)
        {
            await AddPoisonAsync(messages, TimeSpan.Zero);
        }

        public async Task AddPoisonAsync(IReadOnlyList<string> messages, TimeSpan visibilityDelay)
        {
            await AddAsync(_workerQueueFactory.GetPoisonQueue, messages, visibilityDelay);
        }

        private async Task AddAsync(Func<CloudQueue> getQueue, IReadOnlyList<string> messages, TimeSpan visibilityDelay)
        {
            if (messages.Count == 0)
            {
                return;
            }

            var workers = Math.Min(messages.Count, _options.Value.EnqueueWorkers);
            if (workers < 2)
            {
                var queue = getQueue();
                _logger.LogInformation("Enqueueing {Count} individual messages to {QueueName}.", messages.Count, queue.Name);
                var completedCount = 0;
                foreach (var message in messages)
                {
                    await AddMessageAsync(queue, message, visibilityDelay);
                    completedCount++;
                    if (completedCount % 500 == 0 && completedCount < messages.Count)
                    {
                        _logger.LogInformation("Enqueued {CompletedCount} of {TotalCount} messages.", completedCount, messages.Count);
                    }
                }
                _logger.LogInformation("Done enqueueing {Count} individual messages to {QueueName}.", messages.Count, queue.Name);
            }
            else
            {
                var work = new ConcurrentQueue<string>(messages);

                var queueName = getQueue().Name;
                _logger.LogInformation(
                    "Enqueueing {MessageCount} individual messages to {QueueName} with {WorkerCount} workers.",
                    messages.Count,
                    queueName,
                    workers);

                var tasks = Enumerable
                    .Range(0, workers)
                    .Select(async i =>
                    {
                        var queue = getQueue();
                        while (work.TryDequeue(out var message))
                        {
                            await AddMessageAsync(queue, message, visibilityDelay);
                        }
                    })
                    .ToList();

                await Task.WhenAll(tasks);

                _logger.LogInformation(
                    "Done enqueueing {MessageCount} individual messages to {QueueName} with {WorkerCount} workers.",
                    messages.Count,
                    queueName,
                    workers);
            }
        }

        private async Task AddMessageAsync(CloudQueue queue, string message, TimeSpan initialVisibilityDelay)
        {
            await queue.AddMessageAsync(
                new CloudQueueMessage(message),
                timeToLive: null,
                initialVisibilityDelay: initialVisibilityDelay > TimeSpan.Zero ? initialVisibilityDelay : null,
                options: null,
                operationContext: null);
        }
    }
}
