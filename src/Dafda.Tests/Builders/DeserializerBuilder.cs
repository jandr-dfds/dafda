using Dafda.Consuming;

namespace Dafda.Tests.Builders;

internal class DeserializerBuilder
{
    private MessageHandlerRegistry _messageHandlerRegistry = new();

    public DeserializerBuilder With(MessageHandlerRegistry messageHandlerRegistry)
    {
        _messageHandlerRegistry = messageHandlerRegistry;
        return this;
    }

    public Deserializer Build()
    {
        return new Deserializer(_messageHandlerRegistry);
    }
}