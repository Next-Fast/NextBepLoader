using System;
using Microsoft.Extensions.DependencyInjection;

namespace NextBepLoader.Core.PreLoader;

public abstract class BasePreLoader
{
    public virtual Type[] WaitLoadLoader { get; set; } = [];
    public virtual PreLoadPriority Priority { get; set; } = PreLoadPriority.Default;
    
    public virtual void Start() { }

    public virtual void PreLoad(PreLoadEventArg arg) { }

    public virtual void LoaderServiceBuild(IServiceCollection collection, ILoaderBase loaderBase) { }

    public virtual void PluginServiceBuild(IServiceCollection collection, ILoaderBase loaderBase) { }
}
