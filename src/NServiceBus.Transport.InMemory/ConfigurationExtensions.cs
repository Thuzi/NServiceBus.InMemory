using NServiceBus.Configuration.AdvancedExtensibility;

namespace NServiceBus.Transport.InMemory
{
    public static class ConfigurationExtensions
    {
        public static TransportExtensions<InMemoryTransport> UseDatabase(this TransportExtensions<InMemoryTransport> extensions, InMemoryDatabase database)
        {
            extensions.GetSettings().Set(database);

            return extensions;
        }
    }
}