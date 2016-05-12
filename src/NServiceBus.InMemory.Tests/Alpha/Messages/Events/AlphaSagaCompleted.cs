using System;

namespace NServiceBus.InMemory.Tests.Alpha.Messages.Events
{
    public class AlphaSagaCompleted : IEvent
    {
        public DateTime CreatedOn { get; set; }
        public DateTime? TimedOutOn { get; set; }
        public Guid SagaId { get; set; }
    }
}
