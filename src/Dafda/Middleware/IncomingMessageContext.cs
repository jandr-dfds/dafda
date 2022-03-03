using Dafda.Consuming;

namespace Dafda.Middleware;

/// <summary>
/// Context containing the <see cref="IncomingMessage"/>.
/// </summary>
public class IncomingMessageContext
{
    /// <summary/>
    public IncomingMessageContext(IncomingMessage message)
    {
        Message = message;
    }

    /// <summary>
    /// The <see cref="IncomingMessage"/>.
    /// </summary>
    public IncomingMessage Message { get; }
}