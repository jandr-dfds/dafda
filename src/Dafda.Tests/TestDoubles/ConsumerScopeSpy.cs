using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Consuming;

namespace Dafda.Tests.TestDoubles
{
    internal class ConsumerScopeSpy : ConsumerScope
    {
        private readonly MessageResult _messageResult;

        public ConsumerScopeSpy(MessageResult messageResult)
        {
            _messageResult = messageResult;
        }

        public override async Task Consume(Func<MessageResult, Task> onMessageCallback, CancellationToken cancellationToken)
        {
            await onMessageCallback(_messageResult);

            ConsumeCalled++;
        }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }

        public int ConsumeCalled { get; private set; }
        public bool Disposed { get; private set; }
    }
}