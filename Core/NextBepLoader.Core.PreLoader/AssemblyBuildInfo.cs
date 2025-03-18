/*using System;
using AsmResolver.PE.File;
using AssemblyDefinition = AsmResolver.DotNet.AssemblyDefinition;

namespace NextBepLoader.Core.PreLoader;

public class AssemblyBuildInfo
{
    public enum FrameworkType
    {
        Unknown,
        NetFramework,
        NetStandard,
        NetCore
    }

    public Version NetFrameworkVersion { get; private set; }

    public bool IsAnyCpu { get; set; }

    public bool Is64Bit { get; set; }

    public FrameworkType AssemblyFrameworkType { get; set; }

    /*private void SetNet4Version(AssemblyDefinition assemblyDefinition)
    {
        NetFrameworkVersion = new Version(0, 0);
        AssemblyFrameworkType = FrameworkType.Unknown;

        var targetFrameworkAttribute = assemblyDefinition.CustomAttributes.FirstOrDefault(x =>
                     x.Constructor?.FullName == "System.Runtime.Versioning.TargetFrameworkAttribute");

        if (targetFrameworkAttribute == null)
            return;

        if (targetFrameworkAttribute.Signature?.NamedArguments.Count < 1)
            return;

        if (targetFrameworkAttribute.Signature?.NamedArguments[0].ArgumentType.Name != "String")
            return;

        var versionInfo = (string)targetFrameworkAttribute.Signature.NamedArguments[0].Argument.Element!;

        var values = versionInfo.Split(',');


        foreach (var value in values)
            if (value.StartsWith(".NET"))
                AssemblyFrameworkType = value switch
                {
                    ".NETFramework" => FrameworkType.NetFramework,
                    ".NETCoreApp"   => FrameworkType.NetCore,
                    ".NETStandard"  => FrameworkType.NetStandard,
                    _               => FrameworkType.Unknown
                };
            else if (value.StartsWith("Version=v"))
                try
                {
                    NetFrameworkVersion = new Version(value.Substring("Version=v".Length));
                }
                catch { }
    }#1#

    public static AssemblyBuildInfo DetermineInfo(AssemblyDefinition assemblyDefinition)
    {
        var buildInfo = new AssemblyBuildInfo();
        var module = assemblyDefinition.ManifestModule!;

        // framework version

        var runtime = module.RuntimeContext.TargetRuntime;

        buildInfo.AssemblyFrameworkType = FrameworkType.Unknown;

        if (runtime.IsNetFramework)
            buildInfo.AssemblyFrameworkType = FrameworkType.NetFramework;
        if (runtime.IsNetStandard)
            buildInfo.AssemblyFrameworkType = FrameworkType.NetStandard;
        if (runtime.IsNetCoreApp)
            buildInfo.AssemblyFrameworkType = FrameworkType.NetCore;

        buildInfo.NetFrameworkVersion = runtime.Version;


        // bitness

        /*
            AnyCPU 64-bit preferred
            MainModule.Architecture: I386
            MainModule.Attributes: ILOnly

            AnyCPU 32-bit preferred
            MainModule.Architecture: I386
            MainModule.Attributes: ILOnly, Required32Bit, Preferred32Bit

            x86
            MainModule.Architecture: I386
            MainModule.Attributes: ILOnly, Required32Bit

            x64
            MainModule.Architecture: AMD64
            MainModule.Attributes: ILOnly
        #1#

        var architecture = module.MachineType;

        if (architecture.HasFlag(MachineType.Amd64))
        {
            buildInfo.Is64Bit = true;
            buildInfo.IsAnyCpu = false;
        }
        else if (architecture.HasFlag(MachineType.I386) && module.IsBit32Preferred && module.IsBit32Required)
        {
            buildInfo.Is64Bit = false;
            buildInfo.IsAnyCpu = true;
        }
        else if (architecture.HasFlag(MachineType.I386) && module.IsBit32Required)
        {
            buildInfo.Is64Bit = false;
            buildInfo.IsAnyCpu = false;
        }
        else if (architecture.HasFlag(MachineType.I386))
        {
            buildInfo.Is64Bit = true;
            buildInfo.IsAnyCpu = true;
        }
        else
        {
            throw new Exception("Unable to determine assembly architecture");
        }

        return buildInfo;
    }

    /*
    private static bool HasFlag(ModuleAttributes value, ModuleAttributes flag) => (value & flag) == flag;

    /// <inheritdoc />
    public override string ToString()
    {
        var frameworkType = AssemblyFrameworkType switch
        {
            FrameworkType.NetFramework => "Framework",
            FrameworkType.NetStandard  => "Standard",
            FrameworkType.NetCore      => "Core",
            FrameworkType.Unknown      => "Unknown",
            var _                      => throw new ArgumentOutOfRangeException()
        };

        if (IsAnyCpu)
            return $".NET {frameworkType} {NetFrameworkVersion}, AnyCPU ({(Is64Bit ? "64" : "32")}-bit preferred)";

        return $".NET {frameworkType} {NetFrameworkVersion}, {(Is64Bit ? "x64" : "x86")}";
    }#1#
}*/
