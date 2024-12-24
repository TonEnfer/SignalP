using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Moq;
using SignalP.Protocol.Protobuf.UnitTests.Helpers;
using System.Buffers;
using Xunit;

namespace SignalP.Protocol.Protobuf.UnitTests;

public class StreamItemMessageTests
{
    [
        Theory,
        InlineData("3"),
        InlineData("123"),
        InlineData("9876543210123456789"),
        InlineData("##############!!!!!!!!!!!$$$$$$$$$$$$$$^^^^^^^^^^^^^^^***********")
    ]
    public void Protocol_Should_Handle_StreamItem_Message_With_Null_Value(string streamId)
    {
        var binder = new Mock<IInvocationBinder>(MockBehavior.Strict);
        binder.Setup(x => x.GetStreamItemType(streamId)).Returns(typeof(DateTime?));
        var protobufHubProtocol = new ProtobufHubProtocol();

        var writer = new CommunityToolkit.HighPerformance.Buffers.MemoryBufferWriter<byte>(new Memory<byte>(new byte[100000]));

        var completionMessage = new StreamItemMessage(streamId, null);

        protobufHubProtocol.WriteMessage(completionMessage, writer);
        var encodedMessage = new ReadOnlySequence<byte>(writer.WrittenSpan.ToArray());
        var result = protobufHubProtocol.TryParseMessage(ref encodedMessage, binder.Object, out var resultCompletionMessage);

        result.Should().BeTrue();

        resultCompletionMessage.Should().NotBeNull().And.BeOfType<StreamItemMessage>().Subject.Should().BeEquivalentTo(completionMessage);

        binder.VerifyAll();
    }

    [
        Theory,
        InlineData("3", "3"),
        InlineData("123", "3232", "%%%??\n!!fdf", "header?")
    ]
    public void Protocol_Should_Handle_StreamItem_Message_With_Value_And_Headers(params string[] headers)
    {
        var streamId = "id";
        var binder = new Mock<IInvocationBinder>(MockBehavior.Strict);
        binder.Setup(x => x.GetStreamItemType(streamId)).Returns(typeof(DateTime?));
        var protobufHubProtocol = new ProtobufHubProtocol();

        var writer = new CommunityToolkit.HighPerformance.Buffers.MemoryBufferWriter<byte>(new Memory<byte>(new byte[100000]));

        var completionMessage = new StreamItemMessage(streamId, DateTime.UtcNow) { Headers = headers.ToHeadersDictionary() };

        protobufHubProtocol.WriteMessage(completionMessage, writer);
        var encodedMessage = new ReadOnlySequence<byte>(writer.WrittenSpan.ToArray());
        var result = protobufHubProtocol.TryParseMessage(ref encodedMessage, binder.Object, out var resultCompletionMessage);

        result.Should().BeTrue();

        resultCompletionMessage.Should().NotBeNull().And.BeOfType<StreamItemMessage>().Subject.Should().BeEquivalentTo(completionMessage);

        binder.VerifyAll();
    }
}