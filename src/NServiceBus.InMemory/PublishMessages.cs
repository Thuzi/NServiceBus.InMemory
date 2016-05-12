using System.Collections.Generic;
using NServiceBus.Transports;
using NServiceBus.Unicast;

namespace NServiceBus.InMemory
{
    public class PublishMessages : IPublishMessages
    {
        public InMemoryDatabase InMemoryDatabase { get; set; }
        public void Publish(TransportMessage message, PublishOptions publishOptions)
        {
            HashSet<string> endpoints;
            if (InMemoryDatabase.Topics.TryGetValue(publishOptions.EventType, out endpoints))
            {
                foreach (var endpoint in endpoints)
                {
                    NsbQueue eventQueue;
                    if (InMemoryDatabase.Queues.TryGetValue(endpoint, out eventQueue))
                    {
                        eventQueue.AddMessage(message);
                    }
                }
            }
        }
    }
}
