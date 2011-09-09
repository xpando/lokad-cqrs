using System;
using System.Linq;
using System.Reflection;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Evil;
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

                        var method = typeof(Container)
                            .GetMethod("Resolve", new Type[0])
                            .MakeGenericMethod(type);
                        c.For(type).Use(ctx =>
                            {
                                try
                                {
                                    return method.Invoke(host.Container, null);
                                }
                                catch (TargetInvocationException tie)
                                {
                                    throw InvocationUtil.Inner(tie);
                                }
                            });
                    }
                });
        }
    }
}