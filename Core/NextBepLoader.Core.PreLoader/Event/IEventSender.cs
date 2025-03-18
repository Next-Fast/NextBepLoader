namespace NextBepLoader.Core.PreLoader.Event;

public interface IEventSender;

public class HasArgEventSender : IEventSender
{
    private IEventArg? _event;
    public IEventArg? Arg
    {
        set => _event = value;
    }

    public IEventArg EventArg => _event ?? EmptyEventArg.Empty();
}

public class IDEventSender(string id) : HasArgEventSender
{
    public string Id => id;
}
