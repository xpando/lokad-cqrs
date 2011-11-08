using System;
using System.ComponentModel;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.SqlPartition.Sender;

namespace Lokad.Cqrs.Build
{
    public sealed class SqlClientModule : HideObjectMembersFromIntelliSense, IModule
    {
        Action<IComponentRegistry> _modules = context => { };

        public void AddSqlServerSender(ISqlStorageConfig config, string queueName, Action<SendMessageModule> configure)
        {
            var module = new SendMessageModule((context, endpoint) => new SqlQueueWriterFactory(config, context.Resolve<IEnvelopeStreamer>()), config.ConnectionString, queueName);
            configure(module);
            _modules += module.Configure;
        }

        public void AddSqlServerSender(ISqlStorageConfig config, string queueName)
        {
            AddSqlServerSender(config, queueName, m => { });
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Configure(IComponentRegistry container)
        {
            _modules(container);
        }
    }
}