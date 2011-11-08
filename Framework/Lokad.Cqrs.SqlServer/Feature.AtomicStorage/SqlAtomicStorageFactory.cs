using System.Collections.Generic;
using System.Linq;
using Lokad.Cqrs.Feature.AtomicStorage;

namespace Lokad.Cqrs.SqlServer.Feature.AtomicStorage
{
    public sealed class SqlAtomicStorageFactory : IAtomicStorageFactory
    {
        readonly IAtomicStorageStrategy _strategy;
		readonly ISqlStorageConfig _config;

		public SqlAtomicStorageFactory(ISqlStorageConfig config, IAtomicStorageStrategy strategy)
        {
			_config = config;
            _strategy = strategy;
        }

        public IAtomicEntityWriter<TKey, TEntity> GetEntityWriter<TKey, TEntity>()
        {
			return new SqlAtomicEntityContainer<TKey, TEntity>(_config, _strategy);
        }

        public IAtomicEntityReader<TKey, TEntity> GetEntityReader<TKey, TEntity>()
        {
			return new SqlAtomicEntityContainer<TKey, TEntity>(_config, _strategy);
        }

        public IAtomicSingletonReader<TSingleton> GetSingletonReader<TSingleton>()
        {
			return new SqlAtomicSingletonContainer<TSingleton>(_config, _strategy);
        }

        public IAtomicSingletonWriter<TSingleton> GetSingletonWriter<TSingleton>()
        {
			return new SqlAtomicSingletonContainer<TSingleton>(_config, _strategy);
        }

		public IEnumerable<string> Initialize()
		{
			return Enumerable.Empty<string>();
		}
    }
}