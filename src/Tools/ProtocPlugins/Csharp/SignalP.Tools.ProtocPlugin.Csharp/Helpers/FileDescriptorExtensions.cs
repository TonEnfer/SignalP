using Google.Protobuf.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace SignalP.Tools.ProtocPlugin.Csharp.Helpers;

internal static class FileDescriptorExtensions
{
    public static List<string> GetFileComments(this FileDescriptor file)
    {
        var sourceLocation = file
            .ToProto()
            .SourceCodeInfo.Location
            .FirstOrDefault(x => x.Path.SequenceEqual([FileDescriptorProto.SyntaxFieldNumber]));

        return sourceLocation == null
            ? []
            : sourceLocation
                .LeadingComments.Split('\n')
                .Concat(sourceLocation.LeadingDetachedComments.SelectMany(x => x.Split("\n")))
                .ToList();
    }

    public static bool TryGetNamespace(this FileDescriptor file, [NotNullWhen(true)] out string? @namespace)
    {
        @namespace = !string.IsNullOrWhiteSpace(file.GetOptions()?.CsharpNamespace) ? file.GetOptions()?.CsharpNamespace : file.Package;

        return !string.IsNullOrWhiteSpace(@namespace);
    }
}