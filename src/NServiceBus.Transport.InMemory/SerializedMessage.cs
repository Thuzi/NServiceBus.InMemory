namespace NServiceBus.Transport.InMemory
{
    public class SerializedMessage
    {
        public SerializedMessage(string headers, byte[] body)
        {
            Headers = headers;
            Body = body;
        }

        public string Headers { get; }
        public byte[] Body { get; }
    }
}