using System.Threading.Tasks;
using NServiceBus.Extensibility;

namespace NServiceBus.Transport.InMemory
{
    public class InMemoryMessageDispatcher : IDispatchMessages
    {
        public InMemoryMessageDispatcher(InMemoryDatabase inMemoryDatabase)
        {
            InMemoryDatabase = inMemoryDatabase;
        }

        private InMemoryDatabase InMemoryDatabase { get; }

        public Task Dispatch(TransportOperations outgoingMessages, TransportTransaction transaction, ContextBag context)
        {
            foreach (var transportOperation in outgoingMessages.UnicastTransportOperations)
            {
                InMemoryDatabase.Send(transportOperation);
            }

            foreach (var transportOperation in outgoingMessages.MulticastTransportOperations)
            {
                InMemoryDatabase.Publish(transportOperation.Message, transportOperation.MessageType);
            }

            return Task.CompletedTask;
        }
    }
}
