namespace Lokad.Cqrs
{
    public interface ISqlStorageConfig
    {
        string Name { get; }
        string ConnectionString { get; }
    }
}
