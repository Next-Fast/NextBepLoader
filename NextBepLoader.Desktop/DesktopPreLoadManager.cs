using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.PreLoader;
using NextBepLoader.Core.PreLoader.Bootstrap;
using NextBepLoader.Core.PreLoader.NextPreLoaders;

namespace NextBepLoader.Deskstop;

public sealed class DesktopPreLoadManager(
    ILogger<DesktopPreLoadManager> logger,
    IServiceProvider provider,
    DesktopLoader loader,
    DotNetLoader dotNetLoader) : IPreLoaderManager, IOnLoadStart
{
    public List<BasePreLoader> PreLoaders { get; set; } = [];
    public int Priority => 0;

    public async Task OnLoadStart()
    {
        PreLoaders.AddRange(provider.GetServices<BasePreLoader>());

        var findTypes = new FastTypeFinder()
                        .FindFormTypeLoader(dotNetLoader,
                                            type => type.BaseType?.FullName == typeof(BasePreLoader).FullName)
                        ._AllFindInfo.Select(n => n.AssemblyType);

        foreach (var preLoader in findTypes)
            try
            {
                if (preLoader == null) continue;
                if (ActivatorUtilities.CreateInstance(provider, preLoader) is not BasePreLoader instance) continue;
                PreLoaders.Add(instance);
            }
            catch (Exception e)
            {
                logger.LogWarning("PreLoader:{name} Load Error\n {exception}", preLoader?.Name, e.ToString());
            }

        PreLoaders.SortLoaders();
        await LoadPreLoad();
    }

    public T? GetPreLoader<T>() where T : BasePreLoader => PreLoaders.FirstOrDefault(n => n is T) as T;

    private Task LoadPreLoad()
    {
        foreach (var preLoader in PreLoaders)
            try
            {
                logger.LogInformation("Run PreLoader:{name}", preLoader.GetType().Name);
                preLoader.PreLoad(loader.PreLoadEventArg);
            }
            catch (Exception e)
            {
                logger.LogError(
                                e,
                                "PreLoader:{name} PreLoad Error\n {exception}",
                                preLoader.GetType().Name,
                                e.ToString()
                               );
            }

        foreach (var preLoader in PreLoaders)
            try
            {
                preLoader.Start();
            }
            catch (Exception e)
            {
                logger.LogError(
                                e,
                                "PreLoader:{name} Start Error\n {exception}",
                                preLoader.GetType().Name,
                                e.ToString()
                               );
            }

        foreach (var preLoader in PreLoaders)
            try
            {
                preLoader.Finish();
            }
            catch (Exception e)
            {
                logger.LogError(
                                e,
                                "PreLoader:{name} Finish Error\n {exception}",
                                preLoader.GetType().Name,
                                e.ToString()
                               );
            }

        return Task.CompletedTask;
    }
}
