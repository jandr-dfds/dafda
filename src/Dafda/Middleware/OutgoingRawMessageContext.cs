using Dafda.Producing;

namespace Dafda.Middleware;

internal class OutgoingRawMessageContext : IMiddlewareContext
{
    public OutgoingRawMessageContext(OutgoingRawMessage message)
    {
        Message = message;
    }

    public OutgoingRawMessage Message { get; }
}