using NServiceBus.Transports;

namespace NServiceBus.InMemory
{
    public class CreateQueues : ICreateQueues
    {
        public InMemoryDatabase InMemoryDatabase { get; set; }
        public void CreateQueueIfNecessary(Address address, string account)
        {
            InMemoryDatabase.Queues.TryAdd(address.Queue, new NsbQueue(InMemoryDatabase));
        }
    }
}
