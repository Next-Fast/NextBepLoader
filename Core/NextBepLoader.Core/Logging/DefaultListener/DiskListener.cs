using System.IO;
using NextBepLoader.Core.Logging.Interface;
using NextBepLoader.Core.Utils;

namespace NextBepLoader.Core.Logging.DefaultListener;

public class DiskListener(
    string path,
    LogLevel logLevel = LogLevel.Fatal | LogLevel.Error | LogLevel.Warning | LogLevel.Message | LogLevel.Info)
    : ILogListener
{
    public readonly TextWriter? Writer = CreateWriter(path);
    public LogLevel LogLevelFilter => logLevel;

    public void LogEvent(object sender, LogEventArgs eventArgs) => Writer?.WriteLine(eventArgs.ToString());

    public void Dispose()
    {
        try
        {
            Writer?.Dispose();

            if (Logger.Listeners.Contains(this))
                Logger.Listeners.Remove(this);
        }
        catch
        {
            // ignored
        }
    }

    private static TextWriter? CreateWriter(string path)
    {
        var stream = File.OpenWrite(path);
        var writer = new StreamWriter(stream, Utility.UTF8NoBom)
        {
            AutoFlush = true
        };

        return writer;
    }

    ~DiskListener()
    {
        Dispose();
    }
}
