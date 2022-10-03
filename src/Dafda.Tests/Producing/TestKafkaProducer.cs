using System.Threading.Tasks;

namespace Dafda.Tests.Producing
{
    public class TestKafkaProducer
    {
        [Fact]
        public async Task produces_to_expected_topic()
        {
            const string dummyTopic = "foo topic name";
            var spy = new KafkaProducerSpy();

            await spy.Produce(A.OutgoingRawMessage.WithTopic(dummyTopic));

            Assert.Equal(dummyTopic, spy.Topic);
        }

        [Fact]
        public async Task produces_message_with_expected_key()
        {
            const string dummyKey = "foo partition key";
            var spy = new KafkaProducerSpy();

            await spy.Produce(A.OutgoingRawMessage.WithKey(dummyKey));

            Assert.Equal(dummyKey, spy.Key);
        }

        [Fact]
        public async Task produces_message_with_expected_value()
        {
            const string dummyData = "foo value 123";
            var spy = new KafkaProducerSpy();

            await spy.Produce(A.OutgoingRawMessage.WithData(dummyData));

            Assert.Equal(dummyData, spy.Value);
        }

        private static class A
        {
            public static OutgoingRawMessageBuilder OutgoingRawMessage => new();
        }
    }
}