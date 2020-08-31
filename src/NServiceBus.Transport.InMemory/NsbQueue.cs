using System;
using System.Collections.Concurrent;

namespace NServiceBus.Transport.InMemory
{
    /// <summary>
    /// Represents a queue for an end point.
    /// </summary>
    public class NsbQueue : MarshalByRefObject
    {
        private readonly ConcurrentQueue<SerializedMessage> queue = new ConcurrentQueue<SerializedMessage>();

        public override string ToString()
        {
            return $"Pending: {queue.Count}";
        }

        /// <summary>
        /// If the queue is empty.
        /// </summary>
        public bool IsEmpty => queue.IsEmpty;

        /// <summary>
        /// Adds a message to the queue.
        /// </summary>
        /// <param name="message">The message to enqueue</param>
        public void AddMessage(SerializedMessage message)
        {
            queue.Enqueue(message);
        }

        /// <summary>
        /// Removes all messages from the queue.
        /// </summary>
        public void Clear()
        {
            while (!queue.IsEmpty)
            {
                queue.TryDequeue(out var message);
            }
        }

        public bool TryDequeue(out SerializedMessage message)
        {
            return queue.TryDequeue(out message);
        }
    }
}
