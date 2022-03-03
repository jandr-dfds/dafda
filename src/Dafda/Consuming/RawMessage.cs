using System.Collections.Generic;

namespace Dafda.Consuming;

/// <summary>
/// The raw message from Kafka with the following data:
/// <list type="number">
///     <item>The key represented as a string.</item>
///     <item>The headers represented as key, value string pairs.</item>
///     <item>The value (message body) is left as a byte array.</item>
/// </list>
/// </summary>
public sealed class RawMessage
{
    internal RawMessage(string key, IDictionary<string, string> headers, byte[] data)
    {
        Key = key;
        Headers = headers;
        Data = data;
    }

    /// <summary>
    /// The partition key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// The message headers.
    /// </summary>
    public IDictionary<string, string> Headers { get; }

    /// <summary>
    /// The raw message data.
    /// </summary>
    public byte[] Data { get; }
}