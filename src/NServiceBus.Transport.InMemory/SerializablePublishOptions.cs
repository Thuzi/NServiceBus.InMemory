using System;
using System.Runtime.Serialization;
using NServiceBus.Unicast;

namespace NServiceBus.Transport.InMemory
{
    [Serializable]
    public class SerializablePublishOptions : DeliveryOptions, ISerializable
    {
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(EnforceMessagingBestPractices), EnforceMessagingBestPractices);
            info.AddValue(nameof(EnlistInReceiveTransaction), EnlistInReceiveTransaction);
            info.AddValue(nameof(EventType), EventType);
            info.AddValue(nameof(ReplyToAddress), ReplyToAddress == null ? null : new Tuple<string, string>(ReplyToAddress.Queue, ReplyToAddress.Machine), typeof(Tuple<string, string>));
        }

        public SerializablePublishOptions(PublishOptions publishOptions)
        {
            EnforceMessagingBestPractices = publishOptions.EnforceMessagingBestPractices;
            EnlistInReceiveTransaction = publishOptions.EnlistInReceiveTransaction;
            EventType = publishOptions.EventType.AssemblyQualifiedName;
            ReplyToAddress = publishOptions.ReplyToAddress;
        }
        public SerializablePublishOptions(SerializationInfo info, StreamingContext context)
        {
            EnforceMessagingBestPractices = info.GetBoolean(nameof(EnforceMessagingBestPractices));
            EnlistInReceiveTransaction = info.GetBoolean(nameof(EnlistInReceiveTransaction));
            EventType = info.GetString(nameof(EventType));
            var replyToAddress = (Tuple<string, string>)info.GetValue(nameof(ReplyToAddress), typeof(Tuple<string, string>));
            ReplyToAddress = replyToAddress == null ? null : new Address(replyToAddress.Item1, replyToAddress.Item2);
        }
        /// <summary>
        /// The type of event to publish
        /// </summary>
        public string EventType { get; }
    }
}
