using System;
using NServiceBus.ObjectBuilder;

namespace NServiceBus.InMemory.Tests.Helpers
{
    public class BusEvents
    {
        public event Action<IConfigureComponents> ConfigContainer;
        public event Action<MessageHandledEventArgs> MessageHandled;

        public void InvokeConfigContainer(IConfigureComponents container)
        {
            var configContainer = ConfigContainer;
            if (configContainer != null) configContainer(container);
        }
        public void InvokeMessageHandled(MessageHandledEventArgs args)
        {
            var messageHandled = MessageHandled;
            if (messageHandled != null) messageHandled(args);
        }
    }
    public class MessageHandledEventArgs
    {
        public Exception Exception { get; set; }
        public Type HandlerType { get; set; }
        public Type MessageType { get; set; }
        public object Message { get; set; }
        public string EndpointName { get; set; }
    }
}
