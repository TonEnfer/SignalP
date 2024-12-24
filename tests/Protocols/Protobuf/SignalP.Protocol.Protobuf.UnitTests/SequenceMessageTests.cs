#if NET7_0_OR_GREATER || NETSTANDARD || NETFRAMEWORK
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Moq;
using System.Buffers;
using Xunit;

namespace SignalP.Protocol.Protobuf.UnitTests;

public class SequenceMessageTests
{
    [Fact]
    public void Protocol_Should_Handle_Sequence_Message()
    {
        var binder = new Mock<IInvocationBinder>();
        var protobufHubProtocol = new ProtobufHubProtocol();
        var writer = new CommunityToolkit.HighPerformance.Buffers.MemoryBufferWriter<byte>(new Memory<byte>(new byte[10000]));
        var message = new SequenceMessage(150);

        protobufHubProtocol.WriteMessage(message, writer);

        var encodedMessage = new ReadOnlySequence<byte>(writer.WrittenSpan.ToArray());
        var result = protobufHubProtocol.TryParseMessage(ref encodedMessage, binder.Object, out var resultAckMessage);

        result.Should().BeTrue();
        resultAckMessage.Should().NotBeNull().And.BeOfType<SequenceMessage>().Subject.Should().BeEquivalentTo(message);
    }
}

#endif