using System;

namespace NServiceBus.InMemory.Tests.Alpha.Messages.Commands
{
    public class AlphaSagaTimeout : ICommand
    {
        public Guid SagaId { get; set; }
    }
}
