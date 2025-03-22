using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NextBepLoader.Core.LoaderInterface;

namespace NextBepLoader.Core.PreLoader.Event;

public class EventManager(IServiceProvider serviceProvider) : IOnLoadStart
{
    private readonly List<IOnEvent> _events = [];
    private bool _hasSort;
    
    public int Priority => 10;

    public EventManager RegisterEvent(IOnEvent eventHandler)
    {
        _events.Add(eventHandler);
        _hasSort = false;
        return this;
    }

    internal IEventResult CallEvent<TSender>(TSender sender) where TSender : IEventSender
    {
        if (!_hasSort)
        {
            _events.Sort((x, y) => x.Priority.CompareTo(y.Priority));
            _hasSort = true;
        }
        
        foreach (var @event in _events)
        {
            if (@event is not IOnEvent<TSender> eventHandler) continue;
            var result = eventHandler.OnEvent(sender);
            if (result is not EmptyEventResult)
            {
                return result;
            }
        }
        
        return new EmptyEventResult();
    }

    public Task OnLoadStart()
    {
        foreach (var @event in serviceProvider.GetServices<IOnEvent>())
        {
            RegisterEvent(@event);
        }
        
        return Task.CompletedTask;
    }
}

public class EmptyBase<T> : IEmptyEventType where T : class, new()
{
    public static readonly T Instance = new();

    public static T Empty() => Instance;
}

internal interface IEmptyEventType;
