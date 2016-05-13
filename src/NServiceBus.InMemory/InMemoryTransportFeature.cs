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
            context.Container.RegisterSingleton(new EndpointInfo
            {
                Name = GetLocalAddress(context.Settings)
            });

            context.Container.ConfigureComponent<CreateQueues>(DependencyLifecycle.SingleInstance);
            context.Container.ConfigureComponent<DeferMessages>(DependencyLifecycle.SingleInstance);
            context.Container.ConfigureComponent<DequeueMessages>(DependencyLifecycle.InstancePerCall);
            context.Container.ConfigureComponent<ManageSubscriptions>(DependencyLifecycle.SingleInstance);
            context.Container.ConfigureComponent<PublishMessages>(DependencyLifecycle.SingleInstance);
            context.Container.ConfigureComponent<SendMessages>(DependencyLifecycle.SingleInstance);
            context.Container.ConfigureComponent<RunWhenBusStartsAndStops>(DependencyLifecycle.SingleInstance);
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
