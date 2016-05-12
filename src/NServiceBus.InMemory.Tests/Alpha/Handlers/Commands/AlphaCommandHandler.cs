using NServiceBus.InMemory.Tests.Alpha.Messages.Commands;
using NServiceBus.InMemory.Tests.Alpha.Messages.Events;
using NServiceBus.Logging;

namespace NServiceBus.InMemory.Tests.Alpha.Handlers.Commands
{
    public class AlphaCommandHandler : IHandleMessages<AlphaCommand>
    {
        public IBus Bus { get; set; }
        public readonly ILog Log = LogManager.GetLogger<AlphaCommandHandler>();
        public void Handle(AlphaCommand command)
        {
            Log.Info("Alpha.IHandleMessages<AlphaCommand>");

            Bus.Publish(new CommandProcessedInAlpha
            {
                CreatedOn = command.CreatedOn,
                Id = command.Id
            });
        }
    }
}
