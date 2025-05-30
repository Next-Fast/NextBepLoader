using System.Runtime.CompilerServices;
using AsmResolver.DotNet;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Microsoft.Extensions.Logging;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.PreLoader;
using NextBepLoader.Core.PreLoader.Bootstrap;

namespace BepInExPluginProvider;

public class BepInExPluginProvider(DotNetLoader loader, ILogger<BepInExPluginProvider> logger, PluginInfoManager pluginInfoManager) : LoadProviderBase<BasePlugin>(loader)
{
    private static readonly string FullName = typeof(BasePlugin).FullName ?? "";
    public bool GameActivated { get; private set; }

    public override void Init(IProviderManager manager)
    {
        _DotNetLoader.AssemblyFilter += AssemblyFilter;
    }

    private static readonly string CoreAssemblyName = typeof(BasePlugin).Assembly.GetName().Name!;
    private static readonly string BasePluginFullName = typeof(BasePlugin).FullName!;

    private static bool AssemblyFilter(AssemblyDefinition assembly)
    {
        if (assembly.ManifestModule == null)
            return false;

        var typeReferences = assembly.ManifestModule.GetImportedTypeReferences().ToList();
        var references = assembly.ManifestModule.AssemblyReferences.ToList();
        return references.Any(n => n.Name!.Equals(CoreAssemblyName)) &&
               typeReferences.Any(n => n.FullName.Equals(BasePluginFullName));
    }
    
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

    protected override BasePlugin? Selector(FastTypeFinder.FindInfo info)
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

        BasePlugin? instance = null;

        try
        {
            instance = Activator.CreateInstance(info.AssemblyType) as BasePlugin;
            logger.LogInformation("Create Plugin Instance:\n Name:{name} Version:{version} Puid:{puid}",
                                  context.Name, context.Version, context.Puid);
        }
        catch(Exception e)
        {
            logger.LogError("Create Instance Error:\n Path:{path} Type:{type}:\n {ex}", info.Path, info.TypeName, e);
        }

        return instance;
    }

    protected override bool IsTarget(TypeDefinition type) =>
        type.BaseType?.FullName.Equals(BasePluginFullName) ?? false;

    public override void OnGameActive()
    {
        foreach (var plugin in AllSelect)
        {
            try
            {
                plugin.Load();
                logger.LogInformation($"Active {plugin.GetType().FullName}");
            }
            catch (Exception e)
            {
                logger.LogError($"{plugin.GetType().FullName} LoadError:\n{e}");
            }
        }
    }
}

public class BepInExPluginLoadContent : PluginLoadContextBase<BepInPlugin, BasePlugin>;
