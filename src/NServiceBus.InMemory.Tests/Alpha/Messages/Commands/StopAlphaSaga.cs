using System;

namespace NServiceBus.InMemory.Tests.Alpha.Messages.Commands
{
    public class StopAlphaSaga : ICommand
    {
        public Guid SagaId { get; set; }
    }
}
