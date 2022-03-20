using Dafda.Configuration;
using Dafda.Producing;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Dafda.Tests.Producing
{
    public class TestProducerRegistry
    {
        [Fact]
        public void returns_expected_when_nothing_has_been_registered()
        {
            var sut = new ProducerRegistryBuilder().Build();
            var result = sut.Get("foo", NullLoggerFactory.Instance);

            Assert.Null(result);
        }

        [Fact]
        public void returns_expected_when_getting_by_a_known_name()
        {
            var options = new ProducerOptions(new ServiceCollection());
            options.WithBootstrapServers("dummy");
            var producerConfigurationStub = options.Build();

            var sut = new ProducerRegistryBuilder().Build();
            sut.ConfigureProducer("foo", producerConfigurationStub);

            var result = sut.Get("foo", NullLoggerFactory.Instance);

            Assert.IsType<Producer>(result);
            Assert.NotNull(result);
        }

        [Fact]
        public void returns_expected_when_getting_by_an_unknown_name()
        {
            var options = new ProducerOptions(new ServiceCollection());
            options.WithBootstrapServers("dummy");
            var producerConfigurationStub = options.Build();

            var sut = new ProducerRegistryBuilder().Build();
            sut.ConfigureProducer("foo", producerConfigurationStub);

            var result = sut.Get("bar", NullLoggerFactory.Instance);

            Assert.Null(result);
        }

        [Fact]
        public void throws_expected_when_adding_multiple_producers_with_same_name()
        {
            var sut = new ProducerRegistryBuilder().Build();

            var options = new ProducerOptions(new ServiceCollection());
            options.WithBootstrapServers("dummy");
            var producerConfiguration = options.Build();
            
            sut.ConfigureProducer(
                producerName: "foo",
                configuration: producerConfiguration
            );

            Assert.Throws<ProducerFactoryException>(() => sut.ConfigureProducer(
                producerName: "foo",
                configuration: producerConfiguration
            ));
        }

        [Fact]
        public void when_disposing_the_factory_all_kafka_producers_are_also_disposed()
        {
            var spy = new KafkaProducerSpy();

            using (var sut = new ProducerRegistryBuilder().Build())
            {
                var options = new ProducerOptions(new ServiceCollection());
                options.WithBootstrapServers("dummy");
                options.WithKafkaProducerFactory(_ => spy);
                var producerConfiguration = options.Build();

                sut.ConfigureProducer(
                    producerName: "foo",
                    configuration: producerConfiguration
                );

                sut.Get("foo", NullLoggerFactory.Instance);
            }

            Assert.True(spy.WasDisposed);
        }
    }
}