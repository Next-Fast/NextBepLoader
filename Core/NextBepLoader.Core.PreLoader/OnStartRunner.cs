using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NextBepLoader.Core.LoaderInterface;

namespace NextBepLoader.Core.PreLoader;

internal class OnStartRunner(ILogger<OnStartRunner> logger)
{
    public void Run(IServiceProvider provider) =>
        Task.Run(async () =>
        {
            try
            {
                var allStart = provider.GetServices<IOnLoadStart>().ToList();
                allStart.Sort((x, y) => x.Priority.CompareTo(y.Priority));
                foreach (var start in allStart)
                {
                    await start.OnLoadStart();
                    logger.LogInformation($"On LoadStart:{start.GetType().Name}");
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "OnStartRunner error:\n {exception}", e.ToString());
            }
        });
}
