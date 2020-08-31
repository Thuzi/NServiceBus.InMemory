using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Settings;
using NServiceBus.Transport.InMemory;
using NServiceBus.TransportTests;

public class ConfigureInMemoryTransportInfrastructure : IConfigureTransportInfrastructure
{
    public TransportConfigurationResult Configure(SettingsHolder settings, TransportTransactionMode transactionMode)
    {
        return new TransportConfigurationResult
        {
            PurgeInputQueueOnStartup = true,
            TransportInfrastructure = new InMemoryTransportInfrastructure()
        };
    }

    public Task Cleanup()
    {
        return Task.CompletedTask;
    }
}