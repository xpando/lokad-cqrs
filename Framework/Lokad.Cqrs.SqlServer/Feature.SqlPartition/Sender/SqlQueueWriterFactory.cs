using System.Collections.Concurrent;
using Lokad.Cqrs.Core.Outbox;

namespace Lokad.Cqrs.Feature.SqlPartition.Sender
{
    public sealed class SqlQueueWriterFactory : IQueueWriterFactory
    {
        readonly IEnvelopeStreamer _streamer;
        readonly ConcurrentDictionary<string, IQueueWriter> _writeQueues = new ConcurrentDictionary<string, IQueueWriter>();
        readonly ISqlStorageConfig _config;

        public string Endpoint
        {
            get { return _config.Name; }
        }

        public SqlQueueWriterFactory(ISqlStorageConfig config, IEnvelopeStreamer streamer)
        {
            _config = config;
            _streamer = streamer;
        }

        public IQueueWriter GetWriteQueue(string queueName)
        {
            return _writeQueues.GetOrAdd
            (
                queueName, 
                name => new StatelessSqlQueueWriter
                (
                    _streamer, 
                    _config.ConnectionString, 
                    queueName
                )
            );
        }
    }
}