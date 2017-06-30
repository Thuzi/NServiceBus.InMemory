using NServiceBus.Transports;
using NServiceBus.Unicast;

namespace NServiceBus.InMemory
{
    public class SendMessages : ISendMessages
    {
        public InMemoryDatabase InMemoryDatabase { get; set; }
        public void Send(TransportMessage message, SendOptions sendOptions)
        {
            InMemoryDatabase.SendWithDelay(new SerializableTransportMessage(message), new SerializableSendOptions(sendOptions));
        }
    }
}
