using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming;
using Dafda.Tests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestConsumerOptions
    {
        [Fact]
        public void Can_validate_configuration()
        {
            var sut = new ConsumerOptions(new ServiceCollection());

            Assert.Throws<InvalidConfigurationException>(() => sut.Build());
        }

        [Fact]
        public void Can_build_minimal_configuration()
        {
            var sut = new ConsumerOptions(new ServiceCollection());
            sut.WithGroupId("foo");
            sut.WithBootstrapServers("bar");
            var configuration = sut.Build();

            AssertKeyValue(configuration.KafkaConfiguration, ConfigurationKey.GroupId, "foo");
            AssertKeyValue(configuration.KafkaConfiguration, ConfigurationKey.BootstrapServers, "bar");
        }

        private static void AssertKeyValue(IDictionary<string, string> configuration, string expectedKey, string expectedValue)
        {
            configuration.FirstOrDefault(x => x.Key == expectedKey).Deconstruct(out _, out var actualValue);

            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void Can_build_consumer_configuration()
        {
            var sut = new ConsumerOptions(new ServiceCollection());
            sut.WithConfigurationSource(new ConfigurationSourceStub(
                (key: "DEFAULT_KAFKA_GROUP_ID", value: "default_foo"),
                (key: "SAMPLE_KAFKA_ENABLE_AUTO_COMMIT", value: "true"),
                (key: "SAMPLE_KAFKA_ALLOW_AUTO_CREATE_TOPICS", value: "false"),
                (key: ConfigurationKey.GroupId, value: "foo"),
                (key: ConfigurationKey.BootstrapServers, value: "bar"),
                (key: "dummy", value: "ignored")
            ));
            sut.WithNamingConvention(NamingConvention.Default);
            sut.WithEnvironmentStyle("DEFAULT_KAFKA", "SAMPLE_KAFKA");
            sut.WithConfiguration(ConfigurationKey.GroupId, "baz");
            var configuration = sut.Build();

            AssertKeyValue(configuration.KafkaConfiguration, ConfigurationKey.GroupId, "baz");
            AssertKeyValue(configuration.KafkaConfiguration, ConfigurationKey.BootstrapServers, "bar");
            AssertKeyValue(configuration.KafkaConfiguration, ConfigurationKey.EnableAutoCommit, "true");
            AssertKeyValue(configuration.KafkaConfiguration, ConfigurationKey.AllowAutoCreateTopics, "false");
            AssertKeyValue(configuration.KafkaConfiguration, "dummy", null);
        }

        [Fact]
        public void Can_register_message_handler()
        {
            var sut = new ConsumerOptions(new ServiceCollection());
            sut.WithGroupId("foo");
            sut.WithBootstrapServers("bar");
            sut.RegisterMessageHandler<DummyMessage, DummyMessageHandler>("dummyTopic", nameof(DummyMessage));
            var configuration = sut.Build();

            var registration = configuration.MessageHandlerRegistry.GetRegistrationFor(nameof(DummyMessage));

            Assert.Equal(typeof(DummyMessageHandler), registration.HandlerInstanceType);
        }

        [Fact]
        public void returns_expected_auto_commit_when_not_set()
        {
            var sut = new ConsumerOptions(new ServiceCollection());
            sut.WithGroupId("foo");
            sut.WithBootstrapServers("bar");
            var configuration = sut.Build();

            Assert.True(configuration.EnableAutoCommit);
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("TRUE", true)]
        [InlineData("false", false)]
        [InlineData("FALSE", false)]
        public void returns_expected_auto_commit_when_configured_with_valid_value(string configValue, bool expected)
        {
            var sut = new ConsumerOptions(new ServiceCollection());
            sut.WithGroupId("foo");
            sut.WithBootstrapServers("bar");
            sut.WithConfiguration(ConfigurationKey.EnableAutoCommit, configValue);
            var configuration = sut.Build();
            ;

            Assert.Equal(expected, configuration.EnableAutoCommit);
        }

        private record DummyMessage;

        private class DummyMessageHandler : IMessageHandler<DummyMessage>
        {
            public Task Handle(DummyMessage message, MessageHandlerContext context)
            {
                return Task.CompletedTask;
            }
        }
    }
}