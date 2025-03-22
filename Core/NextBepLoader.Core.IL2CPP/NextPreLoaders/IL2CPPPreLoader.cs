using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using NextBepLoader.Core.IL2CPP.Hooks;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.PreLoader;
using NextBepLoader.Core.Utils;

namespace NextBepLoader.Core.IL2CPP.NextPreLoaders;

internal class IL2CPPPreLoader(INextBepEnv env, ILogger<IL2CPPPreLoader> logger, UnityInfo unityInfo) : BasePreLoader
{
    public override PreLoadPriority Priority => PreLoadPriority.VeryLast;

    public override void PreLoad(PreLoadEventArg arg)
    {
        logger.LogInformation("Running under Unity {version}", unityInfo.GetVersion());
        logger.LogInformation("Runtime version: {Version}", Environment.Version);
        logger.LogInformation("Runtime information: {info}", RuntimeInformation.FrameworkDescription);
        logger.LogInformation("OS information: {os}", PlatformDetection.OS);
        logger.LogInformation("Game executable path: {path}", Paths.ExecutablePath);
        logger.LogInformation("Interop assembly directory: {dir}", Paths.IL2CPPInteropAssemblyDirectory);
        logger.LogInformation("Loader root path: {path}", Paths.LoaderRootPath);
        logger.LogInformation("Loader Core Assembly path: {path}", Paths.CoreAssemblyPath);
        logger.LogInformation("Loader Type {type}", LoaderInstance.LoaderRegister.Current.LoaderType);

        if (PlatformDetection.OS.Is(OSKind.Wine) && !Environment.Is64BitProcess)
            if (!NativeLibrary.TryGetExport(NativeLibrary.Load("ntdll"), "RtlRestoreContext", out var _))
                logger.LogWarning("Your wine version doesn't support CoreCLR properly, expect crashes! Upgrade to wine 7.16 or higher.");

        env.RegisterSystemEnv("IL2CPP_INTEROP_DATABASES_LOCATION", Paths.IL2CPPInteropAssemblyDirectory);
    }

    public override void Start()
    {
        NativeLibrary.SetDllImportResolver(typeof(Il2CppInterop.Runtime.IL2CPP).Assembly, DllImportResolver);
        DetourContext.SetGlobalContext(new DetourFactoryContext(new DobbyDetourFactory()));
    }

    private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath) =>
        libraryName == CoreUtils.PlatformGameAssemblyName
            ? NativeLibrary.Load(Paths.GameAssemblyPath, assembly, searchPath)
            : IntPtr.Zero;
}
