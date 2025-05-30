using System.Diagnostics;
using System.Text.Json;
using NextBepLoader.Core;
using NextBepLoader.Core.LoaderInterface;

namespace NextBepLoader.Deskstop;

public class DesktopBepEnv : INextBepEnv, IOnLoadStart
{
    private readonly Dictionary<Type, object> _actions = new();
    private readonly Action<DesktopBepEnv> _onExited = env => { };
    private readonly Dictionary<string, string> _systemEnvs = new();

    public Process CurrentProcess { get; set; }
    public int Priority => 0;


    public INextBepEnv RegisterSystemEnv(string variable, string value)
    {
        _systemEnvs.Add(variable, value);
        Environment.SetEnvironmentVariable(variable, value);
        return this;
    }

    public INextBepEnv RegisterEventArgs<T>(T arg) where T : EventArgs
    {
        _actions.Add(typeof(T), arg);
        return this;
    }

    public T? GetEventArgs<T>() where T : EventArgs => _actions.FirstOrDefault(n => n.Key == typeof(T)).Value as T;

    public T GetOrCreateEventArgs<T>() where T : EventArgs, new()
    {
        if (_actions.TryGetValue(typeof(T), out var value)) return (T)value;

        var t = new T();
        _actions.Add(typeof(T), t);
        return t;
    }

    public INextBepEnv UpdateEventArgs<T>(T arg) where T : EventArgs
    {
        _actions[typeof(T)] = arg;
        return this;
    }


    public void OnLoadStart()
    {
        CurrentProcess = Process.GetCurrentProcess();

        CurrentProcess.Exited += OnExit;
        var dic = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(CachePath));
        if (dic == null) return;
        foreach (var (key, value) in dic)
        {
            RegisterSystemEnv(key, value);
        }
    }

    private static string CachePath => Path.Combine(Paths.CachePath, "SystemEnv.cache");
    private void OnExit(object? sender, EventArgs e)
    {
        File.WriteAllText(CachePath, JsonSerializer.Serialize(_systemEnvs));
        foreach (var (variable, value) in _systemEnvs)
            Environment.SetEnvironmentVariable(variable, null);
        _systemEnvs.Clear();
        _onExited(this);
    }
}
