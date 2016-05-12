using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;

namespace NServiceBus.InMemory.Tests.Helpers
{
    public class ProvideConfiguration : IProvideConfiguration<UnicastBusConfig>
    {
        public UnicastBusConfig GetConfiguration()
        {
            return new UnicastBusConfig
            {
                MessageEndpointMappings = new MessageEndpointMappingCollection
                {
                    new MessageEndpointMapping
                    {
				        AssemblyName = "NServiceBus.InMemory.Tests",
				        Namespace = "NServiceBus.InMemory.Tests.Alpha.Messages.Commands",
				        Endpoint = AlphaServer.EndpointName
                    },
                    new MessageEndpointMapping
                    {
				        AssemblyName = "NServiceBus.InMemory.Tests",
				        Namespace = "NServiceBus.InMemory.Tests.Alpha.Messages.Events",
				        Endpoint = AlphaServer.EndpointName
                    },
                    new MessageEndpointMapping
                    {
				        AssemblyName = "NServiceBus.InMemory.Tests",
				        Namespace = "NServiceBus.InMemory.Tests.Alpha.Messages.Messages",
				        Endpoint = AlphaServer.EndpointName
                    },
                    new MessageEndpointMapping
                    {
				        AssemblyName = "NServiceBus.InMemory.Tests",
				        Namespace = "NServiceBus.InMemory.Tests.Beta.Messages.Commands",
				        Endpoint = BetaServer.EndpointName
                    },
                    new MessageEndpointMapping
                    {
				        AssemblyName = "NServiceBus.InMemory.Tests",
				        Namespace = "NServiceBus.InMemory.Tests.Beta.Messages.Events",
				        Endpoint = BetaServer.EndpointName
                    },
                    new MessageEndpointMapping
                    {
				        AssemblyName = "NServiceBus.InMemory.Tests",
				        Namespace = "NServiceBus.InMemory.Tests.Beta.Messages.Messages",
				        Endpoint = BetaServer.EndpointName
                    }
                }
            };
        }
    }
}
