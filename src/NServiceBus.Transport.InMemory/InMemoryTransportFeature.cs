using NServiceBus.Features;
using NServiceBus.Settings;

namespace NServiceBus.Transport.InMemory
{
    //public class InMemoryTransportFeature : ConfigureTransport
    //{
    //    protected override string GetLocalAddress(ReadOnlySettings settings)
    //    {
    //        return Address.Parse(settings.Get<string>("NServiceBus.LocalAddress")).Queue;
    //    }

    //    protected override void Configure(FeatureConfigurationContext context, string connectionString)
    //    {
    //        context.Container.RegisterSingleton(new EndpointInfo
    //        {
    //            Name = GetLocalAddress(context.Settings)
    //        });

    //        context.Container.ConfigureComponent<InMemoryQueueCreator>(DependencyLifecycle.SingleInstance);
    //        context.Container.ConfigureComponent<InMemoryMessageDispatcher>(DependencyLifecycle.SingleInstance);
    //        context.Container.ConfigureComponent<InMemoryMessagePump>(DependencyLifecycle.InstancePerCall);
    //        context.Container.ConfigureComponent<SubscriptionManager>(DependencyLifecycle.SingleInstance);
    //        context.Container.ConfigureComponent<PublishMessages>(DependencyLifecycle.SingleInstance);
    //        context.Container.ConfigureComponent<SendMessages>(DependencyLifecycle.SingleInstance);
    //        context.Container.ConfigureComponent<RunWhenBusStartsAndStops>(DependencyLifecycle.SingleInstance);
    //    }

    //    protected override string ExampleConnectionStringForErrorMessage => "InMemory";
    //}
}
