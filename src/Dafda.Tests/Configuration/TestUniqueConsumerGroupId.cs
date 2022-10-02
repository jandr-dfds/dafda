using Dafda.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestUniqueConsumerGroupId
    {
        [Fact]
        public void Can_register_multiple_group_ids()
        {
            var services = new ServiceCollection();
            
            UniqueConsumerGroupId.Ensure(services, "foo");
            UniqueConsumerGroupId.Ensure(services, "bar");
        }
        
        [Fact]
        public void Cannot_register_identical_group_ids()
        {
            var services = new ServiceCollection();
            
            UniqueConsumerGroupId.Ensure(services, "foo");

            Assert.Throws<InvalidConfigurationException>(() => UniqueConsumerGroupId.Ensure(services, "foo"));
        }
    }
}
