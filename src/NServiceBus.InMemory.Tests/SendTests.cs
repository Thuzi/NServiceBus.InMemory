using System;
using NServiceBus.AcceptanceTesting;
using NServiceBus.InMemory.Tests.Alpha.Messages.Commands;
using NServiceBus.InMemory.Tests.Alpha.Messages.Events;
using NServiceBus.InMemory.Tests.Beta.Messages.Commands;
using NServiceBus.InMemory.Tests.Beta.Messages.Events;
using NServiceBus.InMemory.Tests.Helpers;
using NUnit.Framework;
using TestContext = NServiceBus.InMemory.Tests.Helpers.TestContext;

namespace NServiceBus.InMemory.Tests
{
    [TestFixture]
    public class SendTests
    {
        public class SendTestsContext : TestContext
        {
            public Guid CommandId { get; set; }
            public bool CommandHandledEventPublished { get; set; }
            public bool SendCommandHandled { get; set; }
            public bool SendCommandSent { get; set; }

            public bool BetaCommandHandled { get; set; }

            public override bool TestComplete
            {
                get
                {
                    return CommandHandledEventPublished && SendCommandHandled && SendCommandSent && BetaCommandHandled;
                }
            }
        }
        [Test]
        public void SendingACommandAndWaitingForAnEventToBePublished()
        {
            Assert.IsTrue(Scenario.Define(new SendTestsContext
            {
                BetaCommandHandled = true
            })
            .WithEndpoint<AlphaServer>(behavior =>
            {
                behavior.When(ctx => ctx.EndpointsStarted && !ctx.SendCommandSent, (bus, ctx) =>
                {
                    ctx.SendCommandSent = true;
                    ctx.Events.MessageHandled += args =>
                    {
                        var command = args.Message as AlphaCommand;
                        if (command != null)
                        {
                            Assert.AreEqual(ctx.CommandId, command.Id, "The command sent does not match the command handled.");
                            ctx.SendCommandHandled = true;
                        }
                        var commandHandledEvent = args.Message as CommandProcessedInAlpha;
                        if (commandHandledEvent != null)
                        {
                            Assert.AreEqual(ctx.CommandId, commandHandledEvent.Id, "The event published does not match the command sent.");
                            ctx.CommandHandledEventPublished = true;
                        }
                    };
                    bus.Send(new AlphaCommand
                    {
                        Id = ctx.CommandId = Guid.NewGuid()
                    });
                });
            })
            .Done(ctx => ctx.TestComplete)
            .Run(TimeSpan.FromMinutes(5)).TestComplete,
            "The test did not complete");
        }
        [Test]
        public void DeferingACommandAndWaitingForAnEventToBePublished()
        {
            Assert.IsTrue(Scenario.Define(new SendTestsContext
            {
                BetaCommandHandled = true
            })
            .WithEndpoint<AlphaServer>(behavior => behavior
                .When(ctx => ctx.EndpointsStarted && !ctx.SendCommandSent, (bus, ctx) =>
                {
                    ctx.SendCommandSent = true;
                    ctx.Events.MessageHandled += args =>
                    {
                        var command = args.Message as AlphaCommand;
                        if (command != null)
                        {
                            Assert.AreEqual(ctx.CommandId, command.Id, "The command sent does not match the command handled.");
                            ctx.SendCommandHandled = true;
                        }
                        var commandHandledEvent = args.Message as CommandProcessedInAlpha;
                        if (commandHandledEvent != null)
                        {
                            Assert.AreEqual(ctx.CommandId, commandHandledEvent.Id, "The event published does not match the command sent.");
                            Assert.IsTrue(commandHandledEvent.ProcessedOn - commandHandledEvent.CreatedOn > TimeSpan.FromSeconds(14), "The command was not defered.");
                            ctx.CommandHandledEventPublished = true;
                        }
                    };
                    bus.Defer(DateTime.UtcNow.AddSeconds(15), new AlphaCommand
                    {
                        Id = ctx.CommandId = Guid.NewGuid()
                    });
                }))
            .Done(ctx => ctx.TestComplete)
            .Run(TimeSpan.FromMinutes(5)).TestComplete,
            "The test did not complete");
        }
        [Test]
        public void SendCommandFromOneEndpointToAnother()
        {
            Assert.IsTrue(Scenario.Define(new SendTestsContext())
            .WithEndpoint<AlphaServer>(behavior => behavior
                .When(ctx => ctx.EndpointsStarted && !ctx.SendCommandSent, (bus, ctx) =>
                {
                    ctx.SendCommandSent = true;
                    ctx.Events.MessageHandled += args =>
                    {
                        var command = args.Message as SendCommandToBeta;
                        if (command != null)
                        {
                            Assert.AreEqual(ctx.CommandId, command.Id, "The command sent does not match the command handled.");
                            ctx.SendCommandHandled = true;
                        }
                    };
                    bus.Send(new SendCommandToBeta
                    {
                        Id = ctx.CommandId = Guid.NewGuid()
                    });
                }))
            .WithEndpoint<BetaServer>(behavior => behavior
                .When(ctx => ctx.EndpointsStarted, (bus, ctx) =>
                {
                    ctx.Events.MessageHandled += args =>
                    {
                        var command = args.Message as BetaCommand;
                        if (command != null)
                        {
                            Assert.AreEqual(ctx.CommandId, command.Id, "The command sent does not match the command handled.");
                            ctx.BetaCommandHandled = true;
                        }
                        var commandHandledEvent = args.Message as CommandProcessedInBeta;
                        if (commandHandledEvent != null)
                        {
                            Assert.AreEqual(ctx.CommandId, commandHandledEvent.Id, "The event published does not match the command sent.");
                            ctx.CommandHandledEventPublished = true;
                        }
                    };
                }))
            .Done(ctx => ctx.TestComplete)
            .Run(TimeSpan.FromMinutes(5)).TestComplete,
            "The test did not complete");
        }
    }
}
