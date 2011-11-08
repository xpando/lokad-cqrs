using System;
using System.Data.Linq;
using System.Linq;
using Lokad.Cqrs.Core.Inbox;
using Lokad.Cqrs.Core.Inbox.Events;

namespace Lokad.Cqrs.Feature.SqlPartition
{
    public sealed class StatelessSqlQueueReader
    {
        readonly ISqlStorageConfig _config;
        readonly string _queueName;
        readonly IEnvelopeStreamer _streamer;
        readonly TimeSpan _visibilityTimeout;
        readonly ISystemObserver _observer;
        
        int _queueID;
        int _poisonQueueID;

        public string Name { get { return _queueName; } }

        public StatelessSqlQueueReader
        (
            ISqlStorageConfig config,
            string name,
            ISystemObserver provider,
            IEnvelopeStreamer streamer, 
            TimeSpan visibilityTimeout
        )
        {
            _config            = config;
            _observer          = provider;
            _queueName         = name;
            _streamer          = streamer;
            _visibilityTimeout = visibilityTimeout;
        }

        public void Initialize()
        {
            using (var db = new DataContext(_config.ConnectionString))
            {
                _queueID       = db.ExecuteQuery<int>("exec [dbo].[GetQueueID] {0}", _queueName).Single();
                _poisonQueueID = db.ExecuteQuery<int>("exec [dbo].[GetQueueID] {0}", _queueName + "_Poison").Single();
            }
        }

        public GetEnvelopeResult TryGetMessage()
        {
            QueueMessage message;

            try
            {
                using (var db = new DataContext(_config.ConnectionString))
                {
                    message = db
                        .ExecuteQuery<QueueMessage>
                        (
                            "exec [dbo].[Dequeue] {0}, {1}, {2}",
                            _queueID,
                            _visibilityTimeout.TotalSeconds,
                            1
                        )
                        .FirstOrDefault();
                }
            }
            catch(Exception ex)
            {
                _observer.Notify(new FailedToReadMessage(ex, _queueName));
                return GetEnvelopeResult.Error();
            }

            if (message == null)
                return GetEnvelopeResult.Empty;

            try
            {
                var envelope = _streamer.ReadAsEnvelopeData(message.Envelope.ToArray());
                var context = new EnvelopeTransportContext(message, envelope, _queueName);
                return GetEnvelopeResult.Success(context);
            }
            catch(Exception ex)
            {
                _observer.Notify(new EnvelopeDeserializationFailed(ex, _queueName, message.EnvelopeID));
                SendToPoisonQueue(message);
                DeleteMessage(message);
                return GetEnvelopeResult.Retry;
            }
        }

        void SendToPoisonQueue(QueueMessage message)
        {
            using (var db = new DataContext(_config.ConnectionString))
            {
                db.ExecuteCommand
                (
                    "exec [dbo].[Enqueue] {0}, {1}, {2}, {3}, {4}", 
                    _poisonQueueID, 
                    message.EnvelopeID, 
                    message.CreatedOnUtc, 
                    DateTime.UtcNow,
                    message.Envelope
                );
            }
        }

        void DeleteMessage(QueueMessage message)
        {
            using (var db = new DataContext(_config.ConnectionString))
            {
                db.ExecuteCommand
                (
                    "exec [dbo].[Delete] {0}", 
                    message.MessageID
                );
            }
        }

        /// <summary>
        /// ACKs the message by deleting it from the queue.
        /// </summary>
        /// <param name="envelope">The message context to ACK.</param>
        public void AckMessage(EnvelopeTransportContext envelope)
        {
            if (envelope == null) 
                throw new ArgumentNullException("message");

            DeleteMessage((QueueMessage)envelope.TransportMessage);
        }

        internal class QueueMessage
        {
            public long MessageID { get; set; }
            public string EnvelopeID { get; set; }
            public Binary Envelope { get; set; }
            public DateTime CreatedOnUtc { get; set; }
            public int DequeueCount { get; set; }
        }
    }
}