using Dafda.Consuming;

namespace Dafda.Middleware;

/// <summary>
/// Context containing the incoming <see cref="RawMessage"/>.
/// </summary>
public class IncomingRawMessageContext : IMiddlewareContext
{
    /// <summary/>
    public IncomingRawMessageContext(RawMessage message)
    {
        Message = message;
    }

    /// <summary>
    /// The <see cref="RawMessage"/>.
    /// </summary>
    public RawMessage Message { get; }
}