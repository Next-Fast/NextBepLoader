using System;
using System.Runtime.CompilerServices;
using AsmResolver.DotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NextBepLoader.Core.Contract;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.PreLoader.Bootstrap;

namespace NextBepLoader.Core.PreLoader.DefaultProviders;

public sealed class PluginLoadProvider(
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
        {
            logger.LogInformation("No Plugin Info");
            return false;
        }

        context.FindInfo = info;

        /*var metadata = info.Type.GetMetadataFromAsmType();
        if (metadata == null)
            return false;

        context.Metadata = metadata;*/
        context.HasLoad = true;
        logger.LogInformation("Load plugin {path}", info.Path);
        return true;
    }

    protected override INextPlugin? Selector(FastTypeFinder.FindInfo info)
    {
        if (info.AssemblyType == null || !pluginInfoManager.TryGet(info, out var context))
            return null;

        try
        {
            RuntimeHelpers.RunModuleConstructor(info.AssemblyType.Module.ModuleHandle);
            context.RunModuleConstructor = true;
            logger.LogInformation("RunModuleConstructor {name}", info.AssemblyType.Name);
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
        catch(Exception e)
        {
            logger.LogError("Create Instance Error:\n Path:{path} Type:{type}:\n {ex}", info.Path, info.TypeName, e);
        }

        /*if (instance == null) return null;
        context.Instance = instance;

        if (instance is not BasePlugin basePlugin) 
            return instance;

        basePlugin.Metadata = info.AssemblyType.GetCustomAttribute<PluginMetadata>();
        basePlugin.Config = new ConfigFile(Path.Combine(Paths.ConfigPath, $"{context.Metadata?.Name}.cfg"), true);*/

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
        {
            pluginInfoManager.Create<NextPluginLoadContext>(type);
        }

        return isTarget;
    }

    public override void OnGameActive()
    {
        foreach (var plugin in AllSelect)
        {
            /*if (!pluginInfoManager.TryGet(plugin, out var context))
                continue;

            if (context.Active)
                continue;*/

            try
            {
                plugin.Load();
                /*context.Active = true;*/
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
