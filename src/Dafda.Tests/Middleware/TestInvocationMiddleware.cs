using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Middleware;

namespace Dafda.Tests.Middleware;

public class TestInvocationMiddleware
{
    [Fact]
    public async Task can_create_incoming_message_context_with_deserialized_message()
    {
        var dummyMessage = new Dummy();
        var spy = new MessageHandlerSpy();
        var sut = new InvocationMiddleware();

        await sut.Invoke(
            A.InvocationContext
                .With(A.IncomingMessage.WithMessage(dummyMessage).WithMetadata("dummy-key", "dummy-value"))
                .With(MessageHandlerDelegate.Create<Dummy, MessageHandlerSpy>())
                .WithMessageHandlerInstance(spy),
            _ => Task.CompletedTask);

        Assert.Equal(dummyMessage, spy.Message);
        Assert.Equal("dummy-value", spy.Context?["dummy-key"]);
    }

    private record Dummy;

    private class MessageHandlerSpy : IMessageHandler<Dummy>
    {
        public Task Handle(Dummy message, MessageHandlerContext context)
        {
            Message = message;
            Context = context;
            return Task.CompletedTask;
        }

        public object Message { get; private set; }
        public MessageHandlerContext Context { get; private set; }
    }

    private static class A
    {
        public static InvocationContextBuilder InvocationContext => new();
        public static IncomingMessageBuilder IncomingMessage => new();
    }
}