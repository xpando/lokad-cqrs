using System.Collections.Generic;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public interface IAtomicStorageFactory 
    {
        IAtomicWriter<TKey,TEntity> GetEntityWriter<TKey,TEntity>();
        IAtomicReader<TKey,TEntity> GetEntityReader<TKey,TEntity>();

        /// <summary>
        /// Call this once on start-up to initialize folders
        /// </summary>
        IEnumerable<string> Initialize();
    }
}