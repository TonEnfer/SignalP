namespace SignalP.Tools.ProtocPlugin.Csharp.UnitTests.Helpers;

[AttributeUsage(AttributeTargets.Assembly)]
public class ProtobufCompilerAttribute(string path): Attribute
{
    public string Path { get; } = path;
}