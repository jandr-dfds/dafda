using Dafda.Consuming;

namespace Dafda.Producing
{
    /// <summary>
    /// The outgoing message and it's <see cref="Metadata"/>.
    /// </summary>
    public class OutgoingMessage
    {
        /// <summary/>
        public OutgoingMessage(object message, Metadata metadata)
        {
            Message = message;
            Metadata = metadata;
        }

        /// <summary>
        /// The outgoing message
        /// </summary>
        public object Message { get; }

        /// <summary>
        /// The metadata for the outgoing message.
        /// </summary>
        public Metadata Metadata { get; }
    }
}