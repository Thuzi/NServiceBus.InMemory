using NServiceBus.InMemory.Tests.Alpha.Messages.Events;
using NServiceBus.Logging;

namespace NServiceBus.InMemory.Tests.Beta.Handlers.Events
{
    public class AlphaEventHandler : IHandleMessages<CommandProcessedInAlpha>
    {
        public readonly ILog Log = LogManager.GetLogger<AlphaEventHandler>();
        public void Handle(CommandProcessedInAlpha message)
        {
            Log.Info("Beta.IHandleMessages<CommandProcessedInAlpha>");
        }
    }
}
