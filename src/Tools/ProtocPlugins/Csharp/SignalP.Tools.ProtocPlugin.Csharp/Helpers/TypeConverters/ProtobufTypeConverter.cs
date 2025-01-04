using Google.Protobuf.Reflection;

namespace SignalP.Tools.ProtocPlugin.Csharp.Helpers.TypeConverters;

internal class ProtobufTypeConverter: ITypeConverter
{
    #region ITypeConverter

    public string? GetTypeName(MessageDescriptor type)
    {
        return type.GetFullTypeName();
    }

    #endregion
}