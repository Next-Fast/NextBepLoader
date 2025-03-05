using System;
using System.IO.Compression;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.PreLoader;

namespace NextBepLoader.Core.IL2CPP.NextPreLoaders;

public class UnityBasePreDownloader(
    HttpClient client,
    ILogger<UnityBasePreDownloader> logger,
    INextBepEnv env,
    UnityInfo unityInfo) : BasePreLoader
{
    public override Type[] WaitLoadLoader => [typeof(IL2CPPPreLoader)];

    /*private static readonly ConfigEntry<string> UnityBaseLibrariesSource = ConfigFile.CoreConfig.Bind(
                                                                                                      "IL2CPP", "UnityBaseLibrariesSource",
                                                                                                      "https://unity.bepinex.dev/libraries/{VERSION}.zip",
                                                                                                      new StringBuilder()
                                                                                                          .AppendLine("URL to the ZIP of managed Unity base libraries.")
                                                                                                          .AppendLine("The base libraries are used by Il2CppInterop to generate interop assemblies.")
                                                                                                          .AppendLine("The URL can include {VERSION} template which will be replaced with the game's Unity engine version.")
                                                                                                          .ToString());*/

    public override async void PreLoad(PreLoadEventArg arg)
    {
        if (!env.GetOrCreateEventArgs<IL2CPPCheckEventArg>().DownloadUnityBaseLib) return;
        var unityVersion = unityInfo.GetVersion();
        var source =
            "https://unity.bepinex.dev/libraries/{VERSION}.zip".Replace("{VERSION}",
                                                                        $"{unityVersion.Major}.{unityVersion.Minor}.{unityVersion.Build}");
        logger.LogInformation("Unity Base Lib Download Source: {source}", source);

        if (string.IsNullOrEmpty(source)) return;
        logger.LogInformation("Downloading unity base libraries");

        await using var zipStream = await client.GetStreamAsync(source);
        using var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read);

        logger.LogInformation("Extracting downloaded unity base libraries to {dir}", Paths.UnityBaseDirectory);
        zipArchive.ExtractToDirectory(Paths.UnityBaseDirectory, true);
    }
}
