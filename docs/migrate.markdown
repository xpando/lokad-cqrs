---
layout: post
title: Lokad.CQRS Migration Notes
---

Migrate from v2.0 to vNext
=========================

Below are some samples to demonstrate changes in Lokad.Cqrs configuration API from version 2 
(and some times before) to Next.

Please keep in mind, that vNext is not a release yet. Although we use it in production already,
it is and will be changed rapidly to accomodate features for various deployments as we discover them.

If you need something stable you are encouraged to use v2 for the time being.

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
            d.InAssemblyOf<RunTaskCommand>(); 
        });

**After**

    builder.MessagesWithHandlersFromAutofac(d =>
        {
            d.HandlerSample<Define.Handle<Define.Command>>(m => m.Handle(null));            
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


Something does not work out?
----------------------------

You can also check out:

* Snippets project in branch `next` of [lokad-cqrs](https://github.com/Lokad/lokad-cqrs)
* [Lokad-cqrs-samples](https://github.com/Lokad/lokad-cqrs-samples)
* [Community](http://groups.google.com/group/lokad)

This document is stored on github in `gh-pages` branch of lokad-cqrs (see [source](https://github.com/Lokad/lokad-cqrs/edit/gh-pages/docs/migrate.markdown))

If you have found a typo or want to add another sample to this document, it would be awesome to send us
a [pull request](http://help.github.com/send-pull-requests/) with the changes. 