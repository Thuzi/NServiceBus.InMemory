using System;

namespace NServiceBus.InMemory.Tests.Alpha.Messages.Events
{
    public class CommandProcessedInAlpha : IEvent
    {
        public DateTime CreatedOn { get; set; }
        public DateTime ProcessedOn { get; set; } = DateTime.UtcNow;
        public Guid Id { get; set; }
    }
}
