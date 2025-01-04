using System.CodeDom.Compiler;

namespace SignalP.Tools.ProtocPlugin.Csharp.Helpers;

internal static class TextWriterExtensions
{
    public static void Indent(this IndentedTextWriter writer)
    {
        writer.Indent += 4;
    }

    public static void Outdent(this IndentedTextWriter writer)
    {
        writer.Indent -= 4;
    }

    public static void WriteObsoleteAttribute(this TextWriter writer, bool obsolete)
    {
        if (obsolete)
        {
            writer.WriteLine("[global::System.ObsoleteAttribute]");
        }
    }

    public static void WriteGeneratedCodeAttribute(this TextWriter writer)
    {
        writer.WriteLine("""[global::System.CodeDom.Compiler.GeneratedCode("signalp_csharp_plugin", null)]""");
    }

    public static void WriteDebuggerNonUserCodeAttribute(this TextWriter writer)
    {
        writer.WriteLine("[global::System.Diagnostics.DebuggerNonUserCodeAttribute]");
    }
}