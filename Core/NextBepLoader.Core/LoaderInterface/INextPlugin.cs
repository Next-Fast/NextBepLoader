namespace NextBepLoader.Core.LoaderInterface;

public interface INextPlugin
{
    public void Load();

    public bool Unload() => false;
}
