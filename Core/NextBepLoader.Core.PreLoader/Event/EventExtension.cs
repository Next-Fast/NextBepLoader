using System;

namespace NextBepLoader.Core.PreLoader.Event;

public static class EventExtension
{
    private static readonly Type EmptyType = typeof(IEmptyEventType);
    public static bool IsEmpty(this IEventResult result) => result.GetType().IsAssignableFrom(EmptyType);
    
    public static bool IsEmpty(this IEventSender sender) => sender.GetType().IsAssignableFrom(EmptyType);

    public static bool IsEmpty(this IEventArg arg) => arg.GetType().IsAssignableFrom(EmptyType);
}
