using System;
using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Middleware;
using Microsoft.Extensions.DependencyInjection;

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

        var sut = new MessageHandlerMiddleware(messageHandlerRegistry);
        var spy = MiddlewareFactory.DecorateWithSpy(sut);

        var serviceProvider = new ServiceCollection()
            .AddTransient(_ => dummyMessageHandler)
            .BuildServiceProvider();
        
        await spy.Invoke(new IncomingMessageContext(dummy, new RootMiddlewareContext(serviceProvider)));

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