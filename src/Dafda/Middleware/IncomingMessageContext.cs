using Dafda.Consuming;

namespace Dafda.Middleware;

/// <summary>
/// Context containing the <see cref="IncomingMessage"/>.
/// </summary>
public class IncomingMessageContext : MiddlewareContext
{
    /// <summary/>
    public IncomingMessageContext(IncomingMessage message, IMiddlewareContext parent)
        : base(parent)
    {
        Message = message;
    }

    /// <summary>
    /// The <see cref="IncomingMessage"/>.
    /// </summary>
    public IncomingMessage Message { get; }
}