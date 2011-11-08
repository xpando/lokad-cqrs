﻿using System;
using System.Threading;
using Lokad.Cqrs.Core.Inbox;

namespace Lokad.Cqrs.Feature.SqlPartition.Inbox
{
    /// <summary>
    /// Handles deserialization, joins multiple queues and notifications
    /// </summary>
    public sealed class SqlPartitionInbox : IPartitionInbox
    {
        readonly StatelessSqlQueueReader[] _readers;
        readonly Func<uint, TimeSpan> _waiter;
        uint _emptyCycles;

        public SqlPartitionInbox
        (
            StatelessSqlQueueReader[] readers,
            Func<uint, TimeSpan> waiter
        )
        {
            _readers = readers;
            _waiter = waiter;
        }

        public void Init()
        {
            foreach (var queue in _readers)
            {
                queue.Initialize();
            }
        }

        public void AckMessage(EnvelopeTransportContext envelope)
        {
            foreach (var queue in _readers)
            {
                if (queue.Name == envelope.QueueName)
                {
                    queue.AckMessage(envelope);
                }
            }
        }

        public void TryNotifyNack(EnvelopeTransportContext context)
        {
        }

        public bool TakeMessage(CancellationToken token, out EnvelopeTransportContext context)
        {
            while (!token.IsCancellationRequested)
            {
                for (var i = 0; i < _readers.Length; i++)
                {
                    var queue = _readers[i];

                    var message = queue.TryGetMessage();
                    switch (message.State)
                    {
                        case GetEnvelopeResultState.Success:

                            _emptyCycles = 0;
                            // future message
                            if (message.Envelope.Unpacked.DeliverOnUtc > DateTime.UtcNow)
                            {
                                throw new InvalidOperationException("Future message delivery has been disabled in the code");
                            }
                            context = message.Envelope;
                            return true;
                        case GetEnvelopeResultState.Empty:
                            _emptyCycles += 1;
                            break;
                        case GetEnvelopeResultState.Exception:
                            // access problem, fall back a bit
                            break;
                        case GetEnvelopeResultState.Retry:
                            // this could be the poison
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    var waiting = _waiter(_emptyCycles);
                    token.WaitHandle.WaitOne(waiting);
                }
            }
            context = null;
            return false;
        }
    }
}