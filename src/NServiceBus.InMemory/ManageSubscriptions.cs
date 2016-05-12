using System;
using System.Collections.Generic;
using NServiceBus.Transports;

namespace NServiceBus.InMemory
{
    public class ManageSubscriptions : IManageSubscriptions
    {
        public EndpointInfo Endpoint { get; set; }
        public InMemoryDatabase InMemoryDatabase { get; set; }
        public void Subscribe(Type eventType, Address publisherAddress)
        {
            if (!InMemoryDatabase.Topics.TryAdd(eventType, new HashSet<string>
            {
                Endpoint.Name
            }))
            {
                InMemoryDatabase.Topics[eventType].Add(Endpoint.Name);
            }
        }
        public void Unsubscribe(Type eventType, Address publisherAddress)
        {
            HashSet<string> endpoints;
            if (InMemoryDatabase.Topics.TryGetValue(eventType, out endpoints))
            {
                endpoints.Remove(Endpoint.Name);
            }
        }
    }
}
