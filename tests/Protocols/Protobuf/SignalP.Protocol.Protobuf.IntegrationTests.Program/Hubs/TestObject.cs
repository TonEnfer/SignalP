using ProtoBuf;

namespace SignalP.Protocol.Protobuf.IntegrationTests.Program.Hubs;

[ProtoContract]
public class TestObject
{
    [ProtoMember(1)]
    public int Field1 { get; set; } = 42;

    [ProtoMember(2)]
    public string Field2 { get; set; } = string.Empty;

    [ProtoMember(3)]
    public DateTime Field3 { get; set; } = DateTime.MaxValue;

    [ProtoMember(4)]
    public string[] Field4 { get; set; } = [];

    [ProtoMember(5)]
    public TestObject? Field5 { get; set; }
}