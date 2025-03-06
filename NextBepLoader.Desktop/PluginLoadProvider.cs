using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using AsmResolver.DotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NextBepLoader.Core;
using NextBepLoader.Core.Configuration;
using NextBepLoader.Core.Contract;
using NextBepLoader.Core.Contract.Attributes;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.PreLoader;
using NextBepLoader.Core.PreLoader.Bootstrap;

namespace NextBepLoader.Deskstop;

public class PluginLoadProvider(
    ILogger<PluginLoadProvider> logger,
    DotNetLoader loader,
    PluginInfoManager pluginInfoManager)
    : LoadProviderBase<INextPlugin>(loader)
{
    private static readonly string FullName = typeof(BasePlugin).FullName ?? "";
    private static readonly string InterfaceName = typeof(INextPlugin).FullName ?? "";
    private ServiceFastInfo? _pluginServiceInfo;
    public bool GameActivated { get; private set; }

    public override void Init(IProviderManager manager) =>
        _pluginServiceInfo = NextServiceManager.Instance.GetServiceInfo("PluginService");

    protected override bool PreFilter(FastTypeFinder.FindInfo info)
    {
        if (!pluginInfoManager.TryGet(info.Type, out var context))
            return false;

        if (context.HasLoad)
            return true;

        var metadata = info.Type.GetMetadataFromAsmType();
        if (metadata == null)
            return false;

        context.Metadata = metadata;
        context.HasLoad = true;
        return true;
    }

    protected override INextPlugin? Selector(FastTypeFinder.FindInfo info)
    {
        if (info.AssemblyType == null || !pluginInfoManager.TryGet(info, out var context) || context.Metadata == null)
            return null;

        try
        {
            RuntimeHelpers.RunModuleConstructor(info.AssemblyType.Module.ModuleHandle);
            context.RunModuleConstructor = true;
        }
        catch
        {
            // ignored
        }

        INextPlugin? instance = null;

        try
        {
            instance = ActivatorUtilities.CreateInstance(
                                                         _pluginServiceInfo?.Collection.BuildOrCreateProvider()
                                                       ??
                                                         NextServiceManager.Instance.MainProvider,
                                                         info.AssemblyType
                                                        ) as INextPlugin;
            logger.LogInformation("Create Plugin Instance:\n Name:{name} Version:{version} Puid:{puid}",
                                  context.Name, context.Version, context.Puid);
        }
        catch
        {
            logger.LogError("Create Instance Error:\n Path:{path} Type:{type}", info.Path, info.TypeName);
        }

        if (instance == null) return instance;
        context.Instance = instance;

        if (instance is not BasePlugin basePlugin) return instance;
        
        basePlugin.Metadata = context.Metadata;
        basePlugin.Config = new ConfigFile(Path.Combine(Paths.ConfigPath, $"{context.Metadata.Name}.cfg"), true);

        return instance;
    }

    protected override bool IsTarget(TypeDefinition type)
    {
        var baseType = type.BaseType;
        if (baseType == null)
        {
            logger.LogInformation("{type} Base IsNull", type.FullName);
            return false;
        }

        var isTarget = baseType.FullName.Equals(FullName);
        logger.LogInformation($"is Target: {baseType.FullName} {FullName} {isTarget}");

        if (isTarget) 
            pluginInfoManager.Create(type);

        return isTarget;
    }

    public override void OnGameActive()
    {
        foreach (var plugin in AllSelect)
        {
            if (!pluginInfoManager.TryGet(plugin, out var context))
                continue;

            if (context.Active || !context.CanLoad)
                continue;

            try
            {
                plugin.Load();
                context.Active = true;
                logger.LogInformation($"Active {plugin.GetType().FullName}");
            }
            catch (Exception e)
            {
                logger.LogError($"{plugin.GetType().FullName} LoadError:\n{e}");
            }
        }

        GameActivated = true;
    }
}

public class PluginLoadContext : IContent
{
    public string Name { get; set; }

    public string Puid { get; set; }
    public Version Version { get; set; }

    public bool CanLoad { get; set; } = true;

    public bool Active { get; set; }

    public bool HasLoad { get; set; }

    internal string TypeName { get; set; }

    internal TypeDefinition TypeDefinition { get; set; }
    internal FastTypeFinder.FindInfo FindInfo { get; set; }

    internal bool RunModuleConstructor { get; set; }

    public INextPlugin? Instance { get; set; }

    public PluginMetadata? Metadata { get; set; }

    public T? To<T>() where T : BasePlugin
    {
        if (TypeName != typeof(T).FullName)
            return null;

        return Instance as T;
    }
}

public class PluginInfoManager : IInfoManager
{
    private readonly List<PluginLoadContext> _allLoadContexts = [];

    internal void Create(TypeDefinition typeDefinition)
    {
        var pluginLoadContext = new PluginLoadContext
        {
            TypeDefinition = typeDefinition
        };
        _allLoadContexts.Add(pluginLoadContext);
    }

    public bool TryGet(FastTypeFinder.FindInfo findInfo, [MaybeNullWhen(false)] out PluginLoadContext pluginLoadContext)
    {
        pluginLoadContext = _allLoadContexts.FirstOrDefault(x => x.FindInfo == findInfo);
        return pluginLoadContext != null;
    }

    internal bool TryGet(TypeDefinition definition, [MaybeNullWhen(false)] out PluginLoadContext pluginLoadContext)
    {
        pluginLoadContext = _allLoadContexts.FirstOrDefault(x => x.TypeDefinition == definition);
        return pluginLoadContext != null;
    }

    internal bool TryGet(INextPlugin plugin, [MaybeNullWhen(false)] out PluginLoadContext pluginLoadContext)
    {
        pluginLoadContext = _allLoadContexts.FirstOrDefault(x => x.Instance == plugin);
        return pluginLoadContext != null;
    }
}
