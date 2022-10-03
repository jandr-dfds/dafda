using System.Threading.Tasks;
using Dafda.Middleware;
using Dafda.Producing;

namespace Dafda.Tests.Middleware;

public class TestSerializationMiddleware
{
    [Fact]
    public async Task Can_create_outgoing_raw_message()
    {
        var spy = new KafkaProducerSpy();
        var sut = new DispatchMiddleware(spy);

        var outgoingRawMessage = new OutgoingRawMessage("dummy-topic", "dummy-key", "dummy-data");
        await sut.Invoke(new OutgoingRawMessageContext(outgoingRawMessage), _ => Task.CompletedTask);
        
        Assert.Equal("dummy-topic",  spy.Topic);
        Assert.Equal("dummy-key",  spy.Key);
        Assert.Equal("dummy-data",  spy.Value);
    }
}