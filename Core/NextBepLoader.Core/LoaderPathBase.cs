using System.Collections.Generic;
using System.IO;
using System.Linq;
using NextBepLoader.Core.Utils;

namespace NextBepLoader.Core;

public abstract class LoaderPathBase
{
    private readonly List<string> _checkList = [];

    /// <summary>
    ///     The directory that the currently executing process resides in.
    ///     <para>On OSX however, this is the parent directory of the game.app folder.</para>
    /// </summary>
    public abstract string? GameRootPath { get; set; }

    /// <summary>
    ///     The path to the main BepInEx folder.
    /// </summary>
    public string LoaderRootPath { get; set; }

    /// <summary>
    ///     The path to the Managed folder that contains the main managed assemblies.
    /// </summary>
    public string ManagedPath { get; set; }

    /// <summary>
    ///     The path to the game data folder of the currently running Unity game.
    /// </summary>
    public string GameDataPath { get; set; }

    /// <summary>
    ///     The directory that the core BepInEx DLLs reside in.
    /// </summary>
    public string CoreDirectory { get; set; }

    /// <summary>
    ///     The path to the core BepInEx DLL.
    /// </summary>
    public string CoreAssemblyPath { get; set; }

    public string LoaderAssemblyPath { get; set; }

    /// <summary>
    ///     The path of the currently executing program BepInEx is encapsulated in.
    /// </summary>
    public string? ExecutablePath { get; set; }


    /// <summary>
    ///     The path to the config directory.
    /// </summary>
    public string ConfigPath { get; set; }


    /// <summary>
    ///     The path to temporary cache files.
    /// </summary>
    public string CachePath { get; set; }


    /// <summary>
    ///     The path to the plugin folder which resides in the BepInEx folder.
    ///     <para>
    ///         This is ONLY guaranteed to be set correctly when Chainloader has been initialized.
    ///     </para>
    /// </summary>
    public string PluginPath { get; set; }

    /// <summary>
    ///     The name of the currently executing process.
    /// </summary>
    public string ProcessName { get; set; }

    public string DependencyDirectory { get; set; }

    /// <summary>
    ///     List of directories from where Mono will search assemblies before assembly resolving is invoked.
    /// </summary>
    public string[]? DllSearchPaths { get; set; }

    public string ProviderDirectory { get; set; }

    public string UnityBaseDirectory { get; set; }

    public string GameAssemblyPath { get; set; }

    public string GameAssemblyName { get; set; }

    public string IL2CPPInteropAssemblyDirectory { get; set; }

    public string GameMetadataPath { get; set; }

    public string CPP2ILCacheDir { get; set; }

    public string CacheDataDir { get; set; }

    public string TempDir { get; set; }

    public string? SystemDir { get; set; }

    public string LogsDir { get; set; }
    
    public string DataDir { get; set; }

    public virtual void InitPaths(bool autoCheckCreate = false)
    {
        Paths.MainInstance = this;

        LoaderRootPath = SetPath(LoaderRootPath, true, true, nameof(NextBepLoader));
        ProcessName = string.IsNullOrEmpty(ProcessName)
                          ? Path.GetFileNameWithoutExtension(ExecutablePath)
                          : ProcessName;
        GameRootPath = string.IsNullOrEmpty(GameRootPath) ? Path.GetDirectoryName(ExecutablePath) : GameRootPath;
        ManagedPath = SetPath(ManagedPath, false, true, "Managed");
        GameDataPath = SetPath(GameDataPath, false, true, $"{ProcessName}_Data");
        ConfigPath = SetPath(ConfigPath, true, false, LoaderRootPath, "Config");
        CachePath = SetPath(CachePath, true, false, LoaderRootPath, "Cache");
        PluginPath = SetPath(PluginPath, true, true, "Plugins");
        CoreDirectory = SetPath(CoreDirectory, true, false, LoaderRootPath, "Core");
        ProviderDirectory = SetPath(ProviderDirectory, true, true, "Providers");
        CoreAssemblyPath = typeof(Paths).Assembly.Location;
        UnityBaseDirectory = SetPath(UnityBaseDirectory, true, false, LoaderRootPath, "Unity-Libs");
        IL2CPPInteropAssemblyDirectory =
            SetPath(IL2CPPInteropAssemblyDirectory, true, false, LoaderRootPath, "Interop");

        if (DllSearchPaths == null)
        {
            DllSearchPaths = [];
            DllSearchPaths = DllSearchPaths.Concat([ManagedPath]).Distinct().ToArray();
        }

        DependencyDirectory = SetPath(DependencyDirectory, true, false, LoaderRootPath, "Dependencies");
        GameMetadataPath = SetPath(GameMetadataPath, false, false, GameDataPath, "il2cpp_data", "Metadata",
                                   "global-metadata.dat");
        if (string.IsNullOrEmpty(GameAssemblyName))
            GameAssemblyName = $"{CoreUtils.PlatformGameAssemblyName}.{CoreUtils.PlatformPostFix}";

        GameAssemblyPath = SetPath(GameAssemblyPath, false, true, GameAssemblyName);
        CPP2ILCacheDir = SetPath(CPP2ILCacheDir, true, false, LoaderRootPath, "CacheCPP2IL");
        CacheDataDir = SetPath(CacheDataDir, true, false, LoaderRootPath, "CacheData");
        TempDir = SetPath(TempDir, true, false, LoaderRootPath, "Temp");
        LogsDir = SetPath(LogsDir, true, false, LoaderRootPath, "Logs");
        DataDir = SetPath(DataDir, true, false, LoaderRootPath, "Data");

        if (autoCheckCreate)
            CheckCreateDirectories();
    }

    public virtual void CheckCreateDirectories()
    {
        foreach (var path in _checkList.Where(path => !Directory.Exists(path))) Directory.CreateDirectory(path);
    }

    public string SetPath(string org, bool check, bool root = true, params string[] pathNames)
    {
        if (!string.IsNullOrEmpty(org))
            return org;

        var current = pathNames.Aggregate(string.Empty,
                                          (current1, path) =>
                                              current1 == string.Empty ? path : Path.Combine(current1, path));
        var fullPath = root ? Path.Combine(GameRootPath!, current) : current;
        if (check)
            _checkList.Add(fullPath);
        return fullPath;
    }
}
