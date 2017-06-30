using System;
using System.Linq;
using System.Runtime.Serialization;
using HeadersKeys = NServiceBus.Headers;

namespace NServiceBus.InMemory
{
    [Serializable]
    public class SerializableTransportMessage : TransportMessage, ISerializable
    {
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(ReplyToAddress), ReplyToAddress == null ? null : new Tuple<string, string>(ReplyToAddress.Queue, ReplyToAddress.Machine), typeof(Tuple<string, string>));
            info.AddValue(nameof(Body), Body, typeof(byte[]));
            info.AddValue(nameof(CorrelationId), CorrelationId);
            info.AddValue(nameof(Headers), Headers.Select(pair => new Tuple<string, string>(pair.Key, pair.Value)).ToArray());
            info.AddValue(nameof(Id), Id);
            info.AddValue(nameof(MessageIntent), (int)MessageIntent);
            info.AddValue(nameof(Recoverable), Recoverable);
            info.AddValue(nameof(TimeToBeReceived), TimeToBeReceived.Ticks);
        }

        public SerializableTransportMessage(SerializationInfo info, StreamingContext context)
            : base(info.GetString(nameof(Id)), ((Tuple<string, string>[])info
                  .GetValue(nameof(Headers), typeof(Tuple<string, string>[])))
                  .ToDictionary(pair => pair.Item1, pair => pair.Item2))
        {
            var replyToAddress = (Tuple<string, string>)info.GetValue(nameof(ReplyToAddress), typeof(Tuple<string, string>));
            if (replyToAddress != null)
            {
                Headers[HeadersKeys.ReplyToAddress] = new Address(replyToAddress.Item1, replyToAddress.Item2).ToString();
            }
            Body = (byte[])info.GetValue(nameof(Body), typeof(byte[]));
            CorrelationId = info.GetString(nameof(CorrelationId));
            MessageIntent = (MessageIntentEnum)info.GetInt32(nameof(MessageIntent));
            Recoverable = info.GetBoolean(nameof(Recoverable));
            TimeToBeReceived = TimeSpan.FromTicks(info.GetInt64(nameof(TimeToBeReceived)));
        }

        public SerializableTransportMessage(TransportMessage message)
            : base(message.Id, message.Headers)
        {
            if (message.ReplyToAddress != null)
            {
                Headers[HeadersKeys.ReplyToAddress] = message.ReplyToAddress.ToString();
            }
            Body = message.Body;
            CorrelationId = message.CorrelationId;
            MessageIntent = message.MessageIntent;
            Recoverable = message.Recoverable;
            TimeToBeReceived = message.TimeToBeReceived;
        }
    }
}
