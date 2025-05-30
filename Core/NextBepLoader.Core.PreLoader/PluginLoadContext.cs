using System;
using AsmResolver.DotNet;
using NextBepLoader.Core.Contract;
using NextBepLoader.Core.Contract.Attributes;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.PreLoader.Bootstrap;

namespace NextBepLoader.Core.PreLoader;


public class NextPluginLoadContext : PluginLoadContextBase<PluginMetadata, INextPlugin>;

public class PluginLoadContextBase<TMetadata, TInstance> : IPluginLoadContext where TInstance : class where TMetadata : Attribute
{
    public TInstance? Instance { get; set; }

    public TMetadata? Metadata { get; set; }

    public T? To<T>() where T : class, TInstance
    {
        if (TypeName != typeof(T).FullName)
            return null;

        return Instance as T;
    }

    public string Name { get; set; }
    public string Puid { get; set; }
    public Version Version { get; set; }
    public bool RunModuleConstructor { get; set; }
    public bool Active { get; set; }
    public bool HasLoad { get; set; }
    public string TypeName { get; set; }
    public TypeDefinition TypeDefinition { get; set; }
    public FastTypeFinder.FindInfo FindInfo { get; set; }
}

public interface IPluginLoadContext : IContent
{
    public string Name { get; }

    public string Puid { get; }
    public Version Version { get;  }
    public bool RunModuleConstructor { get; set; }
    public bool Active { get;}

    public bool HasLoad { get; set; }

    internal string TypeName { get; }

    internal TypeDefinition TypeDefinition { get; set; }
    public FastTypeFinder.FindInfo FindInfo { get; set; }
}
