using NServiceBus.AcceptanceTesting;

namespace NServiceBus.InMemory.Tests.Helpers
{
    public abstract class TestContext : ScenarioContext
    {
        public abstract bool TestComplete { get; }
        public readonly BusEvents Events = new BusEvents();
        public readonly InMemoryDatabase InMemoryDatabase = new InMemoryDatabase();
    }
}
