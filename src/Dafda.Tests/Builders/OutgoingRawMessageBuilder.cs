using Dafda.Producing;

namespace Dafda.Tests.Builders;

internal class OutgoingRawMessageBuilder
{
    private string _key;
    private string _topic;
    private string _data;

    public OutgoingRawMessageBuilder WithKey(string key)
    {
        _key = key;
        return this;
    }

    public OutgoingRawMessageBuilder WithTopic(string topic)
    {
        _topic = topic;
        return this;
    }

    public OutgoingRawMessageBuilder WithData(string data)
    {
        _data = data;
        return this;
    }

    public OutgoingRawMessage Build()
    {
        return new OutgoingRawMessage(_topic, _key, _data);
    }

    public static implicit operator OutgoingRawMessage(OutgoingRawMessageBuilder builder)
    {
        return builder.Build();
    }
}