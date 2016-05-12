using NServiceBus.InMemory.Tests.Alpha.Messages.Commands;
using NServiceBus.InMemory.Tests.Beta.Messages.Commands;
using NServiceBus.Logging;

namespace NServiceBus.InMemory.Tests.Alpha.Handlers.Commands
{
    public class SendCommandToBetaHandler : IHandleMessages<SendCommandToBeta>
    {
        public IBus Bus { get; set; }
        public readonly ILog Log = LogManager.GetLogger<SendCommandToBetaHandler>();
        public void Handle(SendCommandToBeta command)
        {
            Log.Info("Alpha.IHandleMessages<SendCommandToBeta>");

            Bus.Send(new BetaCommand
            {
                Id = command.Id
            });
        }
    }
}
