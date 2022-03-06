using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dafda.Consuming
{
    internal abstract class ConsumerScope : IDisposable
    {
        public abstract Task Consume(Func<MessageResult, Task> onMessageCallback, CancellationToken cancellationToken);

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}