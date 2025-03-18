using System;
using System.IO.Compression;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.PreLoader;

namespace NextBepLoader.Core.IL2CPP.NextPreLoaders;

internal class UnityBasePreDownloader(
    HttpClient client,
    ILogger<UnityBasePreDownloader> logger,
    INextBepEnv env,
    UnityInfo unityInfo) : BasePreLoader
{
    public override Type[] WaitLoadLoader => [typeof(IL2CPPPreLoader)];

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
