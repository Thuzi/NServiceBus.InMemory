using NServiceBus.InMemory.Tests.Beta.Messages.Events;
using NServiceBus.Logging;

namespace NServiceBus.InMemory.Tests.Beta.Handlers.Events
{
    public class BetaEventHandler : IHandleMessages<CommandProcessedInBeta>
    {
        public readonly ILog Log = LogManager.GetLogger<BetaEventHandler>();
        public void Handle(CommandProcessedInBeta message)
        {
            Log.Info("Beta.IHandleMessages<CommandProcessedInBeta>");
        }
    }
}
