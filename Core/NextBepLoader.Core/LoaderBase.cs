using System;
using NextBepLoader.Core.Contract;
using NextBepLoader.Core.LoaderInterface;

namespace NextBepLoader.Core;

public abstract class LoaderBase<T> : ILoaderBase where T : LoaderBase<T>, new()
{
    protected LoaderBase()
    {
        LoaderInstance.Register(this);
    }

    public static T? Instance { get; private set; }
    public virtual IConsoleManager ConsoleManager { get; set; }

    public virtual INextServiceManager ServiceManager { get; set; }

    public abstract LoaderPathBase Paths { get; set; }
    public abstract LoaderPlatformType LoaderType { get; }
    public virtual Version LoaderVersion { get; set; } = new(1, 0, 0);

    public IServiceProvider MainServices { get; set; }

    public Action<IServiceProvider>? OnServiceBuilt { get; set; }


    public abstract void Start();

    internal static bool TryCreateLoader(out string? message, bool start = true)
    {
        try
        {
            Instance = new T();
            if (start)
                Instance.Start();
        }
        catch (Exception e)
        {
            message = e.ToString();
            return false;
        }

        message = null;
        return true;
    }

    internal static bool TryCreateLoader(bool start = true) => TryCreateLoader(out var _, start);
}

public interface ILoaderBase
{
    public INextServiceManager ServiceManager { get; set; }

    public LoaderPathBase Paths { get; set; }
    public LoaderPlatformType LoaderType { get; }
    public Version LoaderVersion { get; set; }

    public IServiceProvider MainServices { get; set; }

    public Action<IServiceProvider>? OnServiceBuilt { get; set; }

    public void Start();
}
