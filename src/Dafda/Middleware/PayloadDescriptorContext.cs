using Dafda.Serializing;

namespace Dafda.Middleware;

internal class PayloadDescriptorContext : MiddlewareContext
{
    public PayloadDescriptorContext(PayloadDescriptor payloadDescriptor, IMiddlewareContext parent)
        : base(parent)
    {
        PayloadDescriptor = payloadDescriptor;
    }

    public PayloadDescriptor PayloadDescriptor { get; }
}