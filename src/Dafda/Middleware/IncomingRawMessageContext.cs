using Dafda.Consuming;

namespace Dafda.Middleware;

/// <summary>
/// Context containing the incoming <see cref="RawMessage"/>.
/// </summary>
public class IncomingRawMessageContext : MiddlewareContext
{
    /// <summary/>
    public IncomingRawMessageContext(RawMessage message, IMiddlewareContext parent)
        : base(parent)
    {
        Message = message;
    }

    /// <summary>
    /// The <see cref="RawMessage"/>.
    /// </summary>
    public RawMessage Message { get; }
}