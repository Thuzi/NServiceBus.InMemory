using NServiceBus.Transports;

namespace NServiceBus.InMemory
{
    public class InMemoryTransport : TransportDefinition
    {
        protected override void Configure(BusConfiguration config)
        {
            config.EnableFeature<InMemoryTransportFeature>();
        }

        public InMemoryTransport()
        {
            HasNativePubSubSupport = true;
            HasSupportForCentralizedPubSub = true;
        }
    }
}
