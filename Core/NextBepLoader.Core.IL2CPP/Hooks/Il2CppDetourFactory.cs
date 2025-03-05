using System.IO;
using MonoMod.Core;
using NextBepLoader.Core.IL2CPP.Hooks.Dobby;

namespace NextBepLoader.Core.IL2CPP.Hooks;

public class Il2CppDetourFactory : IDetourFactory
{
    private static IDetourFactory? currentFactory;

    public static IDetourFactory CurrentFactory
    {
        get
        {
            if (currentFactory != null)
                return currentFactory;

            var hasDobby = File.Exists(Path.Combine(Paths.CoreDirectory, "dobby.dll"));
            currentFactory = hasDobby
                                 ? new Il2CppDetourFactory()
                                 : DetourFactory.Current;

            return currentFactory;
        }
        set => currentFactory = value;
    }

    public ICoreDetour CreateDetour(CreateDetourRequest request) =>
        DetourFactory.Current.CreateDetour(request.Source, request.Target, request.ApplyByDefault);

    public ICoreNativeDetour CreateNativeDetour(CreateNativeDetourRequest request) =>
        new DobbyNativeDetour(request.Source, request.Target);
}
