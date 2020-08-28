namespace NServiceBus.Transport.InMemory
{
    public class SerializedMessage
    {
        public SerializedMessage(string messageId, string headers, byte[] body)
        {
            MessageId = messageId;
            Headers = headers;
            Body = body;
        }

        public string MessageId { get; }
        public string Headers { get; }
        public byte[] Body { get; }
    }
}