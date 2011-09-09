#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using System.Linq;
using Lokad.Cqrs.Build.Engine;
using StructureMap;
using Container = Lokad.Cqrs.Core.Container;

namespace Lokad.Cqrs.Feature.HandlerClasses
{
    /// <summary>
    /// Class capable of building nested container provider
    /// </summary>
    public class StructureMapContainerProvider
    {
        readonly IContainer _strutureMapContainer;

        public StructureMapContainerProvider(IContainer strutureMapContainer)
        {
            _strutureMapContainer = strutureMapContainer;
        }

        public IContainerForHandlerClasses Build(Container container, Type[] handlerTypes)
        {
            var containerHandler = new StructureMapContainerForHandlerClasses(_strutureMapContainer, container);

            _strutureMapContainer.Configure(c =>
                {
                    foreach (var handlerType in handlerTypes)
                        c.For(handlerType);
                });

            container.Register(_strutureMapContainer);

            return containerHandler;
        }
    }

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