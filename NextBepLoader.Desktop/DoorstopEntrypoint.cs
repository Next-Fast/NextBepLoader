using System.Diagnostics;
using MonoMod.Utils;
using NextBepLoader.Core.Utils;
using NextBepLoader.Deskstop;
using NextBepLoader.Deskstop.Utils;

// ReSharper disable once CheckNamespace
namespace Doorstop;

internal static class Entrypoint
{
    /// <summary>
    ///     The main entrypoint of BepInEx, called from Doorstop.
    /// </summary>
    public static void Start()
    {
        // We set it to the current directory first as a fallback, but try to use the same location as the .exe file.
        var silentExceptionLog = Environment.GetEnvironmentVariable("BEPINEX_PRELOADER_LOG") ??
                                 $"preloader_{DateTime.Now:yyyyMMdd_HHmmss_fff}.log";
        Mutex? mutex = null;

        try
        {
            EnvVars.LoadVars();
            silentExceptionLog =
                Path.Combine(Path.GetDirectoryName(EnvVars.DOORSTOP_PROCESS_PATH)!, silentExceptionLog);

            var mutexId = Utility.HashStrings(Process.GetCurrentProcess().ProcessName,
                                              EnvVars.DOORSTOP_PROCESS_PATH ?? string.Empty,
                                              typeof(Entrypoint).FullName ?? string.Empty);

            mutex = new Mutex(false, $"Global\\{mutexId}");
            mutex.WaitOne();

            if (!DesktopLoader.TryCreateLoader(out var message))
                throw new Exception("Loader Create Load Error:\n" + message);
        }
        catch (Exception ex)
        {
            File.WriteAllText(silentExceptionLog, ex.ToString());

            // Don't exit the game if we have no way of signaling to the user that a crash happened
            if (!Send() && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BEPINEX_FAIL_FAST")))
                return;

            Environment.Exit(1);
        }
        finally
        {
            mutex?.ReleaseMutex();
        }
    }

    private static bool Send()
    {
        if (PlatformDetection.OS.Is(OSKind.Windows))
        {
            MessageBox.Show("Failed to start", "NextBepLoader");
            return true;
        }

        if (NotifySend.IsSupported)
        {
            NotifySend.Send("Failed to start NextBepLoader", "Check logs for details");
            return true;
        }

        return false;
    }
}
