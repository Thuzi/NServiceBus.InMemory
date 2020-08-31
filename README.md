# NServiceBus.InMemory

[![Build status](https://ci.appveyor.com/api/projects/status/30c67gx3qdcwrp9h?svg=true)](https://ci.appveyor.com/project/CyAScott/nservicebus-inmemory) [![NuGet Badge](https://buildstats.info/nuget/NServiceBus.InMemory)](https://www.nuget.org/packages/NServiceBus.InMemory/)

A thread safe in memory transport that works across AppDomains for NServiceBus for acceptance testing only. For information about 
acceptance testing with NServiceBus read [this blog](https://roycornelissen.wordpress.com/2014/10/25/automating-end-to-end-nservicebus-tests-with-nservicebus-acceptancetesting/). In that blog they used the MSMQ transport for testing. However, this may not be the best option for your test environment. The NServiceBus.InMemory transport can be used as an in process transport that can exchange messages between endpoints for your acceptance tests. To install the transport simply run this command in you NuGet console in Visual Studio:

`Install-Package NServiceBus.InMemory`


### NServiceBus 7.x Support
This transport only supports NServiceBus 7.x.