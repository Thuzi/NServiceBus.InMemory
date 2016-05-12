using NServiceBus.InMemory.Tests.Alpha.Messages.Events;
using NServiceBus.Logging;

namespace NServiceBus.InMemory.Tests.Alpha.Handlers.Events
{
    public class AlphaSagaCompletedHandler : IHandleMessages<AlphaSagaCompleted>
    {
        public readonly ILog Log = LogManager.GetLogger<AlphaEventHandler>();
        public void Handle(AlphaSagaCompleted message)
        {
            Log.Info("Alpha.IHandleMessages<AlphaEventHandler>");
        }
    }
}
