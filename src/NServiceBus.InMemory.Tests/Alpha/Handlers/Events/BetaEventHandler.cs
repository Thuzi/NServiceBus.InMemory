using NServiceBus.InMemory.Tests.Beta.Messages.Events;
using NServiceBus.Logging;

namespace NServiceBus.InMemory.Tests.Alpha.Handlers.Events
{
    public class BetaEventHandler : IHandleMessages<CommandProcessedInBeta>
    {
        public readonly ILog Log = LogManager.GetLogger<BetaEventHandler>();
        public void Handle(CommandProcessedInBeta message)
        {
            Log.Info("Alpha.IHandleMessages<CommandProcessedInBeta>");
        }
    }
}
