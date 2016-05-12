using System;

namespace NServiceBus.InMemory.Tests.Beta.Messages.Events
{
    public class CommandProcessedInBeta : IEvent
    {
        public DateTime CreatedOn { get; set; }
        public DateTime ProcessedOn { get; set; } = DateTime.UtcNow;
        public Guid Id { get; set; }
    }
}
