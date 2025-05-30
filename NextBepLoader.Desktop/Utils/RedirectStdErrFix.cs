using MonoMod.Utils;
using NextBepLoader.Core;
using NextBepLoader.Core.Logging;
using Vanara.PInvoke;

namespace NextBepLoader.Deskstop.Utils;

internal static class RedirectStdErrFix
{
    public static void Apply()
    {
        if (!PlatformDetection.OS.Is(OSKind.Windows)) return;
        
        var path = Path.Combine(Paths.LoaderRootPath, "ErrorLog.log");
#pragma warning disable CA1416
        var errorFile = Kernel32.CreateFile(
                                            path,
                                            Kernel32.FileAccess.GENERIC_WRITE,
                                            FileShare.Read,
                                            null,
                                            FileMode.CreateNew,
                                            FileFlagsAndAttributes.FILE_ATTRIBUTE_NORMAL);
        
        if (errorFile == Kernel32.SafeEventHandle.InvalidHandle)
        {
            Logger.Log(LogLevel.Warning, "Failed to open error log file; skipping error redirection");
            return;
        }

        if (!Kernel32.SetStdHandle(Kernel32.StdHandleType.STD_ERROR_HANDLE, errorFile))
            Logger.Log(LogLevel.Warning, "Failed to redirect stderr; skipping error redirection");
    }
}
