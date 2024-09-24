using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NextBepLoader.Core;
using NextBepLoader.Core.Contract;
using NextBepLoader.Core.IL2CPP;
using NextBepLoader.Core.IL2CPP.Logging;
using NextBepLoader.Core.IL2CPP.NextPreLoaders;
using NextBepLoader.Core.Logging;
using NextBepLoader.Core.PreLoader;
using NextBepLoader.Core.PreLoader.NextPreLoaders;
using NextBepLoader.Core.PreLoader.RuntimeFixes;
using NextBepLoader.Deskstop.Utils;

namespace NextBepLoader.Deskstop;

public sealed class DesktopLoader : LoaderBase<DesktopLoader>
{
    private List<Type> loaders = [];
    public override LoaderPathBase Paths { get; set; } = new DesktopPath();
    public override LoaderPlatformType LoaderType => LoaderPlatformType.Desktop;
    internal IServiceCollection Collection { get; set; } = new ServiceCollection();

    public override void Start()
    {
        LoaderVersion = new Version();
        loaders = [
            typeof(ResolvePreLoad), 
            typeof(IL2CPPHooker), 
            typeof(IL2CPPPreLoader)
        ];
        PlatformUtils.SetDesktopPlatformVersion();
        MainServices = Collection
                       .AddSingleton<TaskFactory>()
                       .AddSingleton<INextServiceManager, NextServiceManager>()
                       .AddSingleton<IProviderManager, DesktopProviderManager>()
                       .AddSingleton<UnityInfo>()
                       .AddOnStart<INextBepEnv, DesktopBepEnv>()
                       .AddOnStart<IPreLoaderManager, DesktopPreLoadManager>(loaders)
                       .AddStartRunner()
                       .AddNextLogger()
                       .BuildServiceProvider();
        
        HarmonyBackendFix.Initialize();
        RedirectStdErrFix.Apply();
    }
}