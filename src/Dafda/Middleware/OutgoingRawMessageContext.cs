using Dafda.Producing;

namespace Dafda.Middleware;

internal class OutgoingRawMessageContext : MiddlewareContext
{
    public OutgoingRawMessageContext(OutgoingRawMessage message, IMiddlewareContext parent)
        : base(parent)
    {
        Message = message;
    }

    public OutgoingRawMessage Message { get; }
}