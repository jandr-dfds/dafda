using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Consuming;

public class TestDeserializer
{
    private const string MessageJson = "{\"messageId\":\"message-id\",\"type\":\"vehicle_position_changed\",\"data\":{\"aggregateId\":\"aggregate-id\",\"vehicleId\":\"vehicle-id\",\"timeStamp\":\"2019-09-16T07:59:01Z\",\"position\":{\"latitude\":1,\"longitude\":2}}}";

    [Fact]
    public void Can_access_message_headers()
    {
        var messageHandlerRegistry = new MessageHandlerRegistry();
        messageHandlerRegistry.Register<VehiclePositionChanged, VehiclePositionChangedHandler>("", "vehicle_position_changed");
        var sut = new DeserializerBuilder().With(messageHandlerRegistry).Build();

        var message = sut.Deserialize(A.RawMessage.WithData(MessageJson));

        Assert.Equal("message-id", message.Metadata.MessageId);
        Assert.Equal("vehicle_position_changed", message.Metadata.Type);
    }

    private const string MessageJsonWithNonStringMetadata = "{\"messageId\":12345, \"type\":\"vehicle_position_changed\",\"data\":{\"aggregateId\":\"aggregate-id\",\"vehicleId\":\"vehicle-id\",\"timeStamp\":\"2019-09-16T07:59:01Z\",\"position\":{\"latitude\":1,\"longitude\":2}}}";

    [Fact]
    public void Can_read_message_headers_non_string()
    {
        var messageHandlerRegistry = new MessageHandlerRegistry();
        messageHandlerRegistry.Register<VehiclePositionChanged, VehiclePositionChangedHandler>("", "vehicle_position_changed");
        var sut = new DeserializerBuilder().With(messageHandlerRegistry).Build();

        var message = sut.Deserialize(A.RawMessage.WithData(MessageJsonWithNonStringMetadata));

        Assert.Equal("12345", message.Metadata.MessageId);
    }

    [Fact]
    public void Can_decode_data_body()
    {
        var messageHandlerRegistry = new MessageHandlerRegistry();
        messageHandlerRegistry.Register<VehiclePositionChanged, VehiclePositionChangedHandler>("", "vehicle_position_changed");
        var sut = new DeserializerBuilder().With(messageHandlerRegistry).Build();

        var message = sut.Deserialize(A.RawMessage.WithData(MessageJson));

        var data = Assert.IsType<VehiclePositionChanged>(message.Instance);
        Assert.Equal("aggregate-id", data.AggregateId);
        Assert.Equal("vehicle-id", data.VehicleId);
        Assert.Equal(new DateTime(2019, 9, 16, 7, 59, 1, DateTimeKind.Utc), data.TimeStamp);
        Assert.Equal(1, data.Position.Latitude);
        Assert.Equal(2, data.Position.Longitude);
    }

    private const string MessageJsonWithNonCamelCaseFields = "{\"messageId\":12345, \"type\":\"vehicle_position_changed\",\"data\":{\"aggregateId\":\"aggregate-id\",\"VehicleId\":\"vehicle-id\",\"TIMEStamp\":\"2019-09-16T07:59:01Z\",\"PosiTion\":{\"latitude\":1,\"longitude\":2}}}";

    [Fact]
    public void Can_read_message_with_non_camel_case_data_fields()
    {
        var messageHandlerRegistry = new MessageHandlerRegistry();
        messageHandlerRegistry.Register<VehiclePositionChanged, VehiclePositionChangedHandler>("", "vehicle_position_changed");
        var sut = new DeserializerBuilder().With(messageHandlerRegistry).Build();

        var message = sut.Deserialize(A.RawMessage.WithData(MessageJsonWithNonCamelCaseFields));

        var data = Assert.IsType<VehiclePositionChanged>(message.Instance);
        Assert.Equal("aggregate-id", data.AggregateId);
        Assert.Equal("vehicle-id", data.VehicleId);
        Assert.Equal(new DateTime(2019, 9, 16, 7, 59, 1, DateTimeKind.Utc), data.TimeStamp);
        Assert.Equal(1, data.Position.Latitude);
        Assert.Equal(2, data.Position.Longitude);
    }

    private const string MalformedJsonMessage = "{\"aliceCooper\":\"Your cruel device, your blood like ice\"}";

    [Fact]
    public void Malformed_message_throws_exception()
    {
        var messageHandlerRegistry = new MessageHandlerRegistry();
        messageHandlerRegistry.Register<VehiclePositionChanged, VehiclePositionChangedHandler>("", "vehicle_position_changed");
        var sut = new DeserializerBuilder().With(messageHandlerRegistry).Build();

        Assert.Throws<KeyNotFoundException>(() => sut.Deserialize(A.RawMessage.WithData(MalformedJsonMessage)));
    }

    private const string InvalidJsonMessage = "{This is not json at all}";

    [Fact]
    public void InvalidMessage_message_throws_exception()
    {
        var messageHandlerRegistry = new MessageHandlerRegistry();
        messageHandlerRegistry.Register<VehiclePositionChanged, VehiclePositionChangedHandler>("", "vehicle_position_changed");
        var sut = new DeserializerBuilder().With(messageHandlerRegistry).Build();

        Assert.ThrowsAny<JsonException>(() => sut.Deserialize(A.RawMessage.WithData(InvalidJsonMessage)));
    }

    public record VehiclePositionChanged(string AggregateId, string VehicleId, DateTime TimeStamp, Position Position);

    public class VehiclePositionChangedHandler : IMessageHandler<VehiclePositionChanged>
    {
        public Task Handle(VehiclePositionChanged message, MessageHandlerContext context)
        {
            return Task.CompletedTask;
        }
    }

    public record Position(double Latitude, double Longitude);

    private static class A
    {
        public static RawMessageBuilder RawMessage => new();
    }
}