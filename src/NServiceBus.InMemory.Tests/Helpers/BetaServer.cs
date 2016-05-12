using NServiceBus.AcceptanceTesting;

namespace NServiceBus.InMemory.Tests.Helpers
{
    public class BetaServer : EndpointConfigurationBuilder
    {
        public const string EndpointName = "Tests.Beta";
        public BetaServer()
        {
            CustomEndpointName(EndpointName).EndpointSetup<GenericEndpointServer>();
        }
    }
}
