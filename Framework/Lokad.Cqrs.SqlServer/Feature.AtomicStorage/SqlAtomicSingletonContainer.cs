using System;
using System.IO;
using Lokad.Cqrs.Feature.AtomicStorage;

namespace Lokad.Cqrs.SqlServer.Feature.AtomicStorage
{
    public sealed class SqlAtomicSingletonContainer<TEntity> : IAtomicSingletonReader<TEntity>, IAtomicSingletonWriter<TEntity>
    {
        readonly IAtomicStorageStrategy _strategy;
		readonly ISqlStorageConfig _config;
		
		public SqlAtomicSingletonContainer(ISqlStorageConfig config, IAtomicStorageStrategy strategy)
        {
            _strategy = strategy;
			_config = config;
        }

        public bool TryGet(out TEntity singleton)
        {
            singleton = default(TEntity);
            try
            {
                
				//using (var stream = File.OpenRead(_filePath))
				//{
				//    singleton = _strategy.Deserialize<TEntity>(stream);
				//    return true;
				//}
				return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
        }

        public TEntity AddOrUpdate(Func<TEntity> addFactory, Func<TEntity, TEntity> update, AddOrUpdateHint hint)
        {
            try
            {
				//// we are locking this file.
				//using (var file = File.Open(_filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
				//{
				//    TEntity result;
				//    if (file.Length == 0)
				//    {
				//        result = addFactory();
				//    }
				//    else
				//    {
				//        // just because some serializers have a nasty habbit of closing the stream
				//        using (var mem = new MemoryStream())
				//        {
				//            file.CopyTo(mem);
				//            mem.Seek(0, SeekOrigin.Begin);
				//            var entity = _strategy.Deserialize<TEntity>(mem);
				//            result = update(entity);
				//        }
				//    }
				//    // some serializers have nasty habbit of closing the
				//    // underling stream
				//    using (var mem = new MemoryStream())
				//    {
				//        _strategy.Serialize(result, mem);
				//        var data = mem.ToArray();

				//        file.Seek(0, SeekOrigin.Begin);
				//        file.Write(data, 0, data.Length);
				//        // truncate this file
				//        file.SetLength(data.Length);
				//    }
				//    return result;
				return default(TEntity);
            }
            catch (DirectoryNotFoundException)
            {
				//var s = string.Format(
				//    "Container '{0}' does not exist. You need to initialize this atomic storage and ensure that '{1}' is known to '{2}'.",
				//    _filePath, typeof(TEntity).Name, _strategy.GetType().Name);
				throw new InvalidOperationException();
            }
        }

        public bool TryDelete()
        {
			//if (File.Exists(_filePath))
			//{
			//    File.Delete(_filePath);
			//    return true;
			//}
            return false;
        }
    }
}