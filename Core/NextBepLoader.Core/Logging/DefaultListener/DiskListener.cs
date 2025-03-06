using System.IO;
using System.Linq;
using NextBepLoader.Core.Logging.Interface;
using NextBepLoader.Core.Utils;

namespace NextBepLoader.Core.Logging.DefaultListener;

public class DiskListener(
    string path,
    LogLevel logLevel = LogLevel.Fatal | LogLevel.Error | LogLevel.Warning | LogLevel.Message | LogLevel.Info)
    : ILogListener
{
    private readonly TextWriter? _logsWriter
        = CreateWriter(Paths.LogsDir, $"{CoreUtils.TimeStamp}.log");

    private readonly TextWriter? _writer
        = CreateWriter(path);

    public LogLevel LogLevelFilter => logLevel;

    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        var text = eventArgs.ToString();

        _writer?.WriteLine(text);
        _logsWriter?.WriteLine(text);
    }

    public void Dispose()
    {
        try
        {
            _writer?.Dispose();

            if (Logger.Listeners.Contains(this))
                Logger.Listeners.Remove(this);
        }
        catch
        {
            // ignored
        }
    }

    private static TextWriter? CreateWriter(params string[] path)
    {
        var combinePath = path.Aggregate(Path.Combine);
        var stream = File.OpenWrite(combinePath);
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
