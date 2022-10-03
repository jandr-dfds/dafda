using Dafda.Serializing;

namespace Dafda.Middleware
{
    internal class OutboxPayloadDescriptionContext : MiddlewareContext
    {
        public OutboxPayloadDescriptionContext(PayloadDescriptor[] payloadDescriptors, IMiddlewareContext parent)
            : base(parent)
        {
            PayloadDescriptors = payloadDescriptors;
        }

        public PayloadDescriptor[] PayloadDescriptors { get; }
    }
}