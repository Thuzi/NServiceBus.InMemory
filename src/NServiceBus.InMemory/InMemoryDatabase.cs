using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Logging;
using NServiceBus.Unicast;

namespace NServiceBus.InMemory
{
    /// <summary>
    /// The in memory database for nsb.
    /// </summary>
    public class InMemoryDatabase
    {
        private DateTime nextMessageProcessedOn = DateTime.MaxValue;
        private TaskCompletionSource<bool> delayMessageWaiter = new TaskCompletionSource<bool>();
        private Thread delayMessageThread;
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

                    do
                    {
                        var keys = DelayedMessages.Keys.ToArray();

                        now = DateTime.UtcNow;
                        min = DateTime.MaxValue;

                        foreach (var key in keys)
                        {
                            var request = DelayedMessages[key];

                            if (request.Item3 <= now && DelayedMessages.TryRemove(key, out request))
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
            catch (ThreadAbortException)
            {
            }
        }
        private void send(TransportMessage message, SendOptions sendOptions)
        {
            try
            {
                Queues[sendOptions.Destination.Queue].AddMessage(message);
            }
            catch (Exception error)
            {
                log.Error("Failed to send message.", error);
            }
        }

        public override string ToString()
        {
            return $"Queues: {Queues.Count}, Topics: {Topics.Count}, Scheduled: {DelayedMessages.Count}";
        }

        /// <summary>
        /// The list of nsb event subscriptions.
        /// </summary>
        public readonly ConcurrentDictionary<Type, HashSet<string>> Topics = new ConcurrentDictionary<Type, HashSet<string>>();

        /// <summary>
        /// The list of nsb queues.
        /// </summary>
        public readonly ConcurrentDictionary<string, NsbQueue> Queues = new ConcurrentDictionary<string, NsbQueue>();
        
        /// <summary>
        /// A place holder for messages that will get added to the queue at a later time.
        /// </summary>
        public readonly ConcurrentDictionary<string, Tuple<TransportMessage, SendOptions, DateTime>> DelayedMessages = new ConcurrentDictionary<string, Tuple<TransportMessage, SendOptions, DateTime>>();

        /// <summary>
        /// If the message needs to be delayed then add it the delayed message collection else add it to the queue.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="sendOptions">The send options for the message.</param>
        public void SendWithDelay(TransportMessage message, SendOptions sendOptions)
        {
            var now = DateTime.UtcNow;

            var dueTime = sendOptions.DeliverAt ?? now + (sendOptions.DelayDeliveryWith ?? TimeSpan.Zero);

            //if the message should be deferred
            if (dueTime > now)
            {
                DelayedMessages[message.Id] = new Tuple<TransportMessage, SendOptions, DateTime>(message, sendOptions, dueTime);

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
            var delayMessageThreadCopy = delayMessageThread;
            delayMessageThread = null;
            if (delayMessageThreadCopy != null && delayMessageThreadCopy.IsAlive)
            {
                delayMessageThreadCopy.Abort();
            }
            nextMessageProcessedOn = DateTime.MaxValue;

            foreach (var queue in Queues.Values)
            {
                queue.Enabled = false;
                if (purge)
                {
                    queue.Clear();
                }
            }

            if (purge)
            {
                DelayedMessages.Clear();
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
            delayMessageThread = new Thread(delayMessageThreadLoop);
            delayMessageThread.Start();
            
            foreach (var queue in Queues.Values)
            {
                queue.Enabled = true;
                if (!queue.IsEmpty)
                {
                    queue.ProcessQueue();
                }
            }
        }
    }
}
