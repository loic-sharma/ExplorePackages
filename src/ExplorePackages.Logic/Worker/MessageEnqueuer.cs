﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Knapcode.ExplorePackages.Logic.Worker
{
    public class MessageEnqueuer
    {
        private readonly MessageSerializer _messageSerializer;
        private readonly IRawMessageEnqueuer _rawMessageEnqueuer;
        private readonly ILogger<MessageEnqueuer> _logger;

        public MessageEnqueuer(
            MessageSerializer messageSerializer,
            IRawMessageEnqueuer rawMessageEnqueuer,
            ILogger<MessageEnqueuer> logger)
        {
            _messageSerializer = messageSerializer;
            _rawMessageEnqueuer = rawMessageEnqueuer;
            _logger = logger;
        }

        public async Task EnqueueAsync(IReadOnlyList<CatalogIndexScanMessage> messages)
        {
            await EnqueueAsync(messages, m => _messageSerializer.Serialize(m));
        }

        public async Task EnqueueAsync(IReadOnlyList<CatalogPageScanMessage> messages)
        {
            await EnqueueAsync(messages, m => _messageSerializer.Serialize(m));
        }

        public async Task EnqueueAsync(IReadOnlyList<CatalogLeafMessage> messages)
        {
            await EnqueueAsync(messages, m => _messageSerializer.Serialize(m));
        }

        public async Task EnqueueAsync<T>(IReadOnlyList<T> messages, Func<T, ISerializedMessage> serialize)
        {
            if (messages.Count == 0)
            {
                return;
            }

            var bulkEnqueueStrategy = _rawMessageEnqueuer.BulkEnqueueStrategy;
            if (!bulkEnqueueStrategy.IsEnabled || messages.Count < bulkEnqueueStrategy.Threshold)
            {
                var serializedMessages = messages.Select(m => serialize(m).AsString()).ToList();
                await _rawMessageEnqueuer.AddAsync(serializedMessages);
            }
            else
            {
                var batch = new List<JToken>();
                var batchMessage = new BulkEnqueueMessage { Messages = batch };
                var emptyBatchMessageLength = GetMessageLength(batchMessage);
                var batchMessageLength = emptyBatchMessageLength;

                for (int i = 0; i < messages.Count; i++)
                {
                    var innerMessage = serialize(messages[i]);
                    var innerMessageLength = GetMessageLength(innerMessage);

                    if (!batch.Any())
                    {
                        batch.Add(innerMessage.AsJToken());
                        batchMessageLength += innerMessageLength;
                    }
                    else
                    {
                        var newBatchMessageLength = batchMessageLength + ",".Length + innerMessageLength;
                        if (newBatchMessageLength > bulkEnqueueStrategy.MaxSize)
                        {
                            await EnqueueBulkEnqueueMessageAsync(batchMessage, batchMessageLength);
                            batch.Clear();
                            batch.Add(innerMessage.AsJToken());
                            batchMessageLength = emptyBatchMessageLength + innerMessageLength;
                        }
                        else
                        {
                            batch.Add(innerMessage.AsJToken());
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

        private async Task EnqueueBulkEnqueueMessageAsync(BulkEnqueueMessage batchMessage, int expectedLength)
        {
            var bytes = _messageSerializer.Serialize(batchMessage).AsString();
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

        private int GetMessageLength(BulkEnqueueMessage batchMessage)
        {
            return GetMessageLength(_messageSerializer.Serialize(batchMessage));
        }

        private static int GetMessageLength(ISerializedMessage innerMessage)
        {
            return Encoding.UTF8.GetByteCount(innerMessage.AsString());
        }
    }
}