namespace Dafda.Producing;

internal class OutgoingRawMessage
{
    public OutgoingRawMessage(string topic, string key, string data)
    {
        Topic = topic;
        Key = key;
        Data = data;
    }

    public string Topic { get; }
    public string Key { get; }
    public string Data { get; }
}