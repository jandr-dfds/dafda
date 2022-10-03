using Dafda.Producing;

namespace Dafda.Middleware;

/// <summary>
/// Context containing the <see cref="OutgoingMessage"/>.
/// </summary>
public class OutgoingMessageContext : MiddlewareContext
{
    /// <summary/>
    public OutgoingMessageContext(OutgoingMessage message, IMiddlewareContext parent)
        : base(parent)
    {
        Message = message;
    }

    /// <summary>
    /// The outgoing message
    /// </summary>
    public OutgoingMessage Message { get; }
}