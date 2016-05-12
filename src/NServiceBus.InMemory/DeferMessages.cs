using System.Linq;
using NServiceBus.Transports;
using NServiceBus.Unicast;

namespace NServiceBus.InMemory
{
    public class DeferMessages : IDeferMessages
    {
        public InMemoryDatabase InMemoryDatabase { get; set; }
        public void Defer(TransportMessage message, SendOptions sendOptions)
        {
            InMemoryDatabase.SendWithDelay(message, sendOptions);
        }
        public void ClearDeferredMessages(string headerKey, string headerValue)
        {
            var value = InMemoryDatabase.DelayedMessages.Values
                .FirstOrDefault(item =>
                    item.Item1.Headers.ContainsKey(headerKey) &&
                    item.Item1.Headers[headerKey] == headerValue);
            if (value != null)
            {
                InMemoryDatabase.DelayedMessages.TryRemove(value.Item1.Id, out value);
            }
        }
    }
}
