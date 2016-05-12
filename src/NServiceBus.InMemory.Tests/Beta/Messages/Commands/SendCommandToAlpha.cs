using System;

namespace NServiceBus.InMemory.Tests.Beta.Messages.Commands
{
    public class SendCommandToAlpha : ICommand
    {
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}
