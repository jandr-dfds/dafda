using Dafda.Producing;

namespace Dafda.Middleware;

/// <summary>
/// Context containing the <see cref="OutgoingMessage"/>.
/// </summary>
public class OutgoingMessageContext : IMiddlewareContext
{
    /// <summary/>
    public OutgoingMessageContext(OutgoingMessage message)
    {
        Message = message;
    }

    /// <summary>
    /// The outgoing message
    /// </summary>
    public OutgoingMessage Message { get; }
}