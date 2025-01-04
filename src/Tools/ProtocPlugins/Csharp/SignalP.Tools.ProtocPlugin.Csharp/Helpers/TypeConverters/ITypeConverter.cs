using Google.Protobuf.Reflection;

namespace SignalP.Tools.ProtocPlugin.Csharp.Helpers.TypeConverters;

internal interface ITypeConverter
{
    string? GetTypeName(MessageDescriptor type);
}