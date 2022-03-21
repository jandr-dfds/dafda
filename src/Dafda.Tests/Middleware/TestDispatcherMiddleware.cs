using System.Threading.Tasks;
using Dafda.Middleware;
using Dafda.Producing;
using Dafda.Serializing;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Middleware;

public class TestDispatcherMiddleware
{
    [Fact]
    public async Task Can_dispatch_outgoing_raw_message()
    {
        OutgoingRawMessage message = null;
        var sut = new SerializationMiddleware(new TopicPayloadSerializerRegistry(() => new PayloadSerializerStub("dummy-data")));
        var payloadDescriptor = new PayloadDescriptorBuilder()
            .WithTopicName("dummy-topic")
            .WithPartitionKey("dummy-key")
            .Build();

        await sut.Invoke(new PayloadDescriptorContext(payloadDescriptor), context =>
        {
            message = context.Message;
            return Task.CompletedTask;
        });
        
        Assert.Equal("dummy-topic", message.Topic);
        Assert.Equal("dummy-key", message.Key);
        Assert.Equal("dummy-data", message.Data);
    }
}