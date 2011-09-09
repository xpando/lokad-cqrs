using System;
using System.Linq;
using Lokad.Cqrs.Build.Engine;
using StructureMap;
using Container = Lokad.Cqrs.Core.Container;

namespace Lokad.Cqrs.Feature.HandlerClasses
{
    public sealed class StructureMapImportTask : IEngineStartupTask
    {
        readonly IContainer _structureMapContainer;

        public StructureMapImportTask(IContainer structureMapContainer)
        {
            _structureMapContainer = structureMapContainer;
        }

        public void Execute(CqrsEngineHost host)
        {
            _structureMapContainer.Configure(c =>
                {
                    foreach (var service in host.Container.Services)
                    {
                        var type = service.Value.GetType().GetGenericArguments()[0];
                        if (_structureMapContainer.Model.PluginTypes.Any(p => p.PluginType == type))
                            continue;

                        c.For(type).Use((ctx) =>
                            {
                                var result = typeof(Container)
                                    .GetMethod("Resolve", new Type[0])
                                    .MakeGenericMethod(type)
                                    .Invoke(host.Container, null);
                                return result;
                            });
                    }
                });
        }
    }
}