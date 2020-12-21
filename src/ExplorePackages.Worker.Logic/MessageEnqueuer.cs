﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Knapcode.ExplorePackages.Worker
{
    public class MessageEnqueuer
    {
        private readonly SchemaSerializer _serializer;
        private readonly MessageBatcher _batcher;
        private readonly IRawMessageEnqueuer _rawMessageEnqueuer;
        private readonly ILogger<MessageEnqueuer> _logger;

        public MessageEnqueuer(
            SchemaSerializer serializer,
            MessageBatcher batcher,
            IRawMessageEnqueuer rawMessageEnqueuer,
            ILogger<MessageEnqueuer> logger)
        {
            _serializer = serializer;
            _batcher = batcher;
            _rawMessageEnqueuer = rawMessageEnqueuer;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            await _rawMessageEnqueuer.InitializeAsync();
        }

        public Task EnqueueAsync<T>(IReadOnlyList<T> messages) => EnqueueAsync(messages, TimeSpan.Zero);
        public Task EnqueueAsync<T>(IReadOnlyList<T> messages, TimeSpan notBefore) => EnqueueAsync(messages, _serializer.GetSerializer<T>(), notBefore);

        internal Task EnqueueAsync<T>(IReadOnlyList<T> messages, ISchemaSerializer<T> serializer) => EnqueueAsync(messages, serializer, TimeSpan.Zero);

        internal async Task EnqueueAsync<T>(IReadOnlyList<T> messages, ISchemaSerializer<T> serializer, TimeSpan notBefore)
        {
            if (messages.Count == 0)
            {
                return;
            }

            var batches = _batcher.BatchOrNull(messages, serializer);
            if (batches != null)
            {
                await EnqueueAsync(batches, notBefore);
                return;
            }

            var bulkEnqueueStrategy = _rawMessageEnqueuer.BulkEnqueueStrategy;
            if (!bulkEnqueueStrategy.IsEnabled || messages.Count < bulkEnqueueStrategy.Threshold)
            {
                var serializedMessages = messages.Select(m => serializer.SerializeMessage(m).AsString()).ToList();
                await _rawMessageEnqueuer.AddAsync(serializedMessages, notBefore);
            }
            else
            {
                var batch = new List<JToken>();
                var batchMessage = new HomogeneousBulkEnqueueMessage
                {
                    SchemaName = serializer.Name,
                    SchemaVersion = serializer.LatestVersion,
                    Messages = batch,
                    NotBefore = notBefore <= TimeSpan.Zero ? (TimeSpan?)null : notBefore,
                };
                var emptyBatchMessageLength = GetMessageLength(batchMessage);
                var batchMessageLength = emptyBatchMessageLength;

                for (int i = 0; i < messages.Count; i++)
                {
                    var innerData = serializer.SerializeData(messages[i]);
                    var innerDataLength = GetMessageLength(innerData);

                    if (!batch.Any())
                    {
                        batch.Add(innerData.AsJToken());
                        batchMessageLength += innerDataLength;
                    }
                    else
                    {
                        var newBatchMessageLength = batchMessageLength + ",".Length + innerDataLength;
                        if (newBatchMessageLength > bulkEnqueueStrategy.MaxSize)
                        {
                            await EnqueueBulkEnqueueMessageAsync(batchMessage, batchMessageLength);
                            batch.Clear();
                            batch.Add(innerData.AsJToken());
                            batchMessageLength = emptyBatchMessageLength + innerDataLength;
                        }
                        else
                        {
                            batch.Add(innerData.AsJToken());
                            batchMessageLength = newBatchMessageLength;
                        }
                    }
                }

                if (batch.Count > 0)
                {
                    await EnqueueBulkEnqueueMessageAsync(batchMessage, batchMessageLength);
                }
            }
        }

        private async Task EnqueueBulkEnqueueMessageAsync(HomogeneousBulkEnqueueMessage batchMessage, int expectedLength)
        {
            var bytes = _serializer.Serialize(batchMessage).AsString();
            if (bytes.Length != expectedLength)
            {
                throw new InvalidOperationException(
                    $"The bulk enqueue message had an unexpected size. " +
                    $"Expected: {expectedLength}. " +
                    $"Actual: {bytes.Length}");
            }

            _logger.LogInformation("Enqueueing a bulk enqueue message containing {Count} individual messages.", batchMessage.Messages.Count);
            await _rawMessageEnqueuer.AddAsync(new[] { bytes });
        }

        private int GetMessageLength(HomogeneousBulkEnqueueMessage batchMessage)
        {
            return GetMessageLength(_serializer.Serialize(batchMessage));
        }

        private static int GetMessageLength(ISerializedEntity innerMessage)
        {
            return Encoding.UTF8.GetByteCount(innerMessage.AsString());
        }
    }
}
