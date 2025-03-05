using Microsoft.Extensions.Logging;
using NextBepLoader.Core.Logging.DefaultSource;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace NextBepLoader.Core.IL2CPP.Utils;

public static class CPP2ILUtils
{
    public static ManualLogSource? BeLog;
    public static ILogger? MSLogger;

    static CPP2ILUtils()
    {
        Cpp2IL.Core.Logging.Logger.VerboseLog += (message, s) =>
        {
            BeLog?.LogDebug($"[{s}] {message.Trim()}");
            MSLogger?.LogDebug("[{source}] {message}", s, message);
        };
        Cpp2IL.Core.Logging.Logger.InfoLog += (message, s) =>
        {
            BeLog?.LogInfo($"[{s}] {message.Trim()}");
            MSLogger?.LogInformation("[{source}] {message}", s, message);
        };
        Cpp2IL.Core.Logging.Logger.WarningLog += (message, s) =>
        {
            BeLog?.LogWarning($"[{s}] {message.Trim()}");
            MSLogger?.LogWarning("[{source}] {message}", s, message);
        };
        Cpp2IL.Core.Logging.Logger.ErrorLog += (message, s) =>
        {
            BeLog?.LogError($"[{s}] {message.Trim()}");
            MSLogger?.LogError("[{source}] {message}", s, message);
        };
    }

    public static void SetLogger(ManualLogSource? beLog, ILogger? msLogger = null)
    {
        if (beLog != null)
            BeLog = beLog;

        if (msLogger != null)
            MSLogger = msLogger;
    }
}
