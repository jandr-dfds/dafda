using System.Threading;
using System.Threading.Tasks;
using Dafda.Consuming;

namespace Dafda.Tests.TestDoubles;

internal class CancellingConsumerScope : ConsumerScope
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly MessageResult _result;
    private readonly int _cancelAfter;

    private int _nextCount;

    public CancellingConsumerScope(MessageResult result, int cancelAfter = 1)
    {
        _result = result;
        _cancelAfter = cancelAfter;
    }

    public override Task<MessageResult> GetNext(CancellationToken cancellationToken)
    {
        if (++_nextCount >= _cancelAfter)
        {
            _cancellationTokenSource.Cancel();
        }

        return Task.FromResult(_result);
    }

    public CancellationToken Token => _cancellationTokenSource.Token;

    public override void Dispose()
    {
        Disposed++;
        _cancellationTokenSource.Dispose();
    }

    public int Disposed { get; private set; }
}