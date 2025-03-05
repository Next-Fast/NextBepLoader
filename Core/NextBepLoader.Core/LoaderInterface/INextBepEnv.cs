using System;

namespace NextBepLoader.Core.LoaderInterface;

public interface INextBepEnv
{
    public INextBepEnv RegisterSystemEnv(string variable, string value);
    public INextBepEnv RegisterEventArgs<T>(T arg) where T : EventArgs;

    public T? GetEventArgs<T>() where T : EventArgs;

    public T GetOrCreateEventArgs<T>() where T : EventArgs, new();


    public INextBepEnv UpdateEventArgs<T>(T arg) where T : EventArgs;
}
