using Dafda.Consuming;

namespace Dafda.Middleware;

/// <summary>
/// Context containing the outgoing message and it's <see cref="Metadata"/>.
/// </summary>
public class OutgoingMessageContext : IMiddlewareContext
{
    /// <summary/>
    public OutgoingMessageContext(object message, Metadata metadata)
    {
        Message = message;
        Metadata = metadata;
    }

    /// <summary>
    /// The outgoing message
    /// </summary>
    public object Message { get; }

    /// <summary>
    /// The metadata for the outgoing message.
    /// </summary>
    public Metadata Metadata { get; }
}