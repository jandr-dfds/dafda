using System;
using Dafda.Middleware;

namespace Dafda.Tests.TestDoubles
{
    public class DummyMiddlewareContext : IMiddlewareContext
    {
        public IServiceProvider ServiceProvider => throw new NotImplementedException();

        public void Set<T>(T instance) => throw new NotImplementedException();

        public T Get<T>() => throw new NotImplementedException();
    }
}