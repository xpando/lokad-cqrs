using System;
using System.Text.RegularExpressions;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.SqlPartition;
using Lokad.Cqrs.Feature.SqlPartition.Sender;

namespace Lokad.Cqrs.Build.Engine
{
    public class SqlEngineModule : HideObjectMembersFromIntelliSense, IModule
    {
        static readonly Regex QueueName = new Regex("^[A-Za-z][A-Za-z0-9]{2,62}", RegexOptions.Compiled);
        Action<IComponentRegistry> _funqlets = registry => { };

        public void AddSqlSender(ISqlStorageConfig config, string queueName, Action<SendMessageModule> configure)
        {
            var module = new SendMessageModule((context, endpoint) => new SqlQueueWriterFactory(config, context.Resolve<IEnvelopeStreamer>()), config.Name, queueName);
            configure(module);
            _funqlets += module.Configure;
        }

        public void AddSqlSender(ISqlStorageConfig config, string queueName)
        {
            AddSqlSender(config, queueName, m => {});
        }

        public void AddSqlProcess(ISqlStorageConfig config, string[] queues, Action<SqlPartitionModule> configure)
        {
            foreach (var queue in queues)
            {
                if (queue.Contains(":"))
                {
                    var message = string.Format("Queue '{0}' should not contain queue prefix, since it's sql already", queue);
                    throw new InvalidOperationException(message);
                }

                if (!QueueName.IsMatch(queue))
                {
                    var format = string.Format("Queue name should match regex '{0}'", QueueName);
                    throw new InvalidOperationException(format);
                }
            }

            var module = new SqlPartitionModule(config, queues);
            configure(module);
            _funqlets += module.Configure;
        }

        public void AddSqlProcess(ISqlStorageConfig config, params string[] queues)
        {
            AddSqlProcess(config, queues, m => {});
        }

        public void AddSqlRouter(ISqlStorageConfig config, string queueName, Func<ImmutableEnvelope, string> configure)
        {
            AddSqlProcess(config, new[] { queueName }, m => m.DispatchToRoute(configure));
        }

        public void Configure(IComponentRegistry container)
        {
            _funqlets(container);
        }
    }
}