using System;
using NextBepLoader.Core.IL2CPP.Logging;
using NextBepLoader.Core.PreLoader;

namespace NextBepLoader.Core.IL2CPP.NextPreLoaders;

internal class LoggingStarter : BasePreLoader
{
    public override Type[] WaitLoadLoader => [typeof(IL2CPPPreLoader)];

    public override void Start()
    {
        try
        {
            var log = new IL2CPPLogSource();
            Logger.Sources.Add(log);
        }
        catch
        {
            // ignored
        }
        
        try
        {
            var log = new HarmonyLogSource();
            Logger.Sources.Add(log);
        }
        catch
        {
            // ignored
        }
    }
}
