using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AsmResolver.DotNet;
using NextBepLoader.Core.Logging;

namespace NextBepLoader.Core.PreLoader.Bootstrap;

public abstract class BaseTypeLoader<TLoader, TType, TAssembly>
    where TAssembly : notnull where TLoader : BaseTypeLoader<TLoader, TType, TAssembly>
{
    public readonly Dictionary<TAssembly, List<TType>> _LoadTypes = [];
    public List<TAssembly> Assemblies { get; set; } = [];
    public Action<TType, TAssembly>? OnLoadType { get; set; } = null;
    public Func<TType, bool>? TypeFilter { get; set; } = null;
    public Func<TAssembly, bool>? AssemblyFilter { get; set; } = null;
    private Func<TAssembly, bool>? CachedAssemblyFilter { get; set; } = null;

    public virtual TLoader AddAssembly(TAssembly assembly)
    {
        Assemblies.Add(assembly);
        return (TLoader)this;
    }

    public virtual TLoader AddAssemblies(IEnumerable<TAssembly> assemblies)
    {
        Assemblies.AddRange(assemblies);
        return (TLoader)this;
    }


    public abstract TLoader AddAssemblyFormPath(string path);

    public abstract TLoader AddAssemblyFormBytes(byte[] bytes);

    public virtual TLoader AddAssembliesFormDirector(DirectoryInfo directory)
    {
        foreach (var file in directory.GetFiles().Where(n => n.Extension == ".dll"))
            AddAssemblyFormPath(file.FullName);
        return (TLoader)this;
    }

    public virtual TLoader AddAssembliesFormDirector(string path) => AddAssembliesFormDirector(new DirectoryInfo(path));

    public virtual TLoader LoadTypes()
    {
        foreach (var assembly in Assemblies.Where(n => AssemblyFilter?.Invoke(n) ?? true))
        {
            if (!_LoadTypes.ContainsKey(assembly))
                _LoadTypes.Add(assembly, []);

            foreach (var type in LoadTypeFormAssembly(assembly).Where(n => TypeFilter?.Invoke(n) ?? true))
            {
                if (_LoadTypes[assembly].Contains(type)) continue;
                _LoadTypes[assembly].Add(type);
                OnLoadType?.Invoke(type, assembly);
            }
        }

        return (TLoader)this;
    }

    public abstract List<TType> LoadTypeFormAssembly(TAssembly assembly);
}

public class DotNetLoader : BaseTypeLoader<DotNetLoader, TypeDefinition, AssemblyDefinition>
{
    public override DotNetLoader AddAssemblyFormPath(string path)
    {
        Logger.LogInfo("Add Assembly From Path: " + path);
        Assemblies.Add(AssemblyDefinition.FromFile(path));
        return this;
    }

    public override DotNetLoader AddAssemblyFormBytes(byte[] bytes)
    {
        Assemblies.Add(AssemblyDefinition.FromBytes(bytes));
        return this;
    }

    public override List<TypeDefinition> LoadTypeFormAssembly(AssemblyDefinition assembly) =>
        assembly.ManifestModule?.GetAllTypes().ToList() ?? [];
}

public class AssemblyTypeLoader : BaseTypeLoader<AssemblyTypeLoader, Type, Assembly>
{
    public override AssemblyTypeLoader AddAssembly(Assembly assembly)
    {
        Assemblies.Add(assembly);
        return this;
    }

    public override AssemblyTypeLoader AddAssemblies(IEnumerable<Assembly> assemblies)
    {
        Assemblies.AddRange(assemblies);
        return this;
    }

    public override AssemblyTypeLoader AddAssemblyFormPath(string path)
    {
        Assemblies.Add(Assembly.LoadFile(path));
        return this;
    }


    public override AssemblyTypeLoader AddAssemblyFormBytes(byte[] bytes)
    {
        Assemblies.Add(Assembly.Load(bytes));
        return this;
    }


    public override List<Type> LoadTypeFormAssembly(Assembly assembly) => assembly.GetTypes().ToList();
}
