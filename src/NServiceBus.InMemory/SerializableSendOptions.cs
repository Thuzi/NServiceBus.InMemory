using System;
using System.Runtime.Serialization;
using NServiceBus.Unicast;

namespace NServiceBus.InMemory
{
    [Serializable]
    public class SerializableSendOptions : SendOptions, ISerializable
    {
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Destination), Destination.ToString());
            info.AddValue(nameof(EnforceMessagingBestPractices), EnforceMessagingBestPractices);
            info.AddValue(nameof(EnlistInReceiveTransaction), EnlistInReceiveTransaction);
            info.AddValue(nameof(ReplyToAddress), ReplyToAddress == null ? null : new Tuple<string, string>(ReplyToAddress.Queue, ReplyToAddress.Machine), typeof(Tuple<string, string>));
            info.AddValue(nameof(CorrelationId), CorrelationId, typeof(string));
            info.AddValue(nameof(DeliverAt), DeliverAt, typeof(DateTime?));
            info.AddValue(nameof(DelayDeliveryWith), DelayDeliveryWith, typeof(TimeSpan?));
            info.AddValue(nameof(TimeToBeReceived), TimeToBeReceived, typeof(TimeSpan?));
        }

        public SerializableSendOptions(SendOptions sendOptions)
            : base(sendOptions.Destination)
        {
            EnforceMessagingBestPractices = sendOptions.EnforceMessagingBestPractices;
            EnlistInReceiveTransaction = sendOptions.EnlistInReceiveTransaction;
            ReplyToAddress = sendOptions.ReplyToAddress;
            CorrelationId = sendOptions.CorrelationId;
            DeliverAt = sendOptions.DeliverAt;
            DelayDeliveryWith = sendOptions.DelayDeliveryWith;
            TimeToBeReceived = sendOptions.TimeToBeReceived;
        }

        public SerializableSendOptions(SerializationInfo info, StreamingContext context)
            : base(info.GetString(nameof(Destination)))
        {
            EnforceMessagingBestPractices = info.GetBoolean(nameof(EnforceMessagingBestPractices));
            EnlistInReceiveTransaction = info.GetBoolean(nameof(EnlistInReceiveTransaction));
            var replyToAddress = (Tuple<string, string>)info.GetValue(nameof(ReplyToAddress), typeof(Tuple<string, string>));
            ReplyToAddress = replyToAddress == null ? null : new Address(replyToAddress.Item1, replyToAddress.Item2);
            CorrelationId = info.GetString(nameof(CorrelationId));
            DeliverAt = (DateTime?)info.GetValue(nameof(DeliverAt), typeof(DateTime?));
            DelayDeliveryWith = (TimeSpan?)info.GetValue(nameof(DelayDeliveryWith), typeof(TimeSpan?));
            TimeToBeReceived = (TimeSpan?)info.GetValue(nameof(TimeToBeReceived), typeof(TimeSpan?));
        }
    }
}
