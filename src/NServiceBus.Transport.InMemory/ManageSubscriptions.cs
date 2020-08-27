using System;
using NServiceBus.Transports;

namespace NServiceBus.Transport.InMemory
{
    public class ManageSubscriptions : IManageSubscriptions
    {
        public EndpointInfo Endpoint { get; set; }
        public InMemoryDatabase InMemoryDatabase { get; set; }
        public void Subscribe(Type eventType, Address publisherAddress)
        {
            InMemoryDatabase.Subscribe(eventType.AssemblyQualifiedName, Endpoint.Name);
        }
        public void Unsubscribe(Type eventType, Address publisherAddress)
        {
            InMemoryDatabase.Unsubscribe(eventType.AssemblyQualifiedName, Endpoint.Name);
        }
    }
}
