using Dafda.Consuming;

namespace Dafda.Tests.TestDoubles;

internal class ConsumerScopeSpy : ConsumerScopeStub
{
    public ConsumerScopeSpy(MessageResult result) : base(result)
    {
    }

    public override void Dispose()
    {
        Disposed++;
        base.Dispose();
    }

    public int Disposed { get; private set; }
}