using System;
using NServiceBus.AcceptanceTesting;
using NServiceBus.InMemory.Tests.Alpha.Messages.Commands;
using NServiceBus.InMemory.Tests.Alpha.Messages.Events;
using NServiceBus.InMemory.Tests.Helpers;
using NUnit.Framework;
using TestContext = NServiceBus.InMemory.Tests.Helpers.TestContext;

namespace NServiceBus.InMemory.Tests
{
    [TestFixture]
    public class SagaTests
    {
        public class SagaTestsContext : TestContext
        {
            public Guid SagaId { get; set; }
            
            public bool StartSagaCommandHandled { get; set; }
            public bool StartSagaCommandSent { get; set; }

            public bool SagaTimeoutHandled { get; set; }

            public bool StopSagaCommandHandled { get; set; }

            public bool SagaStoppedEventHandled { get; set; }

            public override bool TestComplete
            {
                get
                {
                    return StartSagaCommandHandled && SagaTimeoutHandled && StopSagaCommandHandled && SagaStoppedEventHandled;
                }
            }
        }

        [Test]
        public void TestSaga()
        {
            var context = new SagaTestsContext();

            using (context.InMemoryDatabase)
                Assert.IsTrue(Scenario.Define(context)
                    .WithEndpoint<AlphaServer>(behavior => behavior
                        .When(ctx => ctx.EndpointsStarted && !ctx.StartSagaCommandSent, (bus, ctx) =>
                        {
                            ctx.StartSagaCommandSent = true;
                            ctx.Events.MessageHandled += args =>
                            {
                                var startSagaCommand = args.Message as StartAlphaSaga;
                                if (startSagaCommand != null)
                                {
                                    Assert.AreEqual(ctx.SagaId, startSagaCommand.SagaId, "The start saga command sent does not match the test saga.");
                                    ctx.StartSagaCommandHandled = true;
                                }

                                var alphaSagaTimeout = args.Message as AlphaSagaTimeout;
                                if (alphaSagaTimeout != null)
                                {
                                    Assert.AreEqual(ctx.SagaId, alphaSagaTimeout.SagaId, "The saga timeout does not match test saga.");
                                    ctx.SagaTimeoutHandled = true;
                                    bus.Send(new StopAlphaSaga
                                    {
                                        SagaId = ctx.SagaId
                                    });
                                }

                                var stopSagaCommand = args.Message as StopAlphaSaga;
                                if (stopSagaCommand != null && ctx.SagaTimeoutHandled)
                                {
                                    Assert.AreEqual(ctx.SagaId, stopSagaCommand.SagaId, "The stop saga command sent does not match the test saga.");
                                    ctx.StopSagaCommandHandled = true;
                                }

                                var stopSagaEvent = args.Message as AlphaSagaCompleted;
                                if (stopSagaEvent != null && ctx.StopSagaCommandHandled)
                                {
                                    Assert.AreEqual(ctx.SagaId, stopSagaEvent.SagaId, "The saga stopped event does not match the test saga.");
                                    Assert.IsTrue(stopSagaEvent.TimedOutOn - stopSagaEvent.CreatedOn > TimeSpan.FromSeconds(14), "The saga did not actually do a delay for the timeout.");
                                    ctx.SagaStoppedEventHandled = true;
                                }
                            };
                            bus.Send(new StartAlphaSaga
                            {
                                SagaId = ctx.SagaId = Guid.NewGuid(),
                                TimeoutInSeconds = 15
                            });
                        }))
                .Done(ctx => ctx.TestComplete)
                .Run(TimeSpan.FromMinutes(5)).TestComplete, 
                "The test did not complete");
        }
    }
}
