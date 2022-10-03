using Dafda.Consuming;

namespace Dafda.Middleware;

/// <summary>
/// Context with everything needed to invoke the <see cref="IMessageHandler{T}"/> implementation
/// for the incoming message.
/// </summary>
public class InvocationContext : MiddlewareContext
{
    /// <summary/>
    public InvocationContext(IncomingMessage message, MessageHandlerDelegate messageHandler, object messageHandlerInstance, IMiddlewareContext parent)
        : base(parent)
    {
        Message = message;
        MessageHandler = messageHandler;
        MessageHandlerInstance = messageHandlerInstance;
    }

    /// <summary>
    /// The <see cref="IncomingMessage"/>.
    /// </summary>
    public IncomingMessage Message { get; }

    /// <summary>
    /// The <see cref="MessageHandlerDelegate"/>. 
    /// </summary>
    public MessageHandlerDelegate MessageHandler { get; }

    /// <summary>
    /// The instance of the <see cref="IMessageHandler{T}"/> implementation. 
    /// </summary>
    public object MessageHandlerInstance { get; }
}