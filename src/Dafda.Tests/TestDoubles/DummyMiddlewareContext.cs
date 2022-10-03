using System;
using Dafda.Middleware;

namespace Dafda.Tests.TestDoubles
{
    public class DummyMiddlewareContext : IMiddlewareContext
    {
        public IServiceProvider ServiceProvider => throw new NotImplementedException();
    }
}