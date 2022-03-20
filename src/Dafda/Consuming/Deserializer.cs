using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dafda.Consuming
{
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
}