using System.Linq;
using Dafda.Configuration;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestConfigurationKeys
    {
        [Theory]
        [InlineData("group.id", ConfigurationKeys.GroupId)]
        [InlineData("enable.auto.commit", ConfigurationKeys.EnableAutoCommit)]
        [InlineData("bootstrap.servers", ConfigurationKeys.BootstrapServers)]
        [InlineData("broker.version.fallback", ConfigurationKeys.BrokerVersionFallback)]
        [InlineData("api.version.fallback.ms", ConfigurationKeys.ApiVersionFallbackMs)]
        [InlineData("ssl.ca.location", ConfigurationKeys.SslCaLocation)]
        [InlineData("sasl.username", ConfigurationKeys.SaslUsername)]
        [InlineData("sasl.password", ConfigurationKeys.SaslPassword)]
        [InlineData("sasl.mechanisms", ConfigurationKeys.SaslMechanisms)]
        [InlineData("security.protocol", ConfigurationKeys.SecurityProtocol)]
        public void Key_has_correct_name(string expected, string actual)
        {
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Has_all_consumer_configuration_keys()
        {
            Assert.Equal(new[]
            {
                ConfigurationKeys.BootstrapServers,
                ConfigurationKeys.GroupId,
                ConfigurationKeys.EnableAutoCommit,
                ConfigurationKeys.AllowAutoCreateTopics,
                ConfigurationKeys.BrokerVersionFallback,
                ConfigurationKeys.ApiVersionFallbackMs,
                ConfigurationKeys.SslCaLocation,
                ConfigurationKeys.SaslUsername,
                ConfigurationKeys.SaslPassword,
                ConfigurationKeys.SaslMechanisms,
                ConfigurationKeys.SecurityProtocol
            }, ConfigurationKeys.Consumer.Select(x => x.ToString()));
        }

        [Fact]
        public void Has_all_required_consumer_configuration_keys()
        {
            Assert.Equal(new[]
            {
                ConfigurationKeys.BootstrapServers,
                ConfigurationKeys.GroupId
            }, ConfigurationKeys.Consumer.Required);
        }

        [Fact]
        public void Has_all_producer_configuration_keys()
        {
            Assert.Equal(new[]
            {
                ConfigurationKeys.BootstrapServers,
                ConfigurationKeys.BrokerVersionFallback,
                ConfigurationKeys.ApiVersionFallbackMs,
                ConfigurationKeys.SslCaLocation,
                ConfigurationKeys.SaslUsername,
                ConfigurationKeys.SaslPassword,
                ConfigurationKeys.SaslMechanisms,
                ConfigurationKeys.SecurityProtocol
            }, ConfigurationKeys.Producer.Select(x => x.ToString()));
        }

        [Fact]
        public void Has_all_required_producer_configuration_keys()
        {
            Assert.Equal(new[]
            {
                ConfigurationKeys.BootstrapServers
            }, ConfigurationKeys.Producer.Required);
        }
    }
}