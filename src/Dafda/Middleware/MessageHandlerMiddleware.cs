using System;
using System.Threading.Tasks;
using Dafda.Consuming;

namespace Dafda.Middleware;

internal class MessageHandlerMiddleware : IMiddleware<IncomingMessageContext, InvocationContext>
{
    private readonly MessageHandlerRegistry _messageRegistry;
    private readonly Func<Type, object> _resolver;

    public MessageHandlerMiddleware(MessageHandlerRegistry messageRegistry, Func<Type, object> resolver)
    {
        _messageRegistry = messageRegistry;
        _resolver = resolver;
    }

    public Task Invoke(IncomingMessageContext context, Func<InvocationContext, Task> next)
    {
        var messageHandler = _messageRegistry.GetMessageHandlerFor(context.Message.MessageType);
        if (messageHandler == null)
        {
            // TODO -- handle missing message handler registration (previously done by the "NoOpHandler")
            throw new MissingMessageHandlerRegistrationException(
                $"Error! A Handler has not been registered for messages of type {context.Message.MessageType}");
        }
        var messageHandlerInstance = _resolver(messageHandler.HandlerInstanceType);
        return next(new InvocationContext(context.Message, messageHandler.MessageHandler, messageHandlerInstance));
    }
}