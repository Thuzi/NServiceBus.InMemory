using NServiceBus.AcceptanceTesting;

namespace NServiceBus.InMemory.Tests.Helpers
{
    public class AlphaServer : EndpointConfigurationBuilder
    {
        public const string EndpointName = "Tests.Alpha";
        public AlphaServer()
        {
            CustomEndpointName(EndpointName).EndpointSetup<GenericEndpointServer>();
        }
    }
}
