using AsmResolver.DotNet;
using Microsoft.Extensions.DependencyInjection;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.Logging;
using NextBepLoader.Core.PreLoader;
using NextBepLoader.Core.PreLoader.Bootstrap;

namespace NextBepLoader.Deskstop;

public class DesktopProviderManager(
    IServiceProvider serviceProvider, 
    DotNetLoader dotNetLoader) : IProviderManager, IOnLoadStart
{
    public int Priority => 1;
    public IServiceProvider MainServiceProvider { get; } = serviceProvider;
    public List<IProvider> Providers => ProviderLoader.Providers;
    private ProviderLoader ProviderLoader { get; } = new(serviceProvider);
    public void OnLoadStart()
    {
        ProviderLoader
              .LoadFormTypeLoader(dotNetLoader)
              .InitAll(this)
              .RunAll();
    }

    public void OnGameActive()
    {
        foreach (var provider in Providers)
        {
            try
            {
                provider.OnGameActive();
            }
            catch
            {
                // ignored
            }
        }
    }
    
    public T? GetProvider<T>() where T : IProvider => (T?)Providers.FirstOrDefault(n => n is T);
}

public class ProviderLoader(IServiceProvider serviceProvider)
{
    public readonly List<IProvider> Providers = 
        [
            ActivatorUtilities.CreateInstance<PluginLoadProvider>(serviceProvider)
        ];

    private static readonly string BaseFullName = typeof(LoadProviderBase<>).FullName ?? "";
    private static readonly string InterfaceFullName = typeof(IProvider).FullName ?? "";   
    public ProviderLoader LoadFormTypeLoader(DotNetLoader loader)
    {
        var allFindProvider = new FastTypeFinder()
            .FindFormTypeLoader(loader, IsProvider)
            .SelectTo(Selector);
        
        foreach (var targetProvider in allFindProvider)
        {
            Providers.Add(targetProvider);
        }
        
        return this;

        IProvider? Selector(FastTypeFinder.FindInfo info)
        {
            if (info.AssemblyType == null)
                return null;
            return ActivatorUtilities.CreateInstance(serviceProvider, info.AssemblyType) as IProvider;
        }

        bool IsProvider(TypeDefinition type)
        {
            return 
                type.BaseType?.FullName.Equals(BaseFullName) ?? type.Interfaces.Any(n => n.Interface?.FullName.Equals(InterfaceFullName) ?? false);
        }
    }

    public ProviderLoader InitAll(IProviderManager providerManager)
    {
        foreach (var provider in Providers)
        {
            provider.Init(providerManager);
            Logger.LogInfo($"Init {provider.GetType().Name}");
        }
        return this;
    }
    

    public void RunAll()
    {
        foreach (var provider in Providers)
        {
            try
            {
                provider.Run();
                Logger.LogInfo($"Run {provider.GetType().Name}");
            }
            catch (Exception e)
            {
                Logger.LogError($"{provider.GetType().FullName} RunExtension:\n {e}");
            }
        }
    }
}
