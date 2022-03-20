using System.Linq;
using Dafda.Configuration;
using Dafda.Producing;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestProducerOptions
    {
        [Fact]
        public void Can_validate_configuration()
        {
            var sut = new ProducerOptions(new OutgoingMessageRegistry());

            Assert.Throws<InvalidConfigurationException>(() => sut.Build());
        }

        [Fact]
        public void Can_build_minimal_configuration()
        {
            var sut = new ProducerOptions(new OutgoingMessageRegistry());
            sut.WithBootstrapServers("foo");

            var configuration = sut.Build();

            AssertKeyValue(configuration, ConfigurationKeys.BootstrapServers, "foo");
        }

        private static void AssertKeyValue(ProducerConfiguration configuration, string expectedKey, string expectedValue)
        {
            configuration.KafkaConfiguration.FirstOrDefault(x => x.Key == expectedKey).Deconstruct(out _, out var actualValue);

            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void Can_build_producer_configuration()
        {
            var sut = new ProducerOptions(new OutgoingMessageRegistry());
            sut.WithConfigurationSource(new ConfigurationSourceStub(
                (key: ConfigurationKeys.BootstrapServers, value: "foo"),
                (key: ConfigurationKeys.SaslUsername, value: "username"),
                (key: ConfigurationKeys.SaslMechanisms, value: "foo"),
                (key: "DEFAULT_KAFKA_SASL_MECHANISMS", value: "default"),
                (key: "SAMPLE_KAFKA_SASL_MECHANISMS", value: "sample"),
                (key: "dummy", value: "ignored")
            ));
            sut.WithEnvironmentStyle("DEFAULT_KAFKA", "SAMPLE_KAFKA");
            sut.WithNamingConvention(NamingConvention.Default);
            sut.WithConfiguration(ConfigurationKeys.BootstrapServers, "bar");

            var configuration = sut.Build();

            AssertKeyValue(configuration, ConfigurationKeys.BootstrapServers, "bar");
            AssertKeyValue(configuration, ConfigurationKeys.SaslUsername, "username");
            AssertKeyValue(configuration, ConfigurationKeys.SaslMechanisms, "default");
            AssertKeyValue(configuration, "dummy", null);
        }

        [Fact]
        public void Has_expected_message_id_generator()
        {
            var dummy = MessageIdGenerator.Default;
            var sut = new ProducerOptions(new OutgoingMessageRegistry());
            sut.WithBootstrapServers("foo");
            sut.WithMessageIdGenerator(dummy);

            var producerConfiguration = sut.Build();

            Assert.Equal(dummy, producerConfiguration.MessageIdGenerator);
        }
    }
}