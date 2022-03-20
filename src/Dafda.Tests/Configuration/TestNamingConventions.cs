using Dafda.Configuration;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestNamingConventions
    {
        [Fact]
        public void can_return_empty_list_of_attempted_keys()
        {
            var sut = new NamingConventions();

            var attemptedKeys = sut.GetAttemptedKeys("key");

            Assert.Empty(attemptedKeys);
        }

        [Fact]
        public void can_return_list_of_attempted_keys()
        {
            var sut = new NamingConventions
            {
                NamingConvention.Default,
                key => key.ToUpper(),
                { "DEFAULT_KAFKA", "SAMPLE_KAFKA" }
            };

            var attemptedKeys = sut.GetAttemptedKeys("key");

            Assert.Equal(new[]
            {
                "key",
                "KEY",
                "DEFAULT_KAFKA_KEY",
                "SAMPLE_KAFKA_KEY"
            }, attemptedKeys);
        }
    }
}