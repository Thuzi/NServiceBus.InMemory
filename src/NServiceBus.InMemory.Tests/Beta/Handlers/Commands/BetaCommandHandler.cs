using NServiceBus.InMemory.Tests.Beta.Messages.Commands;
using NServiceBus.InMemory.Tests.Beta.Messages.Events;
using NServiceBus.Logging;

namespace NServiceBus.InMemory.Tests.Beta.Handlers.Commands
{
    public class BetaCommandHandler : IHandleMessages<BetaCommand>
    {
        public IBus Bus { get; set; }
        public readonly ILog Log = LogManager.GetLogger<BetaCommandHandler>();
        public void Handle(BetaCommand command)
        {
            Log.Info("Beta.IHandleMessages<BetaCommand>");

            Bus.Publish(new CommandProcessedInBeta
            {
                CreatedOn = command.CreatedOn,
                Id = command.Id
            });
        }
    }
}
