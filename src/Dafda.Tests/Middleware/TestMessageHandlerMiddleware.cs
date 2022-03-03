using System;
using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Middleware;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Middleware;

public class TestMessageHandlerMiddleware
{
    [Fact]
    public async Task can_get_message_handler_and_resolve_concrete_instance()
    {
        var dummy = new IncomingMessage(typeof(Dummy), new Metadata(), new Dummy());

        var messageHandlerRegistry = new MessageHandlerRegistry();
        messageHandlerRegistry.Register<Dummy, DummyMessageHandler>("", "dummy");

        var dummyMessageHandler = new DummyMessageHandler();

        object Resolver(Type type) => dummyMessageHandler;

        var sut = new MessageHandlerMiddleware(messageHandlerRegistry, Resolver);
        var spy = MiddlewareFactory.DecorateWithSpy(sut);

        await spy.Invoke(new IncomingMessageContext(dummy));

        Assert.Equal(spy.OutContext.Message, dummy);
        Assert.Equal(typeof(DummyMessageHandler), spy.OutContext.MessageHandler.HandlerType);
        Assert.Equal(dummyMessageHandler, spy.OutContext.MessageHandlerInstance);
    }

    private record Dummy;

    private class DummyMessageHandler : IMessageHandler<Dummy>
    {
        public Task Handle(Dummy message, MessageHandlerContext context)
        {
            return Task.CompletedTask;
        }
    }
}