﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Knapcode.ExplorePackages.WideEntities
{
    public class WideEntity
    {
        private readonly WideEntity _withoutData;
        private readonly IReadOnlyList<ReadOnlyMemory<byte>> _chunks;

        internal WideEntity(WideEntitySegment firstSegment)
        {
            PartitionKey = firstSegment.PartitionKey;
            RowKey = firstSegment.RowKeyPrefix;
            Timestamp = firstSegment.Timestamp;
            ETag = firstSegment.ETag;
            SegmentCount = firstSegment.SegmentCount;

            if (firstSegment.Index != 0)
            {
                throw new ArgumentException("The first segment should have an index of 0.", nameof(firstSegment));
            }

            _withoutData = this;
        }

        internal WideEntity(ICollection<WideEntitySegment> segments)
        {
            PartitionKey = segments.Select(x => x.PartitionKey).Distinct().Single();
            RowKey = segments.Select(x => x.RowKeyPrefix).Distinct().Single();

            var orderedSegments = segments.OrderBy(x => x.Index).ToList();
            var firstSegment = orderedSegments.First();
            Timestamp = firstSegment.Timestamp;
            ETag = firstSegment.ETag;

            if (firstSegment.Index != 0)
            {
                throw new ArgumentException("The first segment should have an index of 0.", nameof(segments));
            }

            if (segments.Count != firstSegment.SegmentCount)
            {
                throw new ArgumentException("The number of segments provided must match the segment count property on the first segment.");
            }

            SegmentCount = segments.Count;
            _chunks = orderedSegments.SelectMany(x => x.Chunks).ToList();
            _withoutData = new WideEntity(firstSegment);
        }

        public string PartitionKey { get; }
        public string RowKey { get; }
        public DateTimeOffset Timestamp { get; }
        public string ETag { get; }
        public int SegmentCount { get; }

        public WideEntity CloneWithoutData()
        {
            return _withoutData;
        }

        public Stream GetStream()
        {
            if (_chunks == null)
            {
                throw new InvalidOperationException("The data was not included when retrieving this entity.");
            }

            return new ChunkStream(_chunks);
        }

        public byte[] ToByteArray()
        {
            using var source = GetStream();
            var buffer = new byte[(int)source.Length];
            var offset = 0;
            do
            {
                offset += source.Read(buffer, offset, buffer.Length - offset);
            }
            while (offset < buffer.Length);
            return buffer;
        }
    }
}
