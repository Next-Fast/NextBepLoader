using System.IO;
using System.Runtime.InteropServices;
using MonoMod.Core;
using NextBepLoader.Core.IL2CPP.Hooks.Dobby;

namespace NextBepLoader.Core.IL2CPP.Hooks;

internal class DobbyDetourFactory : IDetourFactory
{
    private static IDetourFactory? currentFactory;

    public static IDetourFactory CurrentFactory
    {
        get
        {
            if (currentFactory != null)
                return currentFactory;
            
            var dobbyPath = Path.Combine(Paths.CoreDirectory, "dobby.dll");
            if (NativeLibrary.TryLoad(dobbyPath, out var _))
            {
                currentFactory = new DobbyDetourFactory();
            }
            else
            {
                currentFactory = DetourFactory.Current;
            }

            return currentFactory;
        }
        set => currentFactory = value;
    }

    public ICoreDetour CreateDetour(CreateDetourRequest request) =>
        DetourFactory.Current.CreateDetour(request.Source, request.Target, request.ApplyByDefault);

    public ICoreNativeDetour CreateNativeDetour(CreateNativeDetourRequest request) =>
        new DobbyNativeDetour(request.Source, request.Target);
}
