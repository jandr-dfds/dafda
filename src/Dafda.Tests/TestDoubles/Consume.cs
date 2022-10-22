using System.Threading;

namespace Dafda.Tests.TestDoubles;

internal sealed class Consume : Cancelable
{
    public static Cancelable Forever => Until.CancelledBy(CancellationToken.None);
    public static Consume Once => new(1);
    public static Consume Twice => new(2);

    private readonly int _maxCount;
    private int _count;

    private Consume(int maxCount) : base(CancellationToken.None)
    {
        _maxCount = maxCount;
    }

    public override bool IsCancelled => _count++ >= _maxCount;
}