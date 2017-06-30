using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Logging;

namespace NServiceBus.InMemory
{
    /// <summary>
    /// The in memory database for nsb.
    /// </summary>
    public class InMemoryDatabase : MarshalByRefObject, IDisposable
    {
        private DateTime nextMessageProcessedOn = DateTime.MaxValue;
        private TaskCompletionSource<bool> delayMessageWaiter = new TaskCompletionSource<bool>();
        private Thread delayMessageThread;
        private bool started;
        private readonly ConcurrentDictionary<string, HashSet<string>> topics = new ConcurrentDictionary<string, HashSet<string>>();
        private readonly ConcurrentDictionary<string, NsbQueue> queues = new ConcurrentDictionary<string, NsbQueue>();
        private readonly ConcurrentDictionary<string, Tuple<SerializableTransportMessage, SerializableSendOptions, DateTime>> delayedMessages = new ConcurrentDictionary<string, Tuple<SerializableTransportMessage, SerializableSendOptions, DateTime>>();
        private readonly ILog log = LogManager.GetLogger<InMemoryTransport>();
        private void delayMessageThreadLoop(object state)
        {
            try
            {
                var min = DateTime.MaxValue;

                while (true)
                {
                    var now = DateTime.UtcNow;

                    if (min == DateTime.MaxValue)
                    {
                        nextMessageProcessedOn = DateTime.MaxValue;
                        delayMessageWaiter.Task.Wait();
                    }
                    else if (min > now)
                    {
                        nextMessageProcessedOn = min;
                        Task.WhenAny(delayMessageWaiter.Task, Task.Delay(nextMessageProcessedOn - now)).Wait();
                    }

                    delayMessageWaiter = new TaskCompletionSource<bool>();

                    if (started)
                    {
                        do
                        {
                            var keys = delayedMessages.Keys.ToArray();

                            now = DateTime.UtcNow;
                            min = DateTime.MaxValue;

                            foreach (var key in keys)
                            {
                                var request = delayedMessages[key];

                                if (request.Item3 <= now && delayedMessages.TryRemove(key, out request))
                                {
                                    send(request.Item1, request.Item2);
                                }
                                else if (min > request.Item3)
                                {
                                    min = request.Item3;
                                }
                            }
                        }
                        while (min <= now);
                    }
                }
            }
            catch (ThreadAbortException)
            {
            }
        }
        private void send(SerializableTransportMessage message, SerializableSendOptions sendOptions)
        {
            try
            {
                queues[sendOptions.Destination.Queue].AddMessage(message);
            }
            catch (Exception error)
            {
                log.Error("Failed to send message.", error);
            }
        }

        public override string ToString()
        {
            return $"Queues: {queues.Count}, Topics: {topics.Count}, Scheduled: {delayedMessages.Count}";
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
        /// If the server is currently processing messages.
        /// </summary>
        public bool Enabled => started;
        
        /// <summary>
        /// Called when the bus wants to defer a message
        /// </summary>
        public void ClearDeferredMessages(string headerKey, string headerValue)
        {
            var value = delayedMessages.Values
                .FirstOrDefault(item =>
                    item.Item1.Headers.ContainsKey(headerKey) &&
                    item.Item1.Headers[headerKey] == headerValue);
            if (value != null)
            {
                delayedMessages.TryRemove(value.Item1.Id, out value);
            }
        }

        /// <summary>
        /// Abstraction of the capability to create queues
        /// </summary>
        public bool CreateQueueIfNecessary(string queueName, NsbQueue queue)
        {
            return queues.TryAdd(queueName, queue);
        }

        /// <summary>
            /// Stops the thread for processing delayed messages.
            /// </summary>
        public void Dispose()
        {
            var delayMessageThreadCopy = delayMessageThread;
            delayMessageThread = null;
            if (delayMessageThreadCopy != null && delayMessageThreadCopy.IsAlive)
            {
                delayMessageThreadCopy.Abort();
            }
        }

        /// <summary>
        /// Publishes the given messages to all known subscribers
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="publishOptions">The publish options for the message.</param>
        public void Publish(SerializableTransportMessage message, SerializablePublishOptions publishOptions)
        {
            HashSet<string> endpoints;
            if (topics.TryGetValue(publishOptions.EventType, out endpoints))
            {
                foreach (var endpoint in endpoints)
                {
                    NsbQueue eventQueue;
                    if (queues.TryGetValue(endpoint, out eventQueue))
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
        
        /// <summary>
        /// If the message needs to be delayed then add it the delayed message collection else add it to the queue.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="sendOptions">The send options for the message.</param>
        public void SendWithDelay(SerializableTransportMessage message, SerializableSendOptions sendOptions)
        {
            var now = DateTime.UtcNow;

            var dueTime = sendOptions.DeliverAt ?? now + (sendOptions.DelayDeliveryWith ?? TimeSpan.Zero);

            //if the message should be deferred
            if (dueTime > now)
            {
                delayedMessages[message.Id] = new Tuple<SerializableTransportMessage, SerializableSendOptions, DateTime>(message, sendOptions, dueTime);

                if (nextMessageProcessedOn > dueTime)
                {
                    delayMessageWaiter.TrySetResult(true);
                }
            }
            else
            {
                send(message, sendOptions);
            }
        }

        /// <summary>
        /// Stops the server from processing any more messages.
        /// </summary>
        /// <param name="purge">If true all messages in the system will be deleted.</param>
        public void StopServer(bool purge = true)
        {
            nextMessageProcessedOn = DateTime.MaxValue;
            started = false;

            foreach (var queue in queues.Values)
            {
                queue.Enabled = false;
                if (purge)
                {
                    queue.Clear();
                }
            }

            if (purge)
            {
                delayedMessages.Clear();
            }
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        /// <param name="purge">If true all messages in the system will be deleted.</param>
        public void StartServer(bool purge = true)
        {
            StopServer(purge);

            nextMessageProcessedOn = DateTime.MaxValue;
            started = true;

            if (delayMessageThread == null)
            {
                delayMessageThread = new Thread(delayMessageThreadLoop);
                delayMessageThread.Start();
            }
            
            foreach (var queue in queues.Values)
            {
                queue.Enabled = true;
                if (!queue.IsEmpty)
                {
                    queue.ProcessQueue();
                }
            }
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
