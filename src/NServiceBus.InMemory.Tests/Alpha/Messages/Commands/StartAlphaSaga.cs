using System;

namespace NServiceBus.InMemory.Tests.Alpha.Messages.Commands
{
    public class StartAlphaSaga : ICommand
    {
        public Guid SagaId { get; set; } = Guid.NewGuid();
        public int TimeoutInSeconds { get; set; } = 5;
    }
}
