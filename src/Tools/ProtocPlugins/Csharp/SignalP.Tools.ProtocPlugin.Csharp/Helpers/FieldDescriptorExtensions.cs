using Google.Protobuf.Reflection;

namespace SignalP.Tools.ProtocPlugin.Csharp.Helpers;

internal static class FieldDescriptorExtensions
{
    private const string DICTIONARY_STRING_TEMPLATE = "global::System.Collections.Generic.Dictionary<{0}, {1}>";
    private const string LIST_STRING_TEMPLATE = "global::System.Collections.Generic.List<{0}>";

    public static string GetClrTypeName(this FieldDescriptor fieldDescriptor)
    {
        var type = fieldDescriptor.FieldType switch
        {
            FieldType.Double => "double",
            FieldType.Float => "float",
            FieldType.SFixed64 or FieldType.SInt64 or FieldType.Int64 => "long",
            FieldType.Fixed64 or FieldType.UInt64 => "ulong",
            FieldType.SFixed32 or FieldType.SInt32 or FieldType.Int32 => "int",
            FieldType.Bool => "bool",
            FieldType.String => "string",
            FieldType.Message when fieldDescriptor.IsMap => string.Format(
                DICTIONARY_STRING_TEMPLATE,
                fieldDescriptor.MessageType.FindFieldByNumber(1).GetClrTypeName(),
                fieldDescriptor.MessageType.FindFieldByNumber(2).GetClrTypeName()
            ),
            FieldType.Message => fieldDescriptor.MessageType.GetFullTypeName(),
            FieldType.Bytes => "byte[]",
            FieldType.Fixed32 or FieldType.UInt32 => "uint",
            FieldType.Enum => fieldDescriptor.EnumType.GetFullTypeName(),
            FieldType.Group or _ =>
                throw new ArgumentOutOfRangeException()
        };

        return fieldDescriptor is { IsRepeated: true, IsMap: false } ? string.Format(LIST_STRING_TEMPLATE, type) : type;
    }
}