namespace Lokad.Cqrs
{
    public class SqlStorageConfig : ISqlStorageConfig
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
    }
}
