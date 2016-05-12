using System;

namespace NServiceBus.InMemory.Tests.Alpha.Messages.Commands
{
    public class AlphaCommand : ICommand
    {
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}
