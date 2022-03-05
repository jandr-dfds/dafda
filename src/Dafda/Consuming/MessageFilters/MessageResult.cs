using System.Threading.Tasks;

namespace Dafda.Consuming.MessageFilters
{
    /// <summary>
    /// Object that contains message when consumed from Kafka.
    /// To be used for message handling prior to dispatching to the handlers.
    /// </summary>
    public class MessageResult
    {

        /// <summary>
        /// Resulting Message contaning Transport Level Message
        /// </summary>
        public MessageResult(IncomingMessage message)
        {
            Message = message;
        }

        /// <summary>
        /// Transmitted message consumed from Kafka
        /// </summary>
        public IncomingMessage Message { get; }

        /// <summary>
        /// Commit message to handlers
        /// </summary>
        public Task Commit()
        {
            return Task.CompletedTask;
        }
    }
}