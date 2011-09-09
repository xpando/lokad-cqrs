---
layout: post
title: Lokad.CQRS Migration Notes
---

Migrate from v2.0 to v3.0
=========================

These are before/after samples to demonstrate changes in Lokad.Cqrs configuration API from version 2 
(and some times before) to Next.

Domain configuration
--------------------

`Domain` was replaced with a set of container-specific configs to wire handler classes 
(classes that process messages). At the moment of writing we support:

* Autofac in `Lokad.Cqrs.Autofac.dll` (you'll need to use `Autofac.dll` as well)
* StructureMap in `Lokad.Cqrs.StructureMap.dll` (you'll need to use `StructureMap.dll` as well)
  
Note, that if you use something like Greg's lambda handlers, you don't need handler classes and any container at all.

**Before**

    builder.Domain(d =>
        {
            d.HandlerSample<IConsume<Define.Command>>(m => m.Consume(null));
            d.ContextFactory((envelope, message) => new MessageContext(envelope.EnvelopeId, envelope.CreatedOnUtc));
            d.InAssemblyOf<RunTaskCommand>(); 
        });

**After**

    builder.MessagesWithHandlersFromAutofac(d =>
        {
            d.HandlerSample<Define.Handle<Define.Command>>(m => m.Handle(null));
            d.ContextFactory((envelope, message) => new MessageContext(envelope.EnvelopeId, envelope.CreatedOnUtc));
            d.InAssemblyOf<RunTaskCommand>();
        });

Queue and listener configuration
--------------------------------

Directory filter configs were moved into Dispatch statement.

**Before**

    builder.Azure(m =>
        {
            m.AddAzureSender(dev, IdFor.CommandsQueue);
            m.AddAzureProcess(dev, IdFor.CommandsQueue, x =>
                {
                    x.DirectoryFilter(f => f.WhereMessagesAre<Define.Command>());
                    x.DispatchAsCommandBatch();
                    x.Quarantine(c => new Quarantine(c.Resolve<IStreamingRoot>()));
                    x.DecayPolicy(TimeSpan.FromSeconds(0.75));
                });
        });

**After**

    builder.Azure(m =>
        {
            m.AddAzureSender(dev, IdFor.CommandsQueue);
            m.AddAzureProcess(dev, IdFor.CommandsQueue, x =>
                {
                    x.DispatchAsCommandBatch(f => f.WhereMessagesAre<Define.Command>());
                    x.Quarantine(c => new Quarantine(c.Resolve<IStreamingRoot>()));
                    x.DecayPolicy(TimeSpan.FromSeconds(0.75));
                });
        });
        
Atomic Storage
--------------

Only specialized atomic interfaces were changed (if you used them).


    Before                                                  After
    IAtomicEntityWriter<TKey, TValue>                       IAtomicWriter<TKey, TValue>
    IAtomicEntityReader<TKey, TValue>                       IAtomicReader<TKey, TValue>
    IAtomicSingletonWriter<TValue>                          IAtomicWriter<unit, TValue>
    IAtomicSingletonReader<TValue>                          IAtomicReader<unit, TValue>
