using NServiceBus.Transports;
using NServiceBus.Unicast;

namespace NServiceBus.InMemory
{
    public class PublishMessages : IPublishMessages
    {
        public InMemoryDatabase InMemoryDatabase { get; set; }
        public void Publish(TransportMessage message, PublishOptions publishOptions)
        {
            InMemoryDatabase.Publish(new SerializableTransportMessage(message), new SerializablePublishOptions(publishOptions));
        }
    }
}
