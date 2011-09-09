#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;
using StructureMap;
using Container = Lokad.Cqrs.Core.Container;

namespace Lokad.Cqrs.Feature.HandlerClasses
{
    /// <summary>
    /// Class capable of building nested container provider
    /// </summary>
    public static class StructureMapContainerProvider
    {
        public static IContainerForHandlerClasses Build(Container container, Type[] handlerTypes,
            IContainer structureMap)
        {
            structureMap.Configure(c =>
                {
                    foreach (var handlerType in handlerTypes)
                        c.For(handlerType);
                });
            container.Register(structureMap);
            // dispose container, when server shuts down
            container.TrackDisposable(structureMap);
            return new StructureMapContainerForHandlerClasses(structureMap);
        }
    }
}