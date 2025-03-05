using System.Threading.Tasks;

namespace NextBepLoader.Core.LoaderInterface;

public interface IOnLoadStart
{
    public virtual int Priority => 0;
    public Task OnLoadStart();
}
