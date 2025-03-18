using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NextBepLoader.Core.LoaderInterface;

namespace NextBepLoader.Core;

public static class LoaderInstance
{
    public static readonly InstanceRegister<ILoaderBase> LoaderRegister = new();
    public static readonly InstanceRegister<INextServiceManager> ManagerRegister = new();
    public static readonly InstanceRegister<IProvider> ProviderRegister = new();
    public static readonly InstanceRegister<IConsoleManager> ConsoleRegister = new();

    public static void Register(ILoaderBase loader) => LoaderRegister.Register(loader);

    public static T? GetLoader<T>() where T : class, ILoaderBase
    {
        LoaderRegister.TryGet<T>(out var result);
        return result;
    }

    public static T GetOrCreateManager<T>() where T : class, INextServiceManager, new() =>
        ManagerRegister.GetOrCreateCurrent<T>();
}

public class InstanceRegister<T>
{
    private readonly List<T> _instances = [];
    public T Current { get; private set; }

    public TGet GetOrCreateCurrent<TGet>() where TGet : class, T, new()
    {
        if (_instances.Count == 0)
            Register(new TGet());
        return (Current as TGet)!;
    }

    public void Register(T instance)
    {
        _instances.Add(instance);
        Current = instance;
    }

    public void RegisterCurrent(T instance) => Current = instance;

    public bool TryGet<TGet>([MaybeNullWhen(false)] out TGet result) where TGet : class, T
    {
        result = null;
        var instance = _instances.FirstOrDefault(n => n is TGet);
        if (instance is not TGet get) return false;
        result = get;
        return true;
    }

    public static explicit operator T(InstanceRegister<T> register) => register.Current;
    public static implicit operator List<T>(InstanceRegister<T> register) => register._instances;
}
