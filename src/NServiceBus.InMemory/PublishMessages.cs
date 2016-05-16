using System;
using System.Collections.Generic;
using NServiceBus.Transports;
using NServiceBus.Logging;
using NServiceBus.Unicast;

namespace NServiceBus.InMemory
{
    public class PublishMessages : IPublishMessages
    {
        private readonly ILog log = LogManager.GetLogger<InMemoryTransport>();

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
                    else
                    {
                        throw new InvalidProgramException("Unable to add event message to the queue.");
                    }
                }
            }
            else
            {
                log.Warn($"Unable to publish message '{publishOptions.EventType}' because no endpoint subscribed to the message.");
            }
        }
    }
}
