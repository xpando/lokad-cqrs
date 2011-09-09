StructureMap Extension for Lokad.CQRS
=====================================

StructureMap Extension is kindly shared by [Andreas Ohlund](http://andreasohlund.net/)

This extension allows plugging [StructureMap](http://structuremap.net/structuremap/) 
IoC container in Lokad.CQRS engine to manage resolution and lifetime of message 
handler classes.

Usage
-----

First you need to reference `Lokad.Cqrs.StructureMap.dll`.

Then you wire messages and message handler classes together using something like this:
	
    builder.MessagesWithHandlersFromStructureMap(d => 
    {   
        // put your specific config code here
        d => d.HandlerSample<IMyHandle<IMyCommand>>(h => h.Handle(null)));
    });

You can also use an overload for passing preconfigured StructureMap instance.

Known limitations
-----------------

* StructureMap currently does not support nested containers, so message 
  batching might not work scenarios, when container is used to provide some
  sort of shared session with automatic transaction management.

