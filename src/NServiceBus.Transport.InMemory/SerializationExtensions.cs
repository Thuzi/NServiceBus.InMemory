﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace NServiceBus.Transport.InMemory
{
    public static class SerializationExtensions
    {
        public static SerializedMessage Serialize(this OutgoingMessage message)
        {
            var headers = JsonConvert.SerializeObject(message.Headers);
            var body = message.Body;

            return new SerializedMessage(headers, body);
        }

        public static (Dictionary<string, string> headers, byte[] body) Deserialize(this SerializedMessage message)
        {
            var headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(message.Headers);
            var body = message.Body;

            return (headers, body);
        }
    }
}