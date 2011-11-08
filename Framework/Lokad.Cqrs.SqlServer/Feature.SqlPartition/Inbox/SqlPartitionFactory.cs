using System;
using System.Collections.Concurrent;
using System.Linq;
using Lokad.Cqrs.Core.Inbox;

namespace Lokad.Cqrs.Feature.SqlPartition.Inbox
{
    public sealed class SqlPartitionFactory 
    {
        readonly IEnvelopeStreamer _streamer;
        readonly ISystemObserver _observer;
        readonly ISqlStorageConfig _config;
        readonly TimeSpan _queueVisibilityTimeout;
        readonly Func<uint, TimeSpan> _decayPolicy;

        readonly ConcurrentDictionary<string, StatelessSqlQueueReader> _intakes = new ConcurrentDictionary<string, StatelessSqlQueueReader>();

        public SqlPartitionFactory
        (
            IEnvelopeStreamer streamer, 
            ISystemObserver observer,
            ISqlStorageConfig config, 
            TimeSpan queueVisibilityTimeout,
            Func<uint, TimeSpan> decayPolicy
        )
        {
            _streamer = streamer;
            _queueVisibilityTimeout = queueVisibilityTimeout;
            _decayPolicy = decayPolicy;
            _config = config;
            _observer = observer;
        }

        StatelessSqlQueueReader BuildIntake(string name)
        {
            //var queue = _config.CreateQueueClient().GetQueueReference(name);
            //var container = _config.CreateBlobClient().GetContainerReference(name);
            
            //var poisonQueue = new Lazy<CloudQueue>(() =>
            //    {
            //        var queueReference = _config.CreateQueueClient().GetQueueReference(name + "-poison");
            //        queueReference.CreateIfNotExist();
            //        return queueReference;
            //    }, LazyThreadSafetyMode.ExecutionAndPublication);

            var reader = new StatelessSqlQueueReader(_config, name, _observer, _streamer, _queueVisibilityTimeout);
            return reader;
        }

        public IPartitionInbox GetNotifier(string[] queueNames)
        {
            var readers = queueNames
                .Select(name => _intakes.GetOrAdd(name, BuildIntake))
                .ToArray();
            
            return new SqlPartitionInbox(readers, _decayPolicy);
        }
    }
}