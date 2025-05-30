using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AsmResolver.DotNet;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.PreLoader.Bootstrap;
using NextBepLoader.Core.PreLoader.DefaultProviders;

namespace NextBepLoader.Core.PreLoader;

public class PluginInfoManager : IInfoManager
{
    private readonly List<IPluginLoadContext> _allLoadContexts = [];

    internal IPluginLoadContext Create<T>(TypeDefinition typeDefinition) where T : IPluginLoadContext, new()
    {
        var pluginLoadContext = new T()
        {
            TypeDefinition = typeDefinition
        };
        _allLoadContexts.Add(pluginLoadContext);
        return pluginLoadContext;
    }

    public bool TryGet(FastTypeFinder.FindInfo findInfo, [MaybeNullWhen(false)] out IPluginLoadContext pluginLoadContext)
    {
        pluginLoadContext = _allLoadContexts.FirstOrDefault(x => x.FindInfo == findInfo);
        return pluginLoadContext != null;
    }

    public bool TryGet(TypeDefinition definition, [MaybeNullWhen(false)] out IPluginLoadContext pluginLoadContext)
    {
        pluginLoadContext = _allLoadContexts.FirstOrDefault(x => x.TypeDefinition == definition);
        return pluginLoadContext != null;
    }

    internal bool TryGet<TContent, TInstance, TMeta>(TInstance plugin, [MaybeNullWhen(false)] out IPluginLoadContext pluginLoadContext)
        where TContent : PluginLoadContextBase<TMeta, TInstance> where TInstance : class where TMeta : Attribute
    {
        foreach (var context in _allLoadContexts)
        {
            if (context is not TContent content) continue;
            if (content.Instance != plugin) continue;   
                
            pluginLoadContext = content;
            return true;
        }

        pluginLoadContext = null;
        return false;
    }
}
