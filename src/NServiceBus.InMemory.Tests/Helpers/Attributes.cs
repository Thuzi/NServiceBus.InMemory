using System;

namespace NServiceBus.InMemory.Tests.Helpers
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DataBusAttribute : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class EncryptedAttribute : Attribute
    {
    }
}
