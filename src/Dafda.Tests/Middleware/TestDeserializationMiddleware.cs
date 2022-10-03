using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Middleware;

namespace Dafda.Tests.Middleware;

public class TestDeserializationMiddleware
{
    [Fact]
    public async Task can_create_incoming_message_context_with_deserialized_message()
    {
        var dummy = new IncomingMessage(typeof(object), new Metadata(), new object());
        var sut = new DeserializationMiddleware(new DeserializerStub(dummy));
        var spy = MiddlewareFactory.DecorateWithSpy(sut);

        await spy.Invoke(new IncomingRawMessageContext(A.RawMessage, new DummyMiddlewareContext()));

        Assert.Equal(spy.OutContext.Message, dummy);
    }

    private class DeserializerStub : IDeserializer
    {
        private readonly IncomingMessage _incomingMessage;

        public DeserializerStub(IncomingMessage incomingMessage)
        {
            _incomingMessage = incomingMessage;
        }

        public IncomingMessage Deserialize(RawMessage message)
        {
            return _incomingMessage;
        }
    }

    private static class A
    {
        public static RawMessageBuilder RawMessage => new();
    }
}