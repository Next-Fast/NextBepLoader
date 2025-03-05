using NextBepLoader.Core.Configuration;
using NextBepLoader.Core.Contract.Attributes;
using NextBepLoader.Core.Logging.DefaultSource;

namespace NextBepLoader.Core.Contract;

public abstract class BasePlugin : INextPlugin
{
    public PluginMetadata? Metadata { get; internal set; }

    public ManualLogSource Log { get; }

    public ConfigFile Config { get; }

    public abstract void Load();


    public virtual bool Unload() => false;

    public virtual void RegisterApi() { }
}
