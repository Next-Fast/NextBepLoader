using System.Diagnostics;
using NextBepLoader.Core.LoaderInterface;

namespace NextBepLoader.Deskstop;

public class DesktopBepEnv : INextBepEnv, IOnLoadStart
{
    private Action<DesktopBepEnv> onExited = env => {};
    private readonly Dictionary<string, string> systemEnvs = new();
    private readonly Dictionary<Type, object> actions = new();


    public INextBepEnv RegisterSystemEnv(string variable, string value)
    {
        systemEnvs.Add(variable, value);
        Environment.SetEnvironmentVariable(variable, value);
        return this;
    }

    public Process CurrentProcess { get; set; }

    public INextBepEnv RegisterEventArgs<T>(T arg) where T : EventArgs
    {
        actions.Add(typeof(T), arg);
        return this;
    }

    public T? GetEventArgs<T>() where T : EventArgs
    {
        return actions.FirstOrDefault(n => n.Key == typeof(T)).Value as T;
    }

    public T GetOrCreateEventArgs<T>() where T : EventArgs, new()
    {
        if (actions.TryGetValue(typeof(T), out var value))
        {
            return (T)value;
        }

        var t = new T();
        actions.Add(typeof(T), t);
        return t;
    }

    public INextBepEnv UpdateEventArgs<T>(T arg) where T : EventArgs
    {
        actions[typeof(T)] = arg;
        return this;
    }


    public Task OnLoadStart()
    {
        CurrentProcess = Process.GetCurrentProcess();

        CurrentProcess.Exited += OnExit;
        return Task.CompletedTask;
    }

    private  void OnExit(object? sender, EventArgs e)
    {
        foreach (var (variable, value) in systemEnvs)
            Environment.SetEnvironmentVariable(variable, null);
        systemEnvs.Clear();
        
        onExited(this);
    }
}
