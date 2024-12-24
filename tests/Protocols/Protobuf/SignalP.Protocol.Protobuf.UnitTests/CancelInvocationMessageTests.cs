using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Moq;
using SignalP.Protocol.Protobuf.UnitTests.Helpers;
using System.Buffers;
using Xunit;

namespace SignalP.Protocol.Protobuf.UnitTests;

public class CancelInvocationMessageTests
{
    [
        Theory,
        InlineData("1"),
        InlineData("1234"),
        InlineData("9876543210123456789"),
        InlineData("")
    ]
    public void Protocol_Should_Handle_CancelInvocationMessage_Without_Header(string invocationId)
    {
        var binder = new Mock<IInvocationBinder>();

        var protobufHubProtocol = new ProtobufHubProtocol();
        var writer = new CommunityToolkit.HighPerformance.Buffers.MemoryBufferWriter<byte>(new Memory<byte>(new byte[100000]));
        var cancelInvocationMessage = new CancelInvocationMessage(invocationId);

        protobufHubProtocol.WriteMessage(cancelInvocationMessage, writer);
        var encodedMessage = new ReadOnlySequence<byte>(writer.WrittenSpan.ToArray());
        var result = protobufHubProtocol.TryParseMessage(ref encodedMessage, binder.Object, out var resultCancelInvocationMessage);

        result.Should().Be(true);

        resultCancelInvocationMessage
            .Should()
            .NotBeNull()
            .And
            .BeOfType<CancelInvocationMessage>()
            .Subject
            .Should()
            .BeEquivalentTo(cancelInvocationMessage);
    }

    [
        Theory,
        InlineData("key", "value"),
        InlineData("foo", "bar", "2048", "4096"),
        InlineData("@21", "fdf", "g123", "aloha", "42", "28")
    ]
    public void Protocol_Should_Handle_CancelInvocationMessage_With_Header(params string[] kvp)
    {
        var binder = new Mock<IInvocationBinder>();

        var protobufHubProtocol = new ProtobufHubProtocol();
        var writer = new CommunityToolkit.HighPerformance.Buffers.MemoryBufferWriter<byte>(new Memory<byte>(new byte[100000]));

        var headers = kvp.ToHeadersDictionary();
        var invocationId = "123";

        var cancelInvocationMessage = new CancelInvocationMessage(invocationId)
        {
            Headers = headers
        };

        protobufHubProtocol.WriteMessage(cancelInvocationMessage, writer);
        var encodedMessage = new ReadOnlySequence<byte>(writer.WrittenSpan.ToArray());
        var result = protobufHubProtocol.TryParseMessage(ref encodedMessage, binder.Object, out var resultCancelInvocationMessage);

        result.Should().Be(true);

        resultCancelInvocationMessage
            .Should()
            .NotBeNull()
            .And
            .BeOfType<CancelInvocationMessage>()
            .Subject.Should()
            .BeEquivalentTo(cancelInvocationMessage);
    }
}