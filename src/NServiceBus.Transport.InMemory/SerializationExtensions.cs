using System.Collections.Generic;
using Newtonsoft.Json;

namespace NServiceBus.Transport.InMemory
{
    public static class SerializationExtensions
    {
        public static SerializedMessage Serialize(this OutgoingMessage message)
        {
            var headers = JsonConvert.SerializeObject(message.Headers);
            var body = message.Body;

            return new SerializedMessage(message.MessageId, headers, body);
        }

        public static (string messagEId, Dictionary<string, string> headers, byte[] body) Deserialize(this SerializedMessage message)
        {
            var messageId = message.MessageId;
            var headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(message.Headers);
            var body = message.Body;

            return (messageId, headers, body);
        }
    }
}