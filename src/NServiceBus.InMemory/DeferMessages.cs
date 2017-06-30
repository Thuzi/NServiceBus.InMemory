using NServiceBus.Transports;
using NServiceBus.Unicast;

namespace NServiceBus.InMemory
{
    public class DeferMessages : IDeferMessages
    {
        public InMemoryDatabase InMemoryDatabase { get; set; }
        public void Defer(TransportMessage message, SendOptions sendOptions)
        {
            InMemoryDatabase.SendWithDelay(new SerializableTransportMessage(message), new SerializableSendOptions(sendOptions));
        }
        public void ClearDeferredMessages(string headerKey, string headerValue)
        {
            InMemoryDatabase.ClearDeferredMessages(headerKey, headerValue);
        }
    }
}
