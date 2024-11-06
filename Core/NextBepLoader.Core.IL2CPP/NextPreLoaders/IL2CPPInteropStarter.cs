using System;
using System.IO;
using System.Threading.Tasks;
using Il2CppInterop.Common;
using Il2CppInterop.Generator;
using Il2CppInterop.Generator.Runners;
using Il2CppInterop.HarmonySupport;
using Il2CppInterop.Runtime.Startup;
using Microsoft.Extensions.Logging;
using NextBepLoader.Core.IL2CPP.Hooks;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.PreLoader;
using NextBepLoader.Core.Utils;

namespace NextBepLoader.Core.IL2CPP.NextPreLoaders;

public class IL2CPPInteropStarter(
    INextBepEnv env,
    ILogger<IL2CPPInteropStarter> logger, 
    TaskFactory factory,
    UnityInfo unityInfo
    ) : BasePreLoader
{
    public override Type[] WaitLoadLoader => [typeof(Cpp2ILStarter)];
    public IL2CPPCheckEventArg IL2CPPCheckEventArg;

    public override void PreLoad(PreLoadEventArg arg)
    {
        IL2CPPCheckEventArg = env.GetOrCreateEventArgs<IL2CPPCheckEventArg>();
    }

    public override void Start()
    {
        factory.StartNew(() =>
        {
            var generatorTime = CoreUtils.StartStopwatch(StartGenerator);
            logger.LogInformation("IL2CPPInteropStarter Start generator,use time:{time}", generatorTime);
            var runtimeTime = CoreUtils.StartStopwatch(StartRuntime);
            logger.LogInformation("IL2CPPInteropStarter Start Runtime,use time:{time}", runtimeTime);
        });
    }

    public void StartRuntime()
    {
        Il2CppInteropRuntime.Create(new RuntimeConfiguration
                            {
                                UnityVersion = unityInfo,
                                DetourProvider = new Il2CppInteropDetourProvider()
                            })
                            .AddLogger(logger)
                            .AddHarmonySupport()
                            .Start();
    }

    public void StartGenerator()
    {
        CoreUtils.DeleteAllFiles(Paths.IL2CPPInteropAssemblyDirectory);
        var opts = new GeneratorOptions
        {
            GameAssemblyPath = Paths.GameAssemblyPath,
            Source = IL2CPPCheckEventArg.ResolverAssemblies,
            OutputDir = Paths.IL2CPPInteropAssemblyDirectory,
            UnityBaseLibsDir = Paths.UnityBaseDirectory,
            ObfuscatedNamesRegex = null
        };
        logger.LogInformation("Generating interop assemblies");
        Il2CppInteropGenerator.Create(opts)
                              .AddLogger(logger)
                              .AddInteropAssemblyGenerator()
                              .Run();
    }
}
