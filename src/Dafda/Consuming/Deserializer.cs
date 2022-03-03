using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dafda.Consuming;

/// <summary>
/// The implementation of this interface will be in charge of deserializing the raw
/// Kafka messages as they are consumed by the low-level Kafka consumer.
/// </summary>
public interface IDeserializer
{
    /// <summary>
    /// Transforms the <see cref="RawMessage"/> into a <see cref="IncomingMessage"/>.
    /// </summary>
    /// <param name="message">The <see cref="RawMessage"/>.</param>
    /// <returns>The <see cref="IncomingMessage"/> containing the deserialized message.</returns>
    IncomingMessage Deserialize(RawMessage message);
}

internal class Deserializer : IDeserializer
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    private readonly MessageHandlerRegistry _registry;

    public Deserializer(MessageHandlerRegistry registry)
    {
        _registry = registry;
    }

    public IncomingMessage Deserialize(RawMessage message)
    {
        using var jsonDocument = JsonDocument.Parse(message.Data);

        var metadataProperties = jsonDocument
            .RootElement
            .EnumerateObject()
            .Where(property => property.Name != MessageEnvelopeProperties.Data)
            .ToDictionary(x => x.Name, x => x.Value.ToString());

        var metadata = new Metadata(metadataProperties);
        var registration = _registry.GetRegistrationFor(metadata.Type);

        var dataProperty = jsonDocument.RootElement.GetProperty(MessageEnvelopeProperties.Data);
        var jsonData = dataProperty.GetRawText();
        var instance = JsonSerializer.Deserialize(jsonData, registration.MessageInstanceType, JsonSerializerOptions);

        return new IncomingMessage(registration.MessageInstanceType, metadata, instance);
    }
}