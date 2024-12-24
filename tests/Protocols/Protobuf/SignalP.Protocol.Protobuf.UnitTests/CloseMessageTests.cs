using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Moq;
using System.Buffers;
using Xunit;

namespace SignalP.Protocol.Protobuf.UnitTests;

public class CloseMessageTests
{
    [
        Theory,
        InlineData("Some Error", true),
        InlineData("Some bad stuff happened", false),
        InlineData("Grrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrr", true),
        InlineData("##############!!!!!!!!!!!$$$$$$$$$$$$$$^^^^^^^^^^^^^^^***********", false)
    ]
    public void Protocol_Should_Handle_Close_Message(string error, bool allowReconnect)
    {
        var binder = new Mock<IInvocationBinder>();
        var protobufHubProtocol = new ProtobufHubProtocol();

        var writer = new CommunityToolkit.HighPerformance.Buffers.MemoryBufferWriter<byte>(new Memory<byte>(new byte[100000]));
        var closeMessage = new CloseMessage(error, allowReconnect);

        protobufHubProtocol.WriteMessage(closeMessage, writer);
        var encodedMessage = new ReadOnlySequence<byte>(writer.WrittenSpan.ToArray());
        var result = protobufHubProtocol.TryParseMessage(ref encodedMessage, binder.Object, out var resultCloseMessage);

        result.Should().BeTrue();
        resultCloseMessage.Should().NotBeNull().And.BeOfType<CloseMessage>().Subject.Should().BeEquivalentTo(closeMessage);
    }
}