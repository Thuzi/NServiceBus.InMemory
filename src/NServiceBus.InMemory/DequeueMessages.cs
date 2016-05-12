using System;
using NServiceBus.Transports;
using NServiceBus.Unicast.Transport;

namespace NServiceBus.InMemory
{
    public class DequeueMessages : IDequeueMessages
    {
        private NsbQueue queue;

        public InMemoryDatabase InMemoryDatabase { get; set; }
        public void Init(Address address, TransactionSettings transactionSettings, Func<TransportMessage, bool> tryProcessMessage, Action<TransportMessage, Exception> endProcessMessage)
        {
            if (!InMemoryDatabase.Queues.TryGetValue(address.Queue, out queue)) queue = new NsbQueue
            {
                Enabled = false,
                MaximumConcurrencyLevel = 1
            };

            queue.Address = address;
            queue.Finalizer = endProcessMessage;
            queue.Handler = tryProcessMessage;
            queue.TransactionSettings = transactionSettings;
        }
        public void Start(int maximumConcurrencyLevel)
        {
            queue.Enabled = true;
            queue.MaximumConcurrencyLevel = maximumConcurrencyLevel;
        }
        public void Stop()
        {
            queue.Enabled = false;
        }
    }
}
