using System;
using NServiceBus.Saga;

namespace NServiceBus.InMemory.Tests.Alpha.Messages.Sagas
{
    public class TestSagaData : IContainSagaData
    {
        public Guid Id { get; set; }
        public string Originator { get; set; }
        public string OriginalMessageId { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime? TimedOutOn { get; set; }

        [Unique]
        public Guid SagaId { get; set; }
    }
}
