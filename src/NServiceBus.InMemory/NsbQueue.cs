using System;
using System.Collections.Concurrent;
using System.Threading;
using NServiceBus.Logging;
using NServiceBus.Unicast.Transport;

namespace NServiceBus.InMemory
{
    /// <summary>
    /// Represents a queue for an end point.
    /// </summary>
    public class NsbQueue : ConcurrentQueue<TransportMessage>
    {
        private int concurrencyLevel;
        private readonly InMemoryDatabase inMemoryDatabase;
        private readonly ILog log = LogManager.GetLogger<NsbQueue>();
        private void workerThread(object state)
        {
            var finalizer = Finalizer;
            var handler = Handler;

            if (finalizer != null && handler != null)
            {
                while (!IsEmpty && inMemoryDatabase.Enabled)
                {
                    TransportMessage message;
                    if (TryDequeue(out message))
                    {
                        Exception handlerException = null;
                        try
                        {
                            log.Debug($"Processing Message: {message.Headers["NServiceBus.EnclosedMessageTypes"]}");
                            handler(message);
                        }
                        catch (Exception error)
                        {
                            log.Error($"Error Processing Message: {message.Headers["NServiceBus.EnclosedMessageTypes"]}", error);
                            handlerException = error;
                        }
                        finally
                        {
                            try
                            {
                                if (inMemoryDatabase.Enabled)
                                {
                                    log.Debug($"Done Processing Message: {message.Headers["NServiceBus.EnclosedMessageTypes"]}");
                                    finalizer(message, handlerException);
                                }
                            }
                            catch (Exception badHandlerException)
                            {
                                log.Error("Error finalizing the message handling.", badHandlerException);
                            }
                        }
                    }
                }
            }
            else
            {
                Timer timer = null;
                timer = new Timer(timerState =>
                {
                    if (timer != null)
                    {
                        timer.Dispose();
                    }

                    if (inMemoryDatabase.Enabled)
                    {
                        ProcessQueue();
                    }
                }, 
                null, TimeSpan.FromSeconds(1), TimeSpan.FromDays(1));
            }

            Interlocked.Decrement(ref concurrencyLevel);
        }

        /// <summary>
        /// Creates a NSB queue that processes messages.
        /// </summary>
        /// <param name="database">The owning database.</param>
        public NsbQueue(InMemoryDatabase database)
        {
            inMemoryDatabase = database;
        }
        public override string ToString()
        {
            return $"Pending: {Count}, Threads: {concurrencyLevel}";
        }

        /// <summary>
        /// The end point's address.
        /// </summary>
        public Address Address { get; set; }
        /// <summary>
        /// The action that is invoked after the message is handled.
        /// </summary>
        public Action<TransportMessage, Exception> Finalizer { get; set; }
        /// <summary>
        /// The function that handles the message.
        /// </summary>
        public Func<TransportMessage, bool> Handler { get; set; }
        /// <summary>
        /// The transaction settings for the queue.
        /// </summary>
        public TransactionSettings TransactionSettings { get; set; }
        /// <summary>
        /// If the queue processing is enabled.
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// The maximum number of threads that can process messages from the queue.
        /// </summary>
        public int MaximumConcurrencyLevel { get; set; } = 1;
        /// <summary>
        /// The queue name.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Adds a message to the queue.
        /// </summary>
        /// <param name="message">The message to add.</param>
        public void AddMessage(TransportMessage message)
        {
            Enqueue(message);
            ProcessQueue();
        }
        /// <summary>
        /// Removes all messages from the queue.
        /// </summary>
        public void Clear()
        {
            while (!IsEmpty)
            {
                TransportMessage message;
                TryDequeue(out message);
            }
        }
        /// <summary>
        /// Ensures that at least one worker is processing the queue.
        /// </summary>
        public void ProcessQueue()
        {
            if (IsEmpty || !Enabled) return;

            if (MaximumConcurrencyLevel > 0 && Interlocked.Increment(ref concurrencyLevel) > MaximumConcurrencyLevel)
            {
                Interlocked.Decrement(ref concurrencyLevel);
            }
            else
            {
                ThreadPool.QueueUserWorkItem(workerThread);
            }
        }
    }
}
