This is before/after samples to demonstrate changes in Lokad.Cqrs configuration API from version 2 (and some times before) to Next.

Domain configuration
--------------------

Before

    builder.Domain(d =>
        {
            d.HandlerSample<IConsume<Define.Command>>(m => m.Consume(null));
            d.ContextFactory((envelope, message) => new MessageContext(envelope.EnvelopeId, envelope.CreatedOnUtc));
            d.InAssemblyOf<RunTaskCommand>(); 
        });

After

    builder.MessagesWithHandlersFromAutofac(d =>
        {
            d.HandlerSample<Define.Handle<Define.Command>>(m => m.Handle(null));
            d.ContextFactory((envelope, message) => new MessageContext(envelope.EnvelopeId, envelope.CreatedOnUtc));
            d.InAssemblyOf<RunTaskCommand>();
        });

Queue and listener configuration
--------------------------------

Before

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

After

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
