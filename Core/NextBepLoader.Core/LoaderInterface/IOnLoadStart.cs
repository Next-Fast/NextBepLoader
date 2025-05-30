using System.Threading.Tasks;

namespace NextBepLoader.Core.LoaderInterface;

public interface IOnLoadStart
{
    public int Priority => 0;
    public void OnLoadStart();
}
