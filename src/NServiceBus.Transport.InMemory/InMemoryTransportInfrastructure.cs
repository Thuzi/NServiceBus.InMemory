using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus.Performance.TimeToBeReceived;
using NServiceBus.Routing;
using NServiceBus.Settings;

namespace NServiceBus.Transport.InMemory
{
    public class InMemoryTransportInfrastructure : TransportInfrastructure
    {
        private readonly InMemoryDatabase inMemoryDatabase = new InMemoryDatabase();

        public InMemoryTransportInfrastructure(SettingsHolder settings)
        {
            if (settings.TryGet(out InMemoryDatabase database))
            {
                inMemoryDatabase = database;
            }
        }

        public override TransportReceiveInfrastructure ConfigureReceiveInfrastructure()
        {
            return new TransportReceiveInfrastructure(
                () => new InMemoryMessagePump(inMemoryDatabase),
                () => new InMemoryQueueCreator(inMemoryDatabase),
                () => Task.FromResult(StartupCheckResult.Success));
        }

        public override TransportSendInfrastructure ConfigureSendInfrastructure()
        {
            return new TransportSendInfrastructure(
                () => new InMemoryMessageDispatcher(inMemoryDatabase),
                () => Task.FromResult(StartupCheckResult.Success));
        }

        public override TransportSubscriptionInfrastructure ConfigureSubscriptionInfrastructure()
        {
            return new TransportSubscriptionInfrastructure(
                () => new SubscriptionManager());
        }

        public override EndpointInstance BindToLocalEndpoint(EndpointInstance instance)
        {
            return instance;
        }

        public override string ToTransportAddress(LogicalAddress logicalAddress)
        {
            var endpointInstance = logicalAddress.EndpointInstance;
            var discriminator = endpointInstance.Discriminator ?? "";
            var qualifier = logicalAddress.Qualifier ?? "";
            return string.Join("/", endpointInstance, discriminator, qualifier);
        }

        public override IEnumerable<Type> DeliveryConstraints => new[]
        {
            typeof(DiscardIfNotReceivedBefore)
        };

        public override TransportTransactionMode TransactionMode => TransportTransactionMode.ReceiveOnly;

        public override OutboundRoutingPolicy OutboundRoutingPolicy => new OutboundRoutingPolicy(OutboundRoutingType.Unicast, OutboundRoutingType.Multicast, OutboundRoutingType.Unicast);
    }
}