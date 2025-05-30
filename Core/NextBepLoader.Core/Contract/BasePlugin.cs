using NextBepLoader.Core.Contract.Attributes;
using NextBepLoader.Core.LoaderInterface;

namespace NextBepLoader.Core.Contract;

public abstract class BasePlugin : INextPlugin
{
    public PluginMetadata? Metadata { get; internal set; }
    

    public abstract void Load();


    public virtual bool Unload() => false;
}
