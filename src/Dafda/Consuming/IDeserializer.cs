namespace Dafda.Consuming
{
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
}