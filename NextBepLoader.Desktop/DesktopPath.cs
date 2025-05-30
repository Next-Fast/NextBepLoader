using MonoMod.Utils;
using NextBepLoader.Core;
using NextBepLoader.Core.Utils;
using NextBepLoader.Deskstop.Utils;

namespace NextBepLoader.Deskstop;

public class DesktopPath : LoaderPathBase
{
    public override string? GameRootPath { get; set; }

    public override void InitPaths(bool autoCheckCreate = false)
    {
        LoaderRootPath =
            Path.GetDirectoryName(Path.GetDirectoryName(Path.GetFullPath(EnvVars.DOORSTOP_INVOKE_DLL_PATH))) ??
            string.Empty;
        ExecutablePath = EnvVars.DOORSTOP_PROCESS_PATH;
        ProcessName = Path.GetFileNameWithoutExtension(ExecutablePath)!;
        GameRootPath = PlatformDetection.OS.Is(OSKind.OSX)
                           ? Utility.ParentDirectory(ExecutablePath, 4)
                           : Path.GetDirectoryName(ExecutablePath); 
        ManagedPath = EnvVars.DOORSTOP_MANAGED_FOLDER_DIR ?? Path.Combine(GameDataPath, "Managed");
        DllSearchPaths = EnvVars.DOORSTOP_DLL_SEARCH_DIRS.Concat([ManagedPath]).Distinct().ToArray();
        LoaderAssemblyPath = typeof(DesktopPath).Assembly.Location;

        if (PlatformDetection.OS.Is(OSKind.Windows))
        {
            SystemDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                     nameof(NextBepLoader));
            TempDir = Path.Combine(SystemDir, "Temp");
        }

        base.InitPaths(autoCheckCreate);
    }
}
