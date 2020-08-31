using System;
using System.Threading.Tasks;
using NServiceBus.Extensibility;

namespace NServiceBus.Transport.InMemory
{
    public class SubscriptionManager : IManageSubscriptions
    {
        public EndpointInfo Endpoint { get; set; }

        public InMemoryDatabase InMemoryDatabase { get; set; }

        public Task Subscribe(Type eventType, ContextBag context)
        {
            InMemoryDatabase.Subscribe(eventType.AssemblyQualifiedName, Endpoint.Name);

            return Task.CompletedTask;
        }

        public Task Unsubscribe(Type eventType, ContextBag context)
        {
            InMemoryDatabase.Unsubscribe(eventType.AssemblyQualifiedName, Endpoint.Name);

            return Task.CompletedTask;
        }
    }
}
