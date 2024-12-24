using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using ProtoBuf;
using System.Collections.Concurrent;
using System.Reflection;

namespace SignalP.Protocol.Protobuf;

internal static class MessageMapper
{
    private static readonly MethodInfo DeserializeMethod = typeof(Serializer)
        .GetMethods()
        .First(
            x => x.Name is nameof(Serializer.Deserialize)
                 && x.GetParameters() is { Length: 3 } param
                 && param[0].ParameterType == typeof(ReadOnlyMemory<byte>)
                 && param[1].ParameterType.IsGenericParameter
                 && param[2].ParameterType == typeof(object)
        );

    private static readonly ConcurrentDictionary<System.Type, MethodInfo> DeserializeMethodCache = [];

    private static readonly ProtobufProtocolMessage ProtocolPingMessage = new()
    {
        Ping = new Ping()
    };

    public static HubMessage MapToHubMessage(this ProtobufProtocolMessage proto, IInvocationBinder binder)
    {
        return proto.MessageCase switch
        {
            ProtobufProtocolMessage.MessageOneofCase.None => throw new InvalidDataException("Invalid message type"),
            ProtobufProtocolMessage.MessageOneofCase.Invocation => _getInvocationMessage(proto.Invocation, binder),
            ProtobufProtocolMessage.MessageOneofCase.StreamItem => _getStreamItemMessage(proto.StreamItem, binder),
            ProtobufProtocolMessage.MessageOneofCase.Completion => _getCompletionMessage(proto.Completion, binder),
            ProtobufProtocolMessage.MessageOneofCase.StreamInvocation => _getStreamInvocationMessage(proto.StreamInvocation, binder),
            ProtobufProtocolMessage.MessageOneofCase.CancelInvocation => _getCancelInvocationMessage(proto.CancelInvocation),
            ProtobufProtocolMessage.MessageOneofCase.Ping => PingMessage.Instance,
            ProtobufProtocolMessage.MessageOneofCase.Close => new CloseMessage(proto.Close.Error, proto.Close.AllowReconnect),
#if NET7_0_OR_GREATER || NETSTANDARD
            ProtobufProtocolMessage.MessageOneofCase.Ack => new AckMessage(proto.Ack.SequenceId),
            ProtobufProtocolMessage.MessageOneofCase.Sequence => new SequenceMessage(proto.Sequence.SequenceId),
#endif
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static ProtobufProtocolMessage MapToProtobufProtocolMessage(this HubMessage message)
    {
        // ProtoBuf.
        return message switch
        {
            InvocationMessage invocationMessage => new ProtobufProtocolMessage
            {
                Invocation = _getInvocationMessage(invocationMessage)
            },
            StreamInvocationMessage streamInvocationMessage => new ProtobufProtocolMessage
            {
                StreamInvocation = _getStreamInvocationMessage(streamInvocationMessage)
            },
            StreamItemMessage streamItemMessage => new ProtobufProtocolMessage
            {
                StreamItem = _getStreamItemMessage(streamItemMessage)
            },
            CompletionMessage completionMessage => new ProtobufProtocolMessage
            {
                Completion = _getCompletionMessage(completionMessage)
            },
            CancelInvocationMessage cancelInvocationMessage => new ProtobufProtocolMessage
            {
                CancelInvocation = _getCancelInvocationMessage(cancelInvocationMessage)
            },
            PingMessage => ProtocolPingMessage,
            CloseMessage closeMessage => new ProtobufProtocolMessage
            {
                Close = _getCloseMessage(closeMessage)
            },
#if NET7_0_OR_GREATER || NETSTANDARD
            AckMessage ackMessage => new ProtobufProtocolMessage
            {
                Ack = _getAckMessage(ackMessage)
            },
            SequenceMessage sequenceMessage => new ProtobufProtocolMessage
            {
                Sequence = _getSequenceMessage(sequenceMessage)
            },
#endif
            _ => throw new InvalidDataException($"Unexpected message type: {message.GetType().Name}")
        };
    }

    private static InvocationMessage _getInvocationMessage(Invocation invocation, IInvocationBinder binder)
    {
        var parameterTypes = binder.GetParameterTypes(invocation.Target);

        var arguments = invocation.Arguments.Select((argument, i) => _getArgumentValue(argument, parameterTypes[i])).ToArray();

        return new InvocationMessage(
            !string.IsNullOrWhiteSpace(invocation.InvocationId) ? invocation.InvocationId : null,
            invocation.Target,
            arguments,
            invocation.StreamIds.Count > 0 ? invocation.StreamIds.ToArray() : null
        ) { Headers = invocation.Headers.Count > 0 ? invocation.Headers : null };
    }

    private static StreamItemMessage _getStreamItemMessage(StreamItem streamItem, IInvocationBinder binder)
    {
        object? result;

        if (binder.GetStreamItemType(streamItem.InvocationId) is { } type && streamItem.Item is not null)
        {
            result = _getArgumentValue(streamItem.Item, type);
        }
        else
        {
            result = null;
        }

        return new StreamItemMessage(
            streamItem.InvocationId,
            result
        ) { Headers = streamItem.Headers.Count > 0 ? streamItem.Headers : null };
    }

    private static CompletionMessage _getCompletionMessage(Completion completion, IInvocationBinder binder)
    {
        object? result = null;

        try
        {
            if (binder.GetReturnType(completion.InvocationId) is { } type && completion.Item is not null)
            {
                result = _getArgumentValue(completion.Item, type);
            }
        }
        catch (InvalidOperationException)
        {
            try
            {
                if (binder.GetStreamItemType(completion.InvocationId) is { } type && completion.Item is not null)
                {
                    result = _getArgumentValue(completion.Item, type);
                }
            }
            catch (InvalidOperationException)
            {
                // ignore
            }
        }

        return new CompletionMessage(
            completion.InvocationId,
            string.IsNullOrWhiteSpace(completion.Error) ? null : completion.Error,
            result,
            completion.ResultCase is Completion.ResultOneofCase.Item
        ) { Headers = completion.Headers.Count > 0 ? completion.Headers : null };
    }

    private static StreamInvocationMessage _getStreamInvocationMessage(StreamInvocation streamInvocation, IInvocationBinder binder)
    {
        var parameterTypes = binder.GetParameterTypes(streamInvocation.Target);

        var arguments = streamInvocation
            .Arguments
            .Select((argument, i) => _getArgumentValue(argument, parameterTypes[i]))
            .ToArray();

        return new StreamInvocationMessage(
            streamInvocation.InvocationId,
            streamInvocation.Target,
            arguments,
            streamInvocation.StreamIds.Count > 0 ? streamInvocation.StreamIds.ToArray() : null
        )
        {
            Headers = streamInvocation.Headers.Count > 0 ? streamInvocation.Headers : null
        };
    }

    private static CancelInvocationMessage _getCancelInvocationMessage(CancelInvocation cancelInvocation)
    {
        return new CancelInvocationMessage(cancelInvocation.InvocationId)
            { Headers = cancelInvocation.Headers.Count > 0 ? cancelInvocation.Headers : null };
    }

    private static Invocation _getInvocationMessage(InvocationMessage invocationMessage)
    {
        return new Invocation
        {
            Headers = { invocationMessage.Headers ?? new Dictionary<string, string>() },
            Target = invocationMessage.Target,
            InvocationId = invocationMessage.InvocationId ?? string.Empty,
            StreamIds = { invocationMessage.StreamIds ?? [] },
            Arguments = { invocationMessage.Arguments.Select(_toArgument) }
        };
    }

    private static StreamInvocation _getStreamInvocationMessage(StreamInvocationMessage streamInvocationMessage)
    {
        return new StreamInvocation
        {
            Headers = { streamInvocationMessage.Headers ?? new Dictionary<string, string>() },
            InvocationId = streamInvocationMessage.InvocationId ?? string.Empty,
            Target = streamInvocationMessage.Target,
            StreamIds = { streamInvocationMessage.StreamIds ?? [] },
            Arguments = { streamInvocationMessage.Arguments.Select(_toArgument) }
        };
    }

    private static StreamItem _getStreamItemMessage(StreamItemMessage streamItemMessage)
    {
        return new StreamItem
        {
            Headers = { streamItemMessage.Headers ?? new Dictionary<string, string>() },
            InvocationId = streamItemMessage.InvocationId ?? string.Empty,
            Item = _toArgument(streamItemMessage.Item)
        };
    }

    private static Completion _getCompletionMessage(CompletionMessage completionMessage)
    {
        var message = new Completion
        {
            Headers = { completionMessage.Headers ?? new Dictionary<string, string>() },
            InvocationId = completionMessage.InvocationId ?? string.Empty
        };

        if (completionMessage.HasResult)
        {
            message.Item = _toArgument(completionMessage.Result);
        }
        else if (!string.IsNullOrWhiteSpace(completionMessage.Error))
        {
            message.Error = completionMessage.Error;
        }

        return message;
    }

    private static CancelInvocation _getCancelInvocationMessage(CancelInvocationMessage cancelInvocationMessage)
    {
        return new CancelInvocation
        {
            Headers = { cancelInvocationMessage.Headers ?? new Dictionary<string, string>() },
            InvocationId = cancelInvocationMessage.InvocationId
        };
    }

    private static Close _getCloseMessage(CloseMessage closeMessage)
    {
        return new Close
        {
            Error = closeMessage.Error,
            AllowReconnect = closeMessage.AllowReconnect
        };
    }

    private static Argument _toArgument(object? argument)
    {
        if (argument is IMessage message)
        {
            return new Argument { Proto = new Any { TypeUrl = message.Descriptor.FullName, Value = message.ToByteString() } };
        }

        try
        {
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, argument);
            stream.Seek(0, SeekOrigin.Begin);
            return new Argument { Proto = new Any { TypeUrl = argument?.GetType().Name ?? string.Empty, Value = ByteString.FromStream(stream) } };
        }
        catch (Exception e)
        {
            throw new InvalidDataException(
                $"Type {argument?.GetType().Name ?? nameof(Object)} cannot be serialized in Protobuf. Type must be a primitive type or a supported object for serialization",
                e
            );
        }
    }

    private static object? _getArgumentValue(Argument argument, System.Type expectedType)
    {
        if (typeof(IMessage).IsAssignableFrom(expectedType))
        {
            return (expectedType.GetProperty(nameof(Invocation.Parser))!.GetValue(null) as MessageParser)!.ParseFrom(argument.Proto.Value);
        }

        try
        {
            var method = DeserializeMethodCache.GetOrAdd(expectedType, type => DeserializeMethod.MakeGenericMethod(type));

            var data = method.Invoke(null, [argument.Proto.Value.Memory, default, null]);

            return data;
        }
        catch (Exception e)
        {
            throw new Exception($"Error while deserializing object with expected type {expectedType.Name}", e);
        }
    }

#if NET7_0_OR_GREATER || NETSTANDARD
    private static Ack _getAckMessage(AckMessage ackMessage)
    {
        return new Ack
        {
            SequenceId = ackMessage.SequenceId
        };
    }

    private static Sequence _getSequenceMessage(SequenceMessage sequenceMessage)
    {
        return new Sequence
        {
            SequenceId = sequenceMessage.SequenceId
        };
    }

#endif
}