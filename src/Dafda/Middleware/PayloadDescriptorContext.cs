using Dafda.Serializing;

namespace Dafda.Middleware;

internal class PayloadDescriptorContext : IMiddlewareContext
{
    public PayloadDescriptorContext(PayloadDescriptor payloadDescriptor)
    {
        PayloadDescriptor = payloadDescriptor;
    }

    public PayloadDescriptor PayloadDescriptor { get; }
}