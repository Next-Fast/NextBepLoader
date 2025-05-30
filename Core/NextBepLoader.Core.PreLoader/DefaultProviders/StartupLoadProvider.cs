using System;
using System.Linq;
using AsmResolver.DotNet;
using Microsoft.Extensions.Logging;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.Logging;
using NextBepLoader.Core.PreLoader.Bootstrap;

namespace NextBepLoader.Core.PreLoader.DefaultProviders;

public class StartupLoadProvider(ILogger<StartupLoadProvider> logger, DotNetLoader dotNetLoader, NextServiceManager serviceManager)
    : LoadProviderBase<IStartup>(dotNetLoader)
{
    private static readonly string BaseFullName = typeof(ServiceStartupBase).FullName ?? "";
    private static readonly string InterfaceFullName = typeof(IStartup).FullName ?? "";
    private NextServiceCollection? Service { get; set; }

    public override void Init(IProviderManager manager)
    {
        Service = serviceManager.CreateService("PluginService");
        if (Service == null)
            Logger.LogError("Service not found");
        
        Service?.Copy(serviceManager.MainFastInfo.Collection);
    }

    public override void Run()
    {
        if (Service == null) return;
        base.Run();
        foreach (var startup in AllSelect)
            try
            {
                startup.ConfigureServices(Service);
            }
            catch
            {
                logger.LogWarning("Startup configure service error");
            }
    }

    protected override IStartup? Selector(FastTypeFinder.FindInfo info)
    {
        if (info.AssemblyType == null)
            return null;
        return Activator.CreateInstance(info.AssemblyType) as IStartup;
    }

    protected override bool IsTarget(TypeDefinition type) =>
        type.BaseType?.FullName.Equals(BaseFullName)
      ??
        type.Interfaces.Any(n => n.Interface?.FullName.Equals(InterfaceFullName) ?? false);
}
