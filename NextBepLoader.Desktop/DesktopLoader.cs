using AsmResolver.DotNet;
using Microsoft.Extensions.DependencyInjection;
using NextBepLoader.Core;
using NextBepLoader.Core.Contract;
using NextBepLoader.Core.IL2CPP;
using NextBepLoader.Core.IL2CPP.NextPreLoaders;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.Logging;
using NextBepLoader.Core.Logging.DefaultListener;
using NextBepLoader.Core.Logging.Extensions;
using NextBepLoader.Core.Logging.Interface;
using NextBepLoader.Core.PreLoader;
using NextBepLoader.Core.PreLoader.Bootstrap;
using NextBepLoader.Core.PreLoader.NextPreLoaders;
using NextBepLoader.Core.Utils;
using NextBepLoader.Deskstop.Console;
using NextBepLoader.Deskstop.Utils;

namespace NextBepLoader.Deskstop;

public sealed class DesktopLoader : LoaderBase<DesktopLoader>
{
    public override LoaderPathBase Paths { get; set; } = new DesktopPath();
    public override LoaderPlatformType LoaderType => LoaderPlatformType.Desktop;
    internal NextServiceCollection Collection { get; set; }
    private static readonly string CoreAssemblyName = typeof(LoaderInstance).Assembly.GetName().Name!;

    public override IConsoleManager ConsoleManager => DesktopConsoleManager.Instance;

    private static readonly string[] FullNames = 
    [
        typeof(BasePlugin).FullName!,
        typeof(BasePreLoader).FullName!,
        typeof(IProvider).FullName!,
        typeof(IStartup).FullName!
    ];


    public readonly DotNetLoader DotNetLoader = new()
    {
        AssemblyFilter = AssemblyFilter
    };

    private static bool AssemblyFilter(AssemblyDefinition assembly)
    {
        if (assembly.ManifestModule == null)
            return false;

        var typeReferences = assembly.ManifestModule.GetImportedTypeReferences().ToList();
        var references = assembly.ManifestModule.AssemblyReferences.ToList();
        return references.Any(n => n.Name!.Equals(CoreAssemblyName)) && FullNames.Any(name => typeReferences.Any(n => n.FullName.Equals(name)));
    }
    

    public ILogListener? DiskLogListener { get; private set; }
    public readonly PreLoadEventArg PreLoadEventArg = new();
    public override void Start()
    {
        PlatformUtils.SetDesktopPlatformVersion();
        RedirectStdErrFix.Apply();
        
        DiskLogListener = new DiskListener("./LatestLog.log").Register();
        ConsoleManager.Init(new ConsoleConfig());
        ConsoleManager.CreateConsole();
        
        LoaderVersion = new Version(1, 0, 0);
        Paths.InitPaths(true);
        
        DotNetLoader.AddAssembliesFormDirector(Paths.PluginPath);
        DotNetLoader.AddAssembliesFormDirector(Paths.ProviderDirectory);
        
        DotNetLoader.OnLoadType = (definition, assemblyDefinition) =>
        {
            Logger.LogInfo("Load Type: " + definition.FullName);
        };

        Collection = BuildService();
        MainServices = Collection.BuildOrCreateProvider();
        MainServices.TryRun<DesktopBepEnv>()
                    .TryRun<DesktopPreLoadManager>()
                    .TryRun<DesktopProviderManager>();
    }

    public NextServiceCollection BuildService()
    {
        var collection = NextServiceManager.Instance.CreateMainCollection();
        collection
            .AddSingleton(DesktopConsoleManager.Instance)
            .AddSingleton(DotNetLoader)
            .AddSingleton(this)
            .AddSingleton(UnityInfo.Instance)
            .AddSingleton<PluginInfoManager>()
            .AddSingleton<BasePreLoader, ResolvePreLoad>()
            .AddSingleton<BasePreLoader, Cpp2ILStarter>()
            .AddSingleton<BasePreLoader, HashComputer>()
            .AddSingleton<BasePreLoader, IL2CPPHooker>()
            .AddSingleton<BasePreLoader, UnityBasePreDownloader>()
            .AddSingleton<BasePreLoader, IL2CPPInteropStarter>()
            .AddSingleton<BasePreLoader, IL2CPPPreLoader>()
            .AddSingleton<IProvider, PluginLoadProvider>()
            .AddSingleton<IProvider, StartupLoadProvider>()
            .SingleService<INextBepEnv, DesktopBepEnv>()
            .SingleService<IProviderManager, DesktopProviderManager>()
            .SingleService<IPreLoaderManager, DesktopPreLoadManager>()
            .AddTransient<HttpClient>()
            .AddNextLogger()
            .AddTraceLogSource();
        return collection;
    }
}
