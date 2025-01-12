using System.Collections.Generic;
using AsmResolver.DotNet;
using Microsoft.Extensions.DependencyInjection;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.PreLoader.Bootstrap;

namespace NextBepLoader.Core.PreLoader;

public class LoadProviderBase<TPlugin>: IProvider
{
    public static LoadProviderBase<TPlugin> Instance;
    public readonly DotNetLoader _DotNetLoader;
    public readonly FastTypeFinder Finder = new();
    public List<TPlugin> AllSelect = [];
    
    protected LoadProviderBase(DotNetLoader loader)
    {
        Instance = this;
        _DotNetLoader = loader;
    }

    public virtual void Init(IProviderManager manager)
    {
    }

    public virtual void Run()
    {
        AllSelect = Finder
                    .FindFormTypeLoader(_DotNetLoader, IsTarget)
                    .Where(PreFilter)
                    .SelectTo(Selector);
    }
    public virtual void OnGameActive(){}

    protected virtual TPlugin? Selector(FastTypeFinder.FindInfo info)
    {
        return default;
    }

    protected virtual bool PreFilter(FastTypeFinder.FindInfo info)
    {
        return true;
    }

    protected virtual bool IsTarget(TypeDefinition type)
    {
        return true;
    }
}
