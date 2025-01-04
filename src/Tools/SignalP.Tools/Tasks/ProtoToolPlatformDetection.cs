using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SignalP.Tools.Utils;
using System.Runtime.InteropServices;

namespace SignalP.Tools.Tasks;

public class ProtoToolPlatformDetection: Task
{
    /// <summary>
    ///     Return one of 'linux', 'macosx' or 'windows'.
    ///     If the OS is unknown, the property is not set.
    /// </summary>
    [Output]
    public string Os { get; set; }

    /// <summary>
    ///     Return one of 'x64', 'x86', 'arm64'.
    ///     If the CPU is unknown, the property is not set.
    /// </summary>
    [Output]
    public string Cpu { get; set; }

    public override bool Execute()
    {
        Os = PlatformDetection.GetOsPlatform() switch
        {
            PlatformDetection.OperationSystem.Windows => "windows",
            PlatformDetection.OperationSystem.Linux => "linux",
            PlatformDetection.OperationSystem.MacOSX => "macosx",
            _ => string.Empty
        };

        Cpu = PlatformDetection.GetArchitecture() switch
        {
            Architecture.X86 => "x86",
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            _ => string.Empty
        };

        if (Cpu.Equals("arm64"))
        {
            Cpu = Os switch
            {
                "macosx" => "x64",
                "windows" => "x86",
                _ => Cpu
            };
        }

        return true;
    }
}