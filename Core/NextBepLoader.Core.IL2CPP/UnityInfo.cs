using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetRipper.Primitives;
using NextBepLoader.Core.IL2CPP.Utils;

namespace NextBepLoader.Core.IL2CPP;

/// <summary>
///     Various information about the currently executing Unity player.
/// </summary>
public class UnityInfo
{
    private static UnityInfo? instance;


    // Adapted from https://github.com/SamboyCoding/Cpp2IL/blob/development/LibCpp2IL/LibCpp2IlMain.cs
    private static readonly ManagerLookup[] DefaultManagerVersionLookup =
    [
        new("globalgamemanagers", 0x14, 0x30),
        new("data.unity3d", 0x12),
        new("mainData", 0x14)
    ];

    public static UnityInfo Instance => instance ??= new UnityInfo();

    private List<ManagerLookup> ManagerVersionLookup { get; } = [..DefaultManagerVersionLookup];

    /// <summary>
    ///     Version of the Unity player
    /// </summary>
    /// <remarks>
    ///     Because BepInEx can execute very early, the exact Unity version might not be available in early
    ///     bootstrapping phases. The version should be treated as an estimation of the actual version of the Unity player.
    /// </remarks>
    private UnityVersion? Version { get; set; }

    internal void InitializeFormPaths() =>
        Initialize(Paths.ExecutablePath, Paths.GameDataPath);

    internal void Initialize(string unityPlayerPath, string gameDataPath)
    {
        var playerPath = Path.GetFullPath(unityPlayerPath);
        var dataPath = Path.GetFullPath(gameDataPath);

        foreach (var lookup in ManagerVersionLookup.Where(lookup => lookup.SetFileRootPath(dataPath).TryLookup()))
        {
            Version = (UnityVersion)lookup;
            break;
        }
    }

    private bool TryInitialize()
    {
        InitializeFormPaths();
        return Version != null;
    }

    public void SetRuntimeUnityVersion(string version) => Version = UnityVersion.Parse(version);
    public void SetRuntimeUnityVersion(UnityVersion version) => Version = version;

    public UnityVersion GetVersion()
    {
        if (Version == null)
            InitializeFormPaths();

        return Version ?? throw new Exception("unity version not initialized");
    }

    public Version ToVersion()
    {
        var version = GetVersion();
        return new Version(version.Major, version.Minor, version.Build);
    }

    public static implicit operator UnityVersion(UnityInfo info) => info.GetVersion();

    public static implicit operator Version(UnityInfo info) => info.ToVersion();
}
