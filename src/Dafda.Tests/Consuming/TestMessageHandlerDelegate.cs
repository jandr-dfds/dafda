using System.Threading.Tasks;
using Dafda.Consuming;
using Xunit;

namespace Dafda.Tests.Consuming;

public class TestMessageHandlerDelegate
{
    [Fact]
    public void can_create_message_handler()
    {
        var messageHandler = MessageHandlerDelegate.Create<DummyMessage, MessageHandlerSpy>();

        Assert.Equal(typeof(MessageHandlerSpy), messageHandler.HandlerType);
    }

    [Fact]
    public void ensure_message_handler_type_is_allowed()
    {
        Assert.Throws<MessageRegistrationException>(() =>
            MessageHandlerDelegate.Create(typeof(DummyMessage), typeof(object))
        );
    }

    [Fact]
    public async Task can_invoke_message_handler()
    {
        var spy = new MessageHandlerSpy();
        var dummy = new DummyMessage();
        var context = new MessageHandlerContext(new Metadata());
        var messageHandler = MessageHandlerDelegate.Create<DummyMessage, MessageHandlerSpy>();

        await messageHandler.Invoke(spy, dummy, context);

        Assert.Same(dummy, spy.Message);
        Assert.Same(context, spy.Context);
    }

    private record DummyMessage;

    private class MessageHandlerSpy : IMessageHandler<DummyMessage>
    {
        public Task Handle(DummyMessage message, MessageHandlerContext context)
        {
            Message = message;
            Context = context;
            return Task.CompletedTask;
        }

        public object Message { get; private set; }
        public MessageHandlerContext Context { get; private set; }
    }
}