using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using MonoMod.RuntimeDetour;
using NextBepLoader.Core.IL2CPP.Logging;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.PreLoader;
using NextBepLoader.Core.Utils;
using UnityEngine;

namespace NextBepLoader.Core.IL2CPP.NextPreLoaders;

public sealed class IL2CPPHooker(ILogger<IL2CPPHooker> logger, IProviderManager providerManager) : BasePreLoader
{
    public Action<IL2CPPHooker> OnActiveSceneChanged;
    internal NativeHook RuntimeInvokeDetour { get; set; }
    public override Type[] WaitLoadLoader => [typeof(IL2CPPPreLoader)];

    public override void PreLoad(PreLoadEventArg arg) =>
        OnActiveSceneChanged += _ => { providerManager.OnGameActive(); };

    public override void Start()
    {
        if (!TryGetHandle(out var il2CppHandle)) return;
        var runtimeInvokePtr = NativeLibrary.GetExport(il2CppHandle, "il2cpp_runtime_invoke");
        logger.LogInformation("Runtime invoke pointer: 0x{value}", $"{runtimeInvokePtr.ToInt64():X}");
        RuntimeInvokeDetour = new NativeHook(runtimeInvokePtr, OnInvokeMethod, true);
        logger.LogInformation("Runtime invoke patched");
    }

    public bool TryGetHandle(out nint handle)
    {
        if (NativeLibrary.TryLoad(CoreUtils.PlatformGameAssemblyName, typeof(IL2CPPHooker).Assembly, null, out handle))
            return true;

        logger.LogError("Could not locate Il2Cpp game assembly (GameAssembly.dll, UserAssembly.dll or libil2cpp.so)." +
                        " The game might be obfuscated or use a yet unsupported build of Unity.");
        return false;
    }

    private IntPtr OnInvokeMethod(OrgInvokeDetourDelegate originalInvoke,
                                  IntPtr method,
                                  IntPtr obj,
                                  IntPtr parameters,
                                  IntPtr exc)
    {
        var methodName = Marshal.PtrToStringAnsi(Il2CppInterop.Runtime.IL2CPP.il2cpp_method_get_name(method));

        var unhook = false;

        logger.LogDebug("Runtime invoke called for {methodName}", methodName);
        if (methodName == "Internal_ActiveSceneChanged")
            try
            {
                Logger.Sources.Add(new IL2CPPUnityLogSource());
                Application.CallLogCallback("Test call after applying unity logging hook", "", LogType.Assert, true);

                OnActiveSceneChanged.Invoke(this);
                unhook = true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }


        var result = originalInvoke(method, obj, parameters, exc);

        if (!unhook) return result;
        RuntimeInvokeDetour.Dispose();

        logger.LogDebug("Runtime invoke unpatched");
        return result;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr OrgInvokeDetourDelegate(IntPtr method, IntPtr obj, IntPtr parameters, IntPtr exc);
}
