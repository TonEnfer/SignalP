using Google.Protobuf.Reflection;

namespace SignalP.Tools.ProtocPlugin.Csharp.Helpers.TypeConverters;

internal abstract class TypeConverterBase: ITypeConverter
{
    #region ITypeConverter

    public string? GetTypeName(MessageDescriptor type)
    {
        if (type.IsEmptyType())
        {
            return null;
        }

        if (type.IsAnyType())
        {
            return GetAnyTypeName();
        }

        if (type.IsWrapperType())
        {
            return type.Name switch
            {
                "BoolValue" => "bool",
                "BytesValue" => "byte[]",
                "DoubleValue" => "double",
                "Duration" => $"global::System.{nameof(TimeSpan)}",
                "FloatValue" => "float",
                "Int32Value" => "int",
                "Int64Value" => "long",
                "StringValue" => "string",
                "Timestamp" => "global::System.DateTime",
                "UInt32Value" => "uint",
                "UInt64Value" => "ulong",
                "Value" => "object",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type.FullName, "Not supported well-known type")
            };
        }

        return type.GetFullTypeName();
    }

    #endregion

    protected virtual string GetAnyTypeName()
    {
        return "object";
    }
}