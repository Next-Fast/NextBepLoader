using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using NextBepLoader.Core.Utils;

namespace NextBepLoader.Core.PreLoader.NextPreLoaders;

public class ResolvePreLoad : BasePreLoader
{
    public static readonly IReadOnlyList<string> MangedDirectors =
    [
        Paths.CoreAssemblyPath,
        Paths.IL2CPPInteropAssemblyDirectory,
        Paths.UnityBaseDirectory,
        Paths.DependencyDirectory
    ];

    public static readonly IReadOnlyList<string> UnmanagedDirectors =
    [
        Paths.ManagedPath,
        Paths.UnityBaseDirectory,
        Paths.DependencyDirectory
    ];

    public List<Assembly> ResolvedAssemblies { get; } = [];
    public Dictionary<string, IntPtr> ResolvedUnmanagedDlls { get; } = new();


    public override PreLoadPriority Priority => PreLoadPriority.VeryLast;
    public AssemblyLoadContext LoadContext { get; private set; }

    public override void Start()
    {
        // Cecil 0.11 requires one to manually set up list of trusted assemblies for assembly resolving
        // The main BCL path
        AddCecilPlatformAssemblies(AppDomain.CurrentDomain, Paths.ManagedPath);
        // The parent path -> .NET has some extra managed DLLs in there
        AddCecilPlatformAssemblies(AppDomain.CurrentDomain, Path.GetDirectoryName(Paths.ManagedPath)!);
        LoadContext = AssemblyLoadContext.Default;
        LoadContext.Resolving += LocalResolve;
        LoadContext.ResolvingUnmanagedDll += LocalResolveUnmanaged;
    }

    private const string TRUSTED_PLATFORM_ASSEMBLIES = "TRUSTED_PLATFORM_ASSEMBLIES";
    private static void AddCecilPlatformAssemblies(AppDomain appDomain, string assemblyDir)
    {
        if (!Directory.Exists(assemblyDir))
            return;
        // Cecil 0.11 requires one to manually set up list of trusted assemblies for assembly resolving
        var curTrusted = appDomain.GetData(TRUSTED_PLATFORM_ASSEMBLIES) as string;
        var addTrusted = string.Join(Path.PathSeparator.ToString(),
                                     Directory.GetFiles(assemblyDir, "*.dll",
                                                        SearchOption.TopDirectoryOnly));
        var newTrusted = curTrusted == null ? addTrusted : $"{curTrusted}{Path.PathSeparator}{addTrusted}";
        appDomain.SetData(TRUSTED_PLATFORM_ASSEMBLIES, newTrusted);
    }

    private IntPtr LocalResolveUnmanaged(Assembly assembly, string name)
    {
        if (ResolvedUnmanagedDlls.TryGetValue(name, out var value))
            return value;

        foreach (var dir in UnmanagedDirectors)
        {
            if (!Utility.TryResolveUnmanagedAssembly(dir, name,
                                                     path => NativeLibrary.TryLoad(path, out var handle)
                                                                 ? handle
                                                                 : IntPtr.Zero, out var ptr)) continue;
            if (ptr == IntPtr.Zero) continue;

            ResolvedUnmanagedDlls[name] = ptr;
            return ptr;
        }

        return IntPtr.Zero;
    }


    private Assembly? LocalResolve(AssemblyLoadContext context, AssemblyName name)
    {
        var foundAssembly = ResolvedAssemblies.FirstOrDefault(n => n.GetName().Name?.Equals(name.Name) ?? false);

        if (foundAssembly != null)
            return foundAssembly;

        foreach (var dir in MangedDirectors)
        {
            if (!Utility.TryResolveDllAssembly(name, dir, out var assembly)) continue;
            ResolvedAssemblies.Add(assembly);
            return assembly;
        }

        return null;
    }
}
