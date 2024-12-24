using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Moq;
using SignalP.Protocol.Protobuf.UnitTests.Helpers;
using System.Buffers;
using Xunit;

namespace SignalP.Protocol.Protobuf.UnitTests;

public class StreamInvocationMessageTests
{
    [
        Theory,
        InlineData("3"),
        InlineData("123"),
        InlineData("9876543210123456789"),
        InlineData("##############!!!!!!!!!!!$$$$$$$$$$$$$$^^^^^^^^^^^^^^^***********")
    ]
    public void Protocol_Should_Handle_StreamInvocation_Message_Without_Parameters_And_Headers_With_InvocationId(string invocationId)
    {
        var binder = new Mock<IInvocationBinder>(MockBehavior.Strict);
        binder.Setup(x => x.GetParameterTypes("target")).Returns([]);
        var protobufHubProtocol = new ProtobufHubProtocol();

        var writer = new CommunityToolkit.HighPerformance.Buffers.MemoryBufferWriter<byte>(new Memory<byte>(new byte[100000]));

        var completionMessage = new StreamInvocationMessage(invocationId, "target", []);

        protobufHubProtocol.WriteMessage(completionMessage, writer);
        var encodedMessage = new ReadOnlySequence<byte>(writer.WrittenSpan.ToArray());
        var result = protobufHubProtocol.TryParseMessage(ref encodedMessage, binder.Object, out var resultCompletionMessage);

        result.Should().BeTrue();

        resultCompletionMessage.Should().NotBeNull().And.BeOfType<StreamInvocationMessage>().Subject.Should().BeEquivalentTo(completionMessage);

        binder.VerifyAll();
    }

    [
        Theory,
        InlineData("12"),
        InlineData("%#^^D\t\b\n\r", "hello"),
        InlineData("%#^^D\t\b\n\r", "hello", "????????????????????????????%#$#??????????????")
    ]
    public void Protocol_Should_Handle_StreamInvocation_Message_With_StreamIds(params string[] streamIds)
    {
        var binder = new Mock<IInvocationBinder>(MockBehavior.Strict);
        binder.Setup(x => x.GetParameterTypes("target")).Returns(streamIds.Select(x => x.GetType()).ToList);
        var protobufHubProtocol = new ProtobufHubProtocol();

        var writer = new CommunityToolkit.HighPerformance.Buffers.MemoryBufferWriter<byte>(new Memory<byte>(new byte[100000]));

        var completionMessage = new StreamInvocationMessage("invId", "target", [], streamIds)
        {
            Headers = new Dictionary<string, string>
            {
                { "%#^^D\t\b\n\r", "%#^^D\t\b\n\r321" },
                { "Header", "HeaderValue" }
            }
        };

        protobufHubProtocol.WriteMessage(completionMessage, writer);
        var encodedMessage = new ReadOnlySequence<byte>(writer.WrittenSpan.ToArray());
        var result = protobufHubProtocol.TryParseMessage(ref encodedMessage, binder.Object, out var resultCompletionMessage);

        result.Should().BeTrue();

        resultCompletionMessage.Should().NotBeNull().And.BeOfType<StreamInvocationMessage>().Subject.Should().BeEquivalentTo(completionMessage);

        binder.VerifyAll();
    }

    [
        Theory,
        MemberData(nameof(GetData))
    ]
    public void Protocol_Should_Handle_StreamInvocation_Message_With_Parameters_And_Headers_And_InvocationId(object[] data)
    {
        var binder = new Mock<IInvocationBinder>(MockBehavior.Strict);
        binder.Setup(x => x.GetParameterTypes("target")).Returns(data.Select(x => x.GetType()).ToList);
        var protobufHubProtocol = new ProtobufHubProtocol();

        var writer = new CommunityToolkit.HighPerformance.Buffers.MemoryBufferWriter<byte>(new Memory<byte>(new byte[100000]));

        var completionMessage = new StreamInvocationMessage("invId", "target", data)
        {
            Headers = new Dictionary<string, string>
            {
                { "%#^^D\t\b\n\r", "%#^^D\t\b\n\r321" },
                { "Header", "HeaderValue" }
            }
        };

        protobufHubProtocol.WriteMessage(completionMessage, writer);
        var encodedMessage = new ReadOnlySequence<byte>(writer.WrittenSpan.ToArray());
        var result = protobufHubProtocol.TryParseMessage(ref encodedMessage, binder.Object, out var resultCompletionMessage);

        result.Should().BeTrue();

        resultCompletionMessage.Should().NotBeNull().And.BeOfType<StreamInvocationMessage>().Subject.Should().BeEquivalentTo(completionMessage);

        binder.VerifyAll();
    }

    public static TheoryData<object[]> GetData()
    {
        return
        [
            new object[] { 0 },
            new object[] { "123" },
            new object[] { 0, "123" },
            new object[] { new Ping(), "%#^^D\t\b\n\r" },
            new object[] { new Ping(), "%#^^D\t\b\n\r" },
            new object[] { new List<string> { "123", "321", "555" } },
            new object[]
            {
                new TestObject
                {
                    Field1 = 12,
                    Field2 = "test",
                    Field3 = DateTime.Parse("2020-02-12T12:32:15Z"),
                    Field4 = new[] { "test1", "test2" },
                    Field5 = new TestObject
                    {
                        Field1 = 21,
                        Field2 = "?????",
                        Field3 = DateTime.Parse("2045-02-12T12:32:15Z"),
                        Field4 = new[] { "test2", "test8" }
                    }
                },
                new Ping(),
                DateTime.UtcNow
            }
        ];
    }
}