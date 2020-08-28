﻿using NServiceBus.Settings;

namespace NServiceBus.Transport.InMemory
{
    public class InMemoryTransport : TransportDefinition
    {
        public override TransportInfrastructure Initialize(SettingsHolder settings, string connectionString)
        {
            return new InMemoryTransportInfrastructure();
        }

        public override string ExampleConnectionStringForErrorMessage { get; } = "";
    }
}
