using CommunityToolkit.HighPerformance;
using FluentAssertions;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Moq;
using SignalP.Protocol.Protobuf.UnitTests.Helpers;
using System.Buffers;
using Xunit;

namespace SignalP.Protocol.Protobuf.UnitTests;

public class CompletionMessageTests
{
    [
        Theory,
        InlineData("3"),
        InlineData("123"),
        InlineData("9876543210123456789"),
        InlineData("##############!!!!!!!!!!!$$$$$$$$$$$$$$^^^^^^^^^^^^^^^***********")
    ]
    public void Protocol_Should_Handle_CompletionMessage_Without_Result_Or_Error(string invocationId)
    {
        var binder = new Mock<IInvocationBinder>();
        var protobufHubProtocol = new ProtobufHubProtocol();

        var writer = new CommunityToolkit.HighPerformance.Buffers.MemoryBufferWriter<byte>(new Memory<byte>(new byte[100000]));

        var completionMessage = new CompletionMessage(invocationId, null, null, false);

        protobufHubProtocol.WriteMessage(completionMessage, writer);
        var encodedMessage = new ReadOnlySequence<byte>(writer.WrittenSpan.ToArray());
        var result = protobufHubProtocol.TryParseMessage(ref encodedMessage, binder.Object, out var resultCompletionMessage);

        result.Should().BeTrue();

        resultCompletionMessage.Should().NotBeNull().And.BeOfType<CompletionMessage>().Subject.Should().BeEquivalentTo(completionMessage);
    }

    [
        Theory,
        InlineData("Some Error"),
        InlineData("Some bad stuff happened"),
        InlineData("Grrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrr"),
        InlineData("##############!!!!!!!!!!!$$$$$$$$$$$$$$^^^^^^^^^^^^^^^***********")
    ]
    public void Protocol_Should_Handle_CompletionMessage_With_An_Error(string error)
    {
        var binder = new Mock<IInvocationBinder>();
        var protobufHubProtocol = new ProtobufHubProtocol();

        var writer = new CommunityToolkit.HighPerformance.Buffers.MemoryBufferWriter<byte>(new Memory<byte>(new byte[100000]));

        var completionMessage = new CompletionMessage("123", error, null, false);

        protobufHubProtocol.WriteMessage(completionMessage, writer);
        var encodedMessage = new ReadOnlySequence<byte>(writer.WrittenSpan.ToArray());
        var result = protobufHubProtocol.TryParseMessage(ref encodedMessage, binder.Object, out var resultCompletionMessage);

        result.Should().BeTrue();

        resultCompletionMessage.Should().NotBeNull().And.BeOfType<CompletionMessage>().Subject.Should().BeEquivalentTo(completionMessage);
    }

    [
        Theory,
        InlineData("key", "value"),
        InlineData("foo", "bar", "2048", "4096"),
        InlineData("@21", "fdf", "g123", "aloha", "42", "28")
    ]
    public void Protocol_Should_Handle_CompletionMessage_With_Headers_And_Error(params string[] kvp)
    {
        var binder = new Mock<IInvocationBinder>();
        var protobufHubProtocol = new ProtobufHubProtocol();

        var writer = new CommunityToolkit.HighPerformance.Buffers.MemoryBufferWriter<byte>(new Memory<byte>(new byte[100000]));

        var headers = kvp.ToHeadersDictionary();

        var completionMessage = new CompletionMessage("123", "Error", null, false)
        {
            Headers = headers
        };

        protobufHubProtocol.WriteMessage(completionMessage, writer);
        var encodedMessage = new ReadOnlySequence<byte>(writer.WrittenSpan.ToArray());
        var result = protobufHubProtocol.TryParseMessage(ref encodedMessage, binder.Object, out var resultCompletionMessage);

        result.Should().BeTrue();

        resultCompletionMessage.Should().NotBeNull().And.BeOfType<CompletionMessage>().Subject.Should().BeEquivalentTo(completionMessage);
    }

    [
        Theory,
        MemberData(nameof(GetData))
    ]
    public void Protocol_Should_Handle_CompletionMessage_With_Result(object completionResult)
    {
        var invocationId = "123";
        var binder = new Mock<IInvocationBinder>(MockBehavior.Strict);
        binder.Setup(invocationBinder => invocationBinder.GetReturnType(invocationId)).Returns(completionResult.GetType());
        var protobufHubProtocol = new ProtobufHubProtocol();

        var writer = new CommunityToolkit.HighPerformance.Buffers.MemoryBufferWriter<byte>(new Memory<byte>(new byte[100000]));

        var completionMessage = new CompletionMessage(invocationId, null, completionResult, true);

        protobufHubProtocol.WriteMessage(completionMessage, writer);
        var encodedMessage = new ReadOnlySequence<byte>(writer.WrittenSpan.ToArray());
        var result = protobufHubProtocol.TryParseMessage(ref encodedMessage, binder.Object, out var resultCompletionMessage);

        result.Should().BeTrue();

        resultCompletionMessage.Should().NotBeNull().And.BeOfType<CompletionMessage>().Subject.Should().BeEquivalentTo(completionMessage);
        binder.VerifyAll();
    }

    [Fact]
    public void Invocation_With_Unsupported_Result_Type_Produce_Exception_On_Write()
    {
        var invocationId = "123";
        var binder = new Mock<IInvocationBinder>(MockBehavior.Strict);
        var protobufHubProtocol = new ProtobufHubProtocol();

        var writer = new CommunityToolkit.HighPerformance.Buffers.MemoryBufferWriter<byte>(new Memory<byte>(new byte[100000]));

        var completionMessage = new CompletionMessage(invocationId, null, DateTimeOffset.UtcNow, true);

        var call = () => protobufHubProtocol.WriteMessage(completionMessage, writer);

        call.Should().Throw<InvalidDataException>("{0} is not supported type", completionMessage.Result!.GetType().Name);

        binder.VerifyAll();
    }

    [Fact]
    public void Invocation_With_Unsupported_Result_Type_Produce_False_On_Read()
    {
        var invocationId = "123";
        var binder = new Mock<IInvocationBinder>(MockBehavior.Strict);
        binder.Setup(invocationBinder => invocationBinder.GetReturnType(invocationId)).Returns(typeof(DateTimeOffset));

        var protobufHubProtocol = new ProtobufHubProtocol();

        var writer = new CommunityToolkit.HighPerformance.Buffers.MemoryBufferWriter<byte>(new Memory<byte>(new byte[100000]));

        var protoMessage = new ProtobufProtocolMessage
        {
            Completion = new Completion
            {
                InvocationId = invocationId,
                Item = new Argument
                {
                    Proto = new Any()
                }
            }
        };

        using var stream = writer.AsStream();
        protoMessage.WriteDelimitedTo(stream);

        var encodedMessage = new ReadOnlySequence<byte>(writer.WrittenSpan.ToArray());
        var result = protobufHubProtocol.TryParseMessage(ref encodedMessage, binder.Object, out _);

        result.Should().BeFalse();

        binder.VerifyAll();
    }

    public static IEnumerable<object[]> GetData()
    {
        return new List<object[]>
        {
            new object[] { 0 },
            new object[] { "hello" },
            new object[] { new[] { "hello", "world" } },
            new object[] { DateTime.UtcNow },
            new object[] { new Dictionary<string, string> { { "hello?", "no!" } } },
            new object[] { new Completion { InvocationId = "321" } },
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
                }
            }
        };
    }
}