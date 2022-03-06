using System.Threading;

namespace Dafda;

internal abstract class Cancelable
{
    protected Cancelable(CancellationToken token)
    {
        Token = token;
    }

    public CancellationToken Token { get; }
    public virtual bool IsCancelled => Token.IsCancellationRequested;
}

internal class Until : Cancelable
{
    public static Until CancelledBy(CancellationToken token)
    {
        return new Until(token);
    }

    private Until(CancellationToken token) : base(token)
    {
    }
}