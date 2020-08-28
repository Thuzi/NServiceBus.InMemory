using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NServiceBus.Logging;

namespace NServiceBus.Transport.InMemory
{
    /// <summary>
    /// The in memory database for nsb.
    /// </summary>
    public class InMemoryDatabase : MarshalByRefObject
    {
        private readonly ConcurrentDictionary<string, HashSet<string>> topics = new ConcurrentDictionary<string, HashSet<string>>();
        private readonly ConcurrentDictionary<string, NsbQueue> queues = new ConcurrentDictionary<string, NsbQueue>();
        private readonly ILog log = LogManager.GetLogger<InMemoryTransport>();

        private void send(OutgoingMessage message, string destination)
        {
            try
            {
                queues[destination].AddMessage(message.Serialize());
            }
            catch (Exception error)
            {
                log.Error("Failed to send message.", error);
            }
        }

        public override string ToString()
        {
            return $"Queues: {queues.Count}, Topics: {topics.Count}";
        }

        /// <summary>
        /// Gets a queue if one exists else returns null.
        /// </summary>
        public NsbQueue GetQueue(string queueName)
        {
            NsbQueue queue;
            return queues.TryGetValue(queueName, out queue) ? queue : null;
        }

        /// <summary>
        /// Abstraction of the capability to create queues
        /// </summary>
        public bool CreateQueueIfNecessary(string queueName, NsbQueue queue)
        {
            return queues.TryAdd(queueName, queue);
        }

        /// <summary>
        /// Publishes the given messages to all known subscribers
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageType">The type of the message.</param>
        public void Publish(OutgoingMessage message, Type messageType)
        {
            if (topics.TryGetValue(messageType.AssemblyQualifiedName, out var endpoints))
            {
                foreach (var endpoint in endpoints)
                {
                    if (queues.TryGetValue(endpoint, out var eventQueue))
                    {
                        eventQueue.AddMessage(message.Serialize());
                    }
                    else
                    {
                        throw new InvalidProgramException("Unable to add event message to the queue.");
                    }
                }
            }
            else
            {
                log.Warn($"Unable to publish message '{messageType}' because no endpoint subscribed to the message.");
            }
        }

        public void Send(UnicastTransportOperation transportOperation)
        {
            send(transportOperation.Message, transportOperation.Destination);
        }

        /// <summary>
        /// Subscribes to the given event.
        /// </summary>
        /// <param name="eventType">The event type</param>
        /// <param name="endpointName">The endpoint name</param>
        public void Subscribe(string eventType, string endpointName)
        {
            if (!topics.TryAdd(eventType, new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                endpointName
            }))
            {
                topics[eventType].Add(endpointName);
            }
        }

        /// <summary>
        /// Unsubscribes from the given event.
        /// </summary>
        /// <param name="eventType">The event type</param>
        /// <param name="endpointName">The endpoint name</param>
        public void Unsubscribe(string eventType, string endpointName)
        {
            HashSet<string> endpoints;
            if (topics.TryGetValue(eventType, out endpoints))
            {
                endpoints.Remove(endpointName);
            }
        }
    }
}
