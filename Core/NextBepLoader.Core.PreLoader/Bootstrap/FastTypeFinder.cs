using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AsmResolver.DotNet;
using NextBepLoader.Core.Logging;

namespace NextBepLoader.Core.PreLoader.Bootstrap;

/// <summary>
///     Provides methods for loading specified types from an assembly.
/// </summary>
public class FastTypeFinder
{
    public List<FindInfo> _AllFindInfo = [];

    public FastTypeFinder FindSingleAssembly(string path,
                                             Func<AssemblyDefinition, bool>? assemblyFilter = null,
                                             Func<TypeDefinition, bool>? typeFilter = null)
    {
        var dotNetLoader = new DotNetLoader
        {
            AssemblyFilter = assemblyFilter,
            TypeFilter = typeFilter
        };
        dotNetLoader.AddAssemblyFormPath(path);
        return FindFormTypeLoader(dotNetLoader, typeFilter);
    }

    public FastTypeFinder FindFormTypePath(string path,
                                           Func<AssemblyDefinition, bool>? assemblyFilter = null,
                                           Func<TypeDefinition, bool>? typeFilter = null)
    {
        var dotNetLoader = new DotNetLoader
        {
            AssemblyFilter = assemblyFilter,
            TypeFilter = typeFilter
        };
        dotNetLoader.AddAssembliesFormDirector(path);
        return FindFormTypeLoader(dotNetLoader, null);
    }

    public FastTypeFinder Where(Func<FindInfo, bool> typeFilter)
    {
        _AllFindInfo = _AllFindInfo.Where(typeFilter).ToList();
        return this;
    }

    public FastTypeFinder FindFormTypeLoader(DotNetLoader loader, Func<TypeDefinition, bool>? typeFilter)
    {
        _AllFindInfo = [];

        foreach (var (assembly, types) in loader.LoadTypes()._LoadTypes)
        {
            if (assembly.ManifestModule == null)
            {
                Logger.LogWarning($"{assembly.Name} has no ManifestModule");
                continue;
            }

            var path = assembly.ManifestModule.FilePath!;
            Logger.LogInfo($"FastTypeFinder: {assembly.Name} {path}");
            try
            {
                var allFind = types.Where(n => typeFilter == null || typeFilter(n))
                                   .Select(n => new FindInfo(path, assembly, n));
                foreach (var find in allFind)
                {
                    _AllFindInfo.Add(find);
                    Logger.LogInfo($"FastTypeFinder Add {find.TypeName}");
                }

                Logger.LogInfo($"FastTypeFinder Add {path}");
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }

        return this;
    }

    public List<T> SelectTo<T>(Func<FindInfo, T?> typeSelector) =>
        _AllFindInfo.Select(typeSelector).OfType<T>().ToList();

    public record FindInfo(string Path, AssemblyDefinition AssemblyDefinition, TypeDefinition Type)
    {
        public string TypeName => Type.FullName;
        public Assembly AssemblyInstance => Assembly.LoadFrom(Path);
        public Type? AssemblyType => AssemblyInstance.GetType(TypeName);
    }
}
