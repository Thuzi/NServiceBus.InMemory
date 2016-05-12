using System;
using NServiceBus.InMemory.Tests.Alpha.Messages.Commands;
using NServiceBus.InMemory.Tests.Alpha.Messages.Events;
using NServiceBus.InMemory.Tests.Alpha.Messages.Sagas;
using NServiceBus.Saga;

namespace NServiceBus.InMemory.Tests.Alpha.Handlers.Sagas
{
    public class TestSaga : Saga<TestSagaData>, IAmStartedByMessages<StartAlphaSaga>, IHandleTimeouts<AlphaSagaTimeout>, IHandleMessages<StopAlphaSaga>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<TestSagaData> mapper)
        {
            mapper.ConfigureMapping<StartAlphaSaga>(model => model.SagaId)
                .ToSaga(model => model.SagaId);
            mapper.ConfigureMapping<StopAlphaSaga>(model => model.SagaId)
                .ToSaga(model => model.SagaId);
            mapper.ConfigureMapping<AlphaSagaTimeout>(model => model.SagaId)
                .ToSaga(model => model.SagaId);
        }
        
        public void Handle(StartAlphaSaga command)
        {
            Data.CreatedOn = DateTime.UtcNow;
            Data.SagaId = command.SagaId;

            RequestTimeout(DateTime.UtcNow.AddSeconds(command.TimeoutInSeconds), new AlphaSagaTimeout
            {
                SagaId = Data.SagaId
            });
        }
        public void Timeout(AlphaSagaTimeout command)
        {
            Data.TimedOutOn = DateTime.UtcNow;
        }
        public void Handle(StopAlphaSaga command)
        {
            Bus.Publish(new AlphaSagaCompleted
            {
                CreatedOn = Data.CreatedOn,
                TimedOutOn = Data.TimedOutOn,
                SagaId = Data.SagaId
            });
            MarkAsComplete();
        }
    }
}
