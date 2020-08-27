using System;
using NServiceBus.Transports;
using NServiceBus.Unicast.Transport;

namespace NServiceBus.Transport.InMemory
{
    public class DequeueMessages : IDequeueMessages
    {
        private NsbQueue queue;

        public InMemoryDatabase InMemoryDatabase { get; set; }
        public void Init(Address address, TransactionSettings transactionSettings, Func<TransportMessage, bool> tryProcessMessage, Action<TransportMessage, Exception> endProcessMessage)
        {
            queue = InMemoryDatabase.GetQueue(address.Queue);
            if (queue == null &&
                !InMemoryDatabase.CreateQueueIfNecessary(address.Queue, queue = new NsbQueue(InMemoryDatabase)
                {
                    Enabled = false,
                    MaximumConcurrencyLevel = 1
                }))
            {
                throw new InvalidProgramException($"Unable to get or add the queue '{address.Queue}'.");
            }

            queue.Address = address;
            queue.Finalizer = endProcessMessage;
            queue.Handler = tryProcessMessage;
            queue.TransactionSettings = transactionSettings;
        }
        public void Start(int maximumConcurrencyLevel)
        {
            queue.Enabled = true;
            queue.MaximumConcurrencyLevel = maximumConcurrencyLevel < 1 ? 1 : maximumConcurrencyLevel;
        }
        public void Stop()
        {
            queue.Enabled = false;
        }
    }
}
