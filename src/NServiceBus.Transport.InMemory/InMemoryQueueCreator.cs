using System.Threading.Tasks;

namespace NServiceBus.Transport.InMemory
{
    public class InMemoryQueueCreator : ICreateQueues
    {
        public InMemoryQueueCreator(InMemoryDatabase inMemoryDatabase)
        {
            InMemoryDatabase = inMemoryDatabase;
        }

        private InMemoryDatabase InMemoryDatabase { get; }

        public Task CreateQueueIfNecessary(QueueBindings queueBindings, string identity)
        {
            foreach (var sendingAddress in queueBindings.SendingAddresses)
            {
                InMemoryDatabase.CreateQueueIfNecessary(sendingAddress, new NsbQueue());
            }

            foreach (var receivingAddress in queueBindings.ReceivingAddresses)
            {
                InMemoryDatabase.CreateQueueIfNecessary(receivingAddress, new NsbQueue());
            }

            return Task.CompletedTask;
        }
    }
}
