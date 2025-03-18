namespace NextBepLoader.Core.PreLoader.Event;

public interface IOnEvent
{
    public int Priority  => 0;
    public IEventResult OnEvent(IEventSender sender) => EmptyEventResult.Empty();
}

public interface IOnEvent<in T> : IOnEvent where T : IEventSender
{
    public IEventResult OnEvent(T sender) => EmptyEventResult.Empty();
}

