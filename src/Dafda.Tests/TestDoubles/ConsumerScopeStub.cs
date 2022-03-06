using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Consuming;

namespace Dafda.Tests.TestDoubles
{
    internal class ConsumerScopeStub : ConsumerScope
    {
        private readonly MessageResult _messageResult;

        public ConsumerScopeStub(MessageResult messageResult)
        {
            _messageResult = messageResult;
        }

        public override Task Consume(Func<MessageResult, Task> onMessageCallback, CancellationToken cancellationToken)
        {
            return onMessageCallback.Invoke(_messageResult);
        }
    }
}