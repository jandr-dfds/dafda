using System;
using System.Threading.Tasks;
using Dafda.Consuming;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Middleware;

internal class MessageHandlerMiddleware : IMiddleware<IncomingMessageContext, InvocationContext>
{
    private readonly MessageHandlerRegistry _messageRegistry;

    public MessageHandlerMiddleware(MessageHandlerRegistry messageRegistry)
    {
        _messageRegistry = messageRegistry;
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
        var messageHandlerInstance = context.ServiceProvider.GetRequiredService(messageHandler.HandlerInstanceType);
        return next(new InvocationContext(context.Message, messageHandler.MessageHandler, messageHandlerInstance, context));
    }
}