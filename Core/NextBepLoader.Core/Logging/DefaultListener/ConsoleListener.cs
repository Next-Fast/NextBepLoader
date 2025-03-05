using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.Logging.Extensions;
using NextBepLoader.Core.Logging.Interface;

namespace NextBepLoader.Core.Logging.DefaultListener;

public class ConsoleListener(
    IConsoleDivider divider,
    LogLevel level = LogLevel.Fatal
                   | LogLevel.Error
                   | LogLevel.Warning
                   | LogLevel.Message
                   | LogLevel.Info) : ILogListener
{
    public IConsoleDivider Divider => divider;

    public LogLevel LogLevelFilter => level;

    public void LogEvent(object sender, LogEventArgs eventArgs) =>
        divider.WriteConsoleLine(eventArgs.ToStringLine(), eventArgs.Level.GetConsoleColor());

    public void Dispose()
    {
        if (Logger.Listeners.Contains(this))
            Logger.Listeners.Remove(this);
    }
}
