using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Moq;
using System.Buffers;
using Xunit;

namespace SignalP.Protocol.Protobuf.UnitTests;

public class PingMessageTests
{
    [Fact]
    public void Protocol_Should_Handle_PingMessage()
    {
        var binder = new Mock<IInvocationBinder>();
        var protobufHubProtocol = new ProtobufHubProtocol();
        var writer = new CommunityToolkit.HighPerformance.Buffers.MemoryBufferWriter<byte>(new Memory<byte>(new byte[10000]));

        protobufHubProtocol.WriteMessage(PingMessage.Instance, writer);
        var encodedMessage = new ReadOnlySequence<byte>(writer.WrittenSpan.ToArray());
        var result = protobufHubProtocol.TryParseMessage(ref encodedMessage, binder.Object, out var resultPingMessage);

        result.Should().BeTrue();
        resultPingMessage.Should().NotBeNull().And.BeOfType<PingMessage>().And.Be(PingMessage.Instance);
    }
}