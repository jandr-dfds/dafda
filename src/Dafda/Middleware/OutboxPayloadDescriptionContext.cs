using Dafda.Serializing;

namespace Dafda.Middleware
{
    internal class OutboxPayloadDescriptionContext : IMiddlewareContext
    {
        public OutboxPayloadDescriptionContext(PayloadDescriptor[] payloadDescriptors)
        {
            PayloadDescriptors = payloadDescriptors;
        }

        public PayloadDescriptor[] PayloadDescriptors { get; }
    }
}