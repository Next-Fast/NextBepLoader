namespace NextBepLoader.Core.Contract;

public interface INextPlugin
{
    public void Load();

    public bool Unload() => false;
}
