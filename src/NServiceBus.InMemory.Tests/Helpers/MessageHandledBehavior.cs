using System;
using NServiceBus.Pipeline;
using NServiceBus.Pipeline.Contexts;

namespace NServiceBus.InMemory.Tests.Helpers
{
    public class MessageHandledBehavior : IBehavior<IncomingContext>
    {
        public BusEvents Events { get; set; }
        public string EndpointName { get; set; }
        public void Invoke(IncomingContext context, Action next)
        {
            Exception exception = null;

            try
            {
                next();
            }
            catch (Exception error)
            {
                exception = error;
                throw;
            }
            finally
            {
                Events.InvokeMessageHandled(new MessageHandledEventArgs
                {
                    EndpointName = EndpointName,
                    Exception = exception,
                    HandlerType = context.MessageHandler.Instance.GetType(),
                    Message = context.IncomingLogicalMessage.Instance,
                    MessageType = context.IncomingLogicalMessage.Instance.GetType()
                });
            }
        }
    }
    public class RegisterMessageHandledStep : RegisterStep
    {
        public RegisterMessageHandledStep()
            : base("MessageHandledBehavior", typeof(MessageHandledBehavior), "So we can know when a message has been processed.")
        {
            InsertBefore(WellKnownStep.InvokeHandlers);
        }
    }
}
