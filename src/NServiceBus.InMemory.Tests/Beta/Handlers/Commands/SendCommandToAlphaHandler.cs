using NServiceBus.InMemory.Tests.Alpha.Messages.Commands;
using NServiceBus.InMemory.Tests.Beta.Messages.Commands;
using NServiceBus.Logging;

namespace NServiceBus.InMemory.Tests.Beta.Handlers.Commands
{
    public class SendCommandToAlphaHandler : IHandleMessages<SendCommandToAlpha>
    {
        public IBus Bus { get; set; }
        public readonly ILog Log = LogManager.GetLogger<SendCommandToAlphaHandler>();
        public void Handle(SendCommandToAlpha command)
        {
            Log.Info("Beta.IHandleMessages<SendCommandToAlpha>");

            Bus.Send(new AlphaCommand
            {
                Id = command.Id
            });
        }
    }
}
