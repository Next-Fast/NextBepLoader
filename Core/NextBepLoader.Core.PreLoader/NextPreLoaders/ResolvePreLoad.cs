using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using NextBepLoader.Core.Logging;
using NextBepLoader.Core.Utils;

namespace NextBepLoader.Core.PreLoader.NextPreLoaders;


public class ResolvePreLoad : BasePreLoader
{
    public List<Assembly> ResolvedAssemblies { get; } = [];
    public Dictionary<string, IntPtr> ResolvedUnmanagedDlls { get; } = new();
    
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
        Paths.DependencyDirectory,
    ];
    
    
    public override PreLoadPriority Priority => PreLoadPriority.VeryLast;
    public AssemblyLoadContext LoadContext { get; private set; }

    public override void Start()
    {
        LoadContext = AssemblyLoadContext.Default;
        LoadContext.Resolving += LocalResolve;
        LoadContext.ResolvingUnmanagedDll += LocalResolveUnmanaged; 
    }

    private IntPtr LocalResolveUnmanaged(Assembly assembly, string name)
    {
        if (ResolvedUnmanagedDlls.TryGetValue(name, out var value))
            return value;
        
        foreach (var dir in UnmanagedDirectors)
        {
            if (!Utility.TryResolveUnmanagedAssembly(dir, name, path => NativeLibrary.TryLoad(path, out var handle) ? handle : IntPtr.Zero, out var ptr)) continue;
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
