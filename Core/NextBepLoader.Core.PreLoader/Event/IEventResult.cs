namespace NextBepLoader.Core.PreLoader.Event;

public interface IEventResult;

public interface IEventResult<T> : IEventResult;
public class EmptyEventResult : EmptyBase<EmptyEventResult>, IEventResult;
