using System.Reflection;
using System.Runtime.InteropServices;

namespace SignalP.Tools.ProtocPlugin.Csharp.UnitTests.Helpers;

internal static class ProtobufCompiler
{
    public static string GetCompilerPath()
    {
        var toolsPath = Assembly.GetExecutingAssembly().GetCustomAttribute<ProtobufCompilerAttribute>()!.Path;

        var os = _getOsPlatform();

        var cpu = _getArchitecture() switch
        {
            Architecture.X86 => "x86",
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            _ => string.Empty
        };

        if (cpu.Equals("arm64"))
        {
            cpu = os switch
            {
                "macosx" => "x64",
                "windows" => "x86",
                _ => cpu
            };
        }

        var compilerDirectory = $"{toolsPath}{os}_{cpu}";

        if (!Directory.Exists(compilerDirectory))
        {
            throw new PlatformNotSupportedException();
        }

        return $"{compilerDirectory}{Path.DirectorySeparatorChar}protoc{(os.Equals("windows") ? ".exe" : string.Empty)}";
    }

    private static string _getOsPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "windows";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "linux";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "macosx";
        }

        throw new PlatformNotSupportedException();
    }

    private static Architecture _getArchitecture()
    {
        return RuntimeInformation.ProcessArchitecture;
    }
}