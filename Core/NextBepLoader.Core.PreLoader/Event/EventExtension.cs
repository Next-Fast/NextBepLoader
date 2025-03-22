using System;
using System.Linq;

namespace NextBepLoader.Core.PreLoader.Event;

public static class EventExtension
{
    private static readonly Type EmptyType = typeof(IEmptyEventType);
    public static bool IsEmpty(this IEventResult result) => result.HasEmptyType();
    
    public static bool IsEmpty(this IEventSender sender) => sender.HasEmptyType();

    public static bool IsEmpty(this IEventArg arg) => arg.HasEmptyType();

    private static bool HasEmptyType(this object type)
    {
        return type.GetType().GetInterfaces().Any(n => n == EmptyType);
    }
}
