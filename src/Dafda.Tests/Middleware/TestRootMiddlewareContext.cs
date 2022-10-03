using System;
using Dafda.Middleware;
using Moq;

namespace Dafda.Tests.Middleware
{
    public class TestRootMiddlewareContext
    {
        [Fact]
        public void Can_get_default_value_for_value_type()
        {
            var sut = new RootMiddlewareContext(Dummy.Of<IServiceProvider>());
            
            var result = sut.Get<int>();

            Assert.Equal(default(int), result);
        }

        [Fact]
        public void Can_get_default_value_for_reference_type()
        {
            var sut = new RootMiddlewareContext(Dummy.Of<IServiceProvider>());
            
            var result = sut.Get<object>();

            Assert.Null(result);
        }

        [Fact]
        public void Can_retrieve_stored_value()
        {
            var sut = new RootMiddlewareContext(Dummy.Of<IServiceProvider>());
            var instance = new object();

            sut.Set(instance);
           
            var result = sut.Get<object>();

            Assert.Same(instance, result);
        }
    }
}
