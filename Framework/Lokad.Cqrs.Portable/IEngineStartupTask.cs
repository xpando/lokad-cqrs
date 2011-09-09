using Lokad.Cqrs.Build.Engine;

namespace Lokad.Cqrs
{
    public interface IEngineStartupTask
    {
        void Execute(CqrsEngineHost host);
    }
}