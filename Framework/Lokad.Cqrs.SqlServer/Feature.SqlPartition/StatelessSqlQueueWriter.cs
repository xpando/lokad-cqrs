using System;
using System.Data.Linq;
using System.Linq;
using Lokad.Cqrs.Core.Outbox;

namespace Lokad.Cqrs.Feature.SqlPartition
{
    public sealed class StatelessSqlQueueWriter : IQueueWriter
    {
        public string Name { get; private set; }

        int _queueID;

        public void PutMessage(ImmutableEnvelope envelope)
        {
            var buffer = _streamer.SaveEnvelopeData(envelope);
            var now = DateTime.UtcNow;

            using (var db = new DataContext(_connectionString))
            {
                db.ExecuteCommand
                (
                    "exec [dbo].[Enqueue] {0}, {1}, {2}, {3}, {4}", 
                    _queueID, 
                    envelope.EnvelopeId, 
                    envelope.CreatedOnUtc, 
                    envelope.DeliverOnUtc < now ? now : envelope.DeliverOnUtc,
                    buffer
                );
            }
        }

        public StatelessSqlQueueWriter
        (
            IEnvelopeStreamer streamer, 
            string connectionString, 
            string name
        )
        {
            _streamer         = streamer;
            _connectionString = connectionString;
            Name              = name;

            using (var db = new DataContext(_connectionString))
            {
                _queueID = db.ExecuteQuery<int>("exec [dbo].[GetQueueID] {0}", name).Single();
            }
        }

        readonly string _connectionString;
        readonly IEnvelopeStreamer _streamer;
    }
}