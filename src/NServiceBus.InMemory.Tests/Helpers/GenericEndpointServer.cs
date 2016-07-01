using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NServiceBus.AcceptanceTesting.Support;
using NServiceBus.Config.ConfigurationSource;
using NServiceBus.Features;
using NServiceBus.Persistence;

namespace NServiceBus.InMemory.Tests.Helpers
{
    public class GenericEndpointServer : IEndpointSetupTemplate
    {
        public BusConfiguration GetConfiguration(RunDescriptor runDescriptor,
            EndpointConfiguration endpointConfiguration,
            IConfigurationSource configSource,
            Action<BusConfiguration> configurationBuilderCustomization)
        {
            var config = new BusConfiguration();

            var licensePath = Path.Combine(
                Path.GetDirectoryName(
                Path.GetDirectoryName(
                Path.GetDirectoryName(
                Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location))))), "License.xml");
            if (File.Exists(licensePath))
            {
                config.License(File.ReadAllText(licensePath));
            }

            var context = runDescriptor.ScenarioContext as TestContext;
            if (context == null) throw new InvalidOperationException("The test context is not a TestContext type.");

            var busEvents = context.Events;
            
            config.RegisterComponents(configureComponents =>
            {
                configureComponents.ConfigureComponent<MessageHandledBehavior>(DependencyLifecycle.SingleInstance)
                    .ConfigureProperty(service => service.Events, busEvents)
                    .ConfigureProperty(service => service.EndpointName, endpointConfiguration.EndpointName);
                configureComponents
                    .RegisterSingleton(busEvents)
                    .RegisterSingleton(context.InMemoryDatabase);

                busEvents.InvokeConfigContainer(configureComponents);
            });

            config.CustomConfigurationSource(configSource);

            config.UsePersistence<InMemoryPersistence, StorageType.Sagas>();
            config.UsePersistence<InMemoryPersistence, StorageType.Subscriptions>();
            config.UsePersistence<InMemoryPersistence, StorageType.Timeouts>();
            config.UseTransport<InMemoryTransport>().ConnectionString("InMemory");

            config.EndpointName(endpointConfiguration.EndpointName);
            config.EnableInstallers();
            config.EnableFeature<TimeoutManager>();
            
            var handlerIgnoreNamespace = endpointConfiguration.EndpointName.IndexOf("alpha", StringComparison.OrdinalIgnoreCase) == -1 ?
                "NServiceBus.InMemory.Tests.Alpha.Handlers." :
                "NServiceBus.InMemory.Tests.Beta.Handlers.";

            config.TypesToScan(AllAssemblies.Matching("NServiceBus.")
                .SelectMany(assembly => assembly.DefinedTypes)
                .Where(type => string.IsNullOrEmpty(type.Namespace) || !type.Namespace.StartsWith(handlerIgnoreNamespace)));

            config.Conventions()
                .DefiningCommandsAs(type => type.GetInterfaces().Contains(typeof(ICommand)))
                .DefiningDataBusPropertiesAs(property => Attribute.IsDefined(property, typeof (DataBusAttribute)))
                .DefiningEncryptedPropertiesAs(property => Attribute.IsDefined(property, typeof (EncryptedAttribute)))
                .DefiningEventsAs(type => type.GetInterfaces().Contains(typeof (IEvent)))
                .DefiningExpressMessagesAs(type => Attribute.IsDefined(type, typeof(ExpressAttribute)))
                .DefiningMessagesAs(type => type.GetInterfaces().Contains(typeof (IMessage)))
                .DefiningTimeToBeReceivedAs(type =>
                {
                    var att = type.GetCustomAttribute<TimeToBeReceivedAttribute>();
                    return att == null ? TimeSpan.MaxValue : att.TimeToBeReceived;
                });

            config.Pipeline.Register<RegisterMessageHandledStep>();

            if (configurationBuilderCustomization != null)
            {
                configurationBuilderCustomization(config);
            }

            return config;
        }
    }
}
