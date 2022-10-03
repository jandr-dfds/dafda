using Dafda.Consuming;
using Dafda.Middleware;

namespace Dafda.Tests.Builders;

internal class InvocationContextBuilder
{
    private IncomingMessage _incomingMessage;
    private MessageHandlerDelegate _messageHandler;
    private object _messageHandlerInstance;

    public InvocationContextBuilder With(IncomingMessage incomingMessage)
    {
        _incomingMessage = incomingMessage;
        return this;
    }

    public InvocationContextBuilder With(MessageHandlerDelegate messageHandler)
    {
        _messageHandler = messageHandler;
        return this;
    }

    public InvocationContextBuilder WithMessageHandlerInstance<T>(IMessageHandler<T> messageHandlerInstance)
    {
        _messageHandlerInstance = messageHandlerInstance;
        return this;
    }

    public InvocationContext Build()
    {
        return new InvocationContext(_incomingMessage, _messageHandler, _messageHandlerInstance, new DummyMiddlewareContext());
    }

    public static implicit operator InvocationContext(InvocationContextBuilder builder)
    {
        return builder.Build();
    }
}