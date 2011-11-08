using System;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Feature.AtomicStorage;
using Lokad.Cqrs.SqlServer.Feature.AtomicStorage;

namespace Lokad.Cqrs
{
    public static class ExtendStorageModule
    {
        public static void AtomicIsInSql(this StorageModule self, ISqlStorageConfig storage, Action<DefaultAtomicStorageStrategyBuilder> config)
        {
            var builder = new DefaultAtomicStorageStrategyBuilder();
            config(builder);
            AtomicIsInSql(self, storage, builder.Build());
        }

        public static void AtomicIsInSql(this StorageModule self, ISqlStorageConfig storage)
        {
            AtomicIsInSql(self, storage, builder => { });
        }

        public static void AtomicIsInSql(this StorageModule self, ISqlStorageConfig config, IAtomicStorageStrategy strategy)
        {
			self.AtomicIs(new SqlAtomicStorageFactory(config, strategy));
        }

        public static void StreamingIsInAzure(this StorageModule self, ISqlStorageConfig storage)
        {
            //self.StreamingIs(new SqlStreamingRoot(storage.CreateBlobClient()));
        }
    }
}