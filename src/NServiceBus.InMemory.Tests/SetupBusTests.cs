using System;
using NServiceBus.AcceptanceTesting;
using NServiceBus.InMemory.Tests.Helpers;
using NUnit.Framework;
using TestContext = NServiceBus.InMemory.Tests.Helpers.TestContext;

namespace NServiceBus.InMemory.Tests
{
    [TestFixture]
    public class SetupBusTests
    {
        public class SetupBusTestsContext : TestContext
        {
            public bool AlphaServerStarted { get; set; }
            public bool BetaServerStarted { get; set; }

            public override bool TestComplete
            {
                get
                {
                    return AlphaServerStarted && BetaServerStarted;
                }
            }
        }
        [Test]
        public void SetupBus()
        {
            var context = new SetupBusTestsContext();

            using (context.InMemoryDatabase)
                Assert.IsTrue(Scenario.Define(context)
                    .WithEndpoint<AlphaServer>(behavior => behavior
                        .When(ctx => ctx.EndpointsStarted, (bus, ctx) => ctx.AlphaServerStarted = true))
                    .WithEndpoint<BetaServer>(behavior => behavior
                        .When(ctx => ctx.EndpointsStarted, (bus, ctx) => ctx.BetaServerStarted = true))
                    .Done(ctx => ctx.TestComplete)
                    .Run(TimeSpan.FromMinutes(5)).TestComplete,
                "The test did not complete");
        }
    }
}
