using Google.Protobuf.Reflection;

namespace SignalP.Tools.ProtocPlugin.Csharp.Helpers;

internal static class MessageDescriptorExtensions
{
    private static readonly HashSet<string> WellKnownTypeNames = new()
    {
        "google/protobuf/any.proto",
        "google/protobuf/api.proto",
        "google/protobuf/duration.proto",
        "google/protobuf/empty.proto",
        "google/protobuf/wrappers.proto",
        "google/protobuf/timestamp.proto",
        "google/protobuf/field_mask.proto",
        "google/protobuf/source_context.proto",
        "google/protobuf/struct.proto",
        "google/protobuf/type.proto"
    };

    public static bool IsAnyType(this MessageDescriptor descriptor)
    {
        return descriptor._isInGoogleProtobufPackage() && descriptor._isFileNameEqual("google/protobuf/any.proto");
    }

    public static bool IsEmptyType(this MessageDescriptor descriptor)
    {
        return descriptor._isInGoogleProtobufPackage() && descriptor._isFileNameEqual("google/protobuf/empty.proto");
    }

    public static bool IsWellKnownType(this MessageDescriptor descriptor)
    {
        return descriptor._isInGoogleProtobufPackage() && WellKnownTypeNames.Contains(descriptor.File.Name);
    }

    public static bool IsWrapperType(this MessageDescriptor descriptor)
    {
        return descriptor._isInGoogleProtobufPackage() && descriptor._isFileNameEqual("google/protobuf/wrappers.proto");
    }

    private static bool _isInGoogleProtobufPackage(this MessageDescriptor descriptor)
    {
        return descriptor.File.Package == "google.protobuf";
    }

    private static bool _isFileNameEqual(this MessageDescriptor descriptor, string fileName)
    {
        return descriptor.File.Name == fileName;
    }
}