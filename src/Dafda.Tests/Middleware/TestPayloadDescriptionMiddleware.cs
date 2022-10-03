using System.Collections.Generic;
using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Middleware;
using Dafda.Producing;
using Dafda.Serializing;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Middleware;

public class TestPayloadDescriptionMiddleware
{
    [Fact]
    public async Task Can_create_payload_descriptor_from_middleware()
    {
        PayloadDescriptor payloadDescriptor = null;
        var outgoingMessageRegistry = new OutgoingMessageRegistryBuilder()
            .Register<object>("dummy-topic", "dummy-type", _ => "dummy-key")
            .Build();
        var sut = new PayloadDescriptionMiddleware(outgoingMessageRegistry, new MessageIdGeneratorStub(() => "dummy-id"));
        var dummyMessage = new object();

        await sut.Invoke(new OutgoingMessageContext(new OutgoingMessage(dummyMessage, new Metadata())), context =>
        {
            payloadDescriptor = context.PayloadDescriptor;
            return Task.CompletedTask;
        });

        Assert.Equal("dummy-id", payloadDescriptor.MessageId);
        Assert.Equal("dummy-type", payloadDescriptor.MessageType);
        Assert.Equal("dummy-key", payloadDescriptor.PartitionKey);
        Assert.Equal("dummy-topic", payloadDescriptor.TopicName);
        Assert.Equal(dummyMessage, payloadDescriptor.MessageData);
        Assert.Equal(
            new Dictionary<string, string>
            {
                { "correlationId", "dummy-id" },
                { "causationId", "dummy-id" },
            },
            payloadDescriptor.MessageHeaders);
    }
}