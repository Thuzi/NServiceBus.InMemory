using NServiceBus.Transports;

namespace NServiceBus.Transport.InMemory
{
    public class CreateQueues : ICreateQueues
    {
        public InMemoryDatabase InMemoryDatabase { get; set; }
        public void CreateQueueIfNecessary(Address address, string account)
        {
            InMemoryDatabase.CreateQueueIfNecessary(address.Queue, new NsbQueue(InMemoryDatabase));
        }
    }
}
