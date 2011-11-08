using System;
using System.Data.Linq;
using System.IO;
using System.Linq;
using Lokad.Cqrs.Feature.AtomicStorage;

namespace Lokad.Cqrs.SqlServer.Feature.AtomicStorage
{
	public sealed class SqlAtomicEntityContainer<TKey, TEntity> : IAtomicEntityReader<TKey, TEntity>, IAtomicEntityWriter<TKey, TEntity>
	{
		readonly IAtomicStorageStrategy _strategy;
		readonly ISqlStorageConfig _config;

		public SqlAtomicEntityContainer(ISqlStorageConfig config, IAtomicStorageStrategy strategy)
		{
			_strategy = strategy;
			_config = config;
		}

		public bool TryGet(TKey key, out TEntity view)
		{
			view = default(TEntity);
			try
			{
				var path = _strategy.GetNameForEntity(typeof(TEntity), key);

				using (var db = new DataContext(_config.ConnectionString))
				{
					var result = db.ExecuteQuery<BlobResult>("exec [dbo].[Blob_Get] {0}", path)
						.SingleOrDefault();

					if (result != null)
					{
						using (MemoryStream stream = new MemoryStream(result.Blob.ToArray()))
							view = _strategy.Deserialize<TEntity>(stream);

						return true;
					}
					else
					{
						return false;
					}
				}
			}
			catch
			{
				//To do: log exception
				return false;
			}
		}

		public TEntity AddOrUpdate(TKey key, Func<TEntity> addFactory, Func<TEntity, TEntity> update, AddOrUpdateHint hint)
		{
			try
			{
				TEntity result = default(TEntity);
				string sqlCommand = string.Empty;

				if (!TryGet(key, out result))
				{
					result = addFactory();
					sqlCommand = "exec [dbo].[Blob_Insert] {0}, {1}";
				}
				else
				{
					result = update(result);
					sqlCommand = "exec [dbo].[Blob_Update] {0}, {1}";
				}

				var name = _strategy.GetNameForEntity(typeof(TEntity), key);

				using (var db = new DataContext(_config.ConnectionString))
				using (var stream = new MemoryStream())
				{
					_strategy.Serialize<TEntity>(result, stream);
					Binary blob = stream.ToArray();
					db.ExecuteCommand
					(
						sqlCommand,
						name,
						blob
					);
				}
				return result;
			}
			catch
			{
				//to do: log error
				throw new InvalidOperationException();
			}
		}

		public bool TryDelete(TKey key)
		{
			var path = _strategy.GetNameForEntity(typeof(TEntity), key);

			try
			{
				using (var db = new DataContext(_config.ConnectionString))
				{
					db.ExecuteCommand
					(
						"exec [dbo].[Blob_Delete] {0}",
						path
					);
				}
				return true;
			}
			catch
			{
				//to do: log error
				return false;
			}
		}
	}
}