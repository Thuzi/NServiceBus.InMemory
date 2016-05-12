using NServiceBus.Features;
using NServiceBus.Settings;
using NServiceBus.Transports;

namespace NServiceBus.InMemory
{
    public class InMemoryTransportFeature : ConfigureTransport
    {
        protected override string GetLocalAddress(ReadOnlySettings settings)
        {
            return Address.Parse(settings.Get<string>("NServiceBus.LocalAddress")).Queue;
        }

        protected override void Configure(FeatureConfigurationContext context, string connectionString)
        {
            var endpointInfo = new EndpointInfo
            {
                Name = GetLocalAddress(context.Settings)
            };
            
            var server = new InMemoryDatabase();
            
            context.Container
                .RegisterSingleton(endpointInfo)
                .RegisterSingleton(new RunWhenBusStartsAndStops())
                .RegisterSingleton<ICreateQueues>(new CreateQueues())
                .RegisterSingleton<IDeferMessages>(new DeferMessages())
                .RegisterSingleton<IManageSubscriptions>(new ManageSubscriptions())
                .RegisterSingleton<IPublishMessages>(new PublishMessages())
                .RegisterSingleton<ISendMessages>(new SendMessages())
                .ConfigureComponent<DequeueMessages>(DependencyLifecycle.InstancePerCall);
        }

        protected override string ExampleConnectionStringForErrorMessage
        {
            get
            {
                return "InMemory";
            }
        }
    }
}
