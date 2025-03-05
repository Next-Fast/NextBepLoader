using System;

namespace NextBepLoader.Core.LoaderInterface;

public interface IProviderManager
{
    public IServiceProvider MainServiceProvider { get; }
    public T? GetProvider<T>() where T : IProvider;

    public void OnGameActive();
}
