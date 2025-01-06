using Google.Protobuf.Reflection;
using System.Text;

namespace SignalP.Tools.ProtocPlugin.Csharp.Helpers;

internal static class DescriptorExtensions
{
    public static string GetFullTypeName(this DescriptorBase descriptor)
    {
        var typePathSegments = descriptor.FullName.Split('.');

        var strBuilder = new StringBuilder(typePathSegments.Length);

        foreach (var se in typePathSegments.SkipLast(1))
        {
            strBuilder.Append($"{se}.Types.");
        }

        strBuilder.Append(typePathSegments.Last());

        return descriptor.File.TryGetNamespace(out var @namespace)
            ? $"global::{@namespace}.{strBuilder}"
            : $"global::{strBuilder}";
    }
}