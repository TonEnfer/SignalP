using System.Runtime.InteropServices;

namespace SignalP.Tools.Utils;

internal static class PlatformDetection
{
    public enum OperationSystem
    {
        Unknown,
        Windows,
        Linux,
        MacOSX
    };

    public static OperationSystem GetOsPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return OperationSystem.Windows;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return OperationSystem.Linux;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return OperationSystem.MacOSX;
        }

        return OperationSystem.Unknown;
    }

    public static Architecture GetArchitecture()
    {
        return RuntimeInformation.ProcessArchitecture;
    }
}