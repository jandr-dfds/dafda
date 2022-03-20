using Dafda.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestServiceCollectionExtensions
    {
        [Fact]
        public void can_register_and_get_singleton()
        {
            var services = new ServiceCollection();
            var instanceA = services.GetOrAddSingleton(() => new Instance());
            var instanceB = services.GetOrAddSingleton(() => new Instance());

            Assert.Same(instanceA, instanceB);
            Assert.Equal(1, Instance.Created);
        }

        private class Instance
        {
            public static int Created;

            public Instance()
            {
                Created++;
            }
        }
    }
}