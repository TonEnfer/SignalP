using Google.Protobuf;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Nerdbank.Streams;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace SignalP.Protocol.Protobuf;

public class ProtobufHubProtocol: IHubProtocol
{
    private const string PROTOCOL_NAME = "protobuf";
    private const int PROTOCOL_VERSION = 1;

    public string Name => PROTOCOL_NAME;

    public int Version => PROTOCOL_VERSION;

    public TransferFormat TransferFormat => TransferFormat.Binary;

    #region IHubProtocol

    public bool TryParseMessage(ref ReadOnlySequence<byte> input, IInvocationBinder binder, [NotNullWhen(true)] out HubMessage? message)
    {
        message = null;

        if (input.IsEmpty)
        {
            return false;
        }

        try
        {
            using var stream = input.AsStream();

            message = ProtobufProtocolMessage.Parser.ParseDelimitedFrom(stream).MapToHubMessage(binder);

            input = input.Slice(stream.Position);

            return true;
        }
        catch (Exception)
        {
            input = input.Slice(input.End);
            return false;
        }
    }

    public void WriteMessage(HubMessage message, IBufferWriter<byte> output)
    {
        var protocolMessage = message.MapToProtobufProtocolMessage();
        using var outputStream = output.AsStream();
        protocolMessage.WriteDelimitedTo(outputStream);
    }

    public ReadOnlyMemory<byte> GetMessageBytes(HubMessage message)
    {
        var protocolMessage = message.MapToProtobufProtocolMessage();

        var size = protocolMessage.CalculateSize();
        var array = new byte[size + CodedOutputStream.ComputeInt32Size(size)];

        using var stream = new MemoryStream(array);

        protocolMessage.WriteDelimitedTo(stream);

        return new ReadOnlyMemory<byte>(array);
    }

    public bool IsVersionSupported(int version)
    {
        return version <= Version;
    }

    #endregion
}