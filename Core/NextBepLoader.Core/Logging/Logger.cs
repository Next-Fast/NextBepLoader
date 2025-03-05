using System.Linq;
using NextBepLoader.Core.Logging.BepInExLogHandlers;
using NextBepLoader.Core.Logging.DefaultSource;
using NextBepLoader.Core.Logging.Interface;
using NextBepLoader.Core.Utils;

namespace NextBepLoader.Core.Logging;

/// <summary>
///     Handles pub-sub event marshalling across all log listeners and sources.
/// </summary>
public static class Logger
{
    /// <summary>
    ///     Collection of all log source that output log events.
    /// </summary>
    public static readonly NextEventList<ILogSource> Sources = new(OnSourceEventList);

    /// <summary>
    ///     Collection of all log listeners that receive log events.
    /// </summary>
    public static readonly NextEventList<ILogListener> Listeners = new(OnListenerEventList);

    private static readonly ManualLogSource MainLogSource = CreateLogSource(nameof(NextBepLoader));


    /// <summary>
    ///     Log levels that are currently listened to by at least one listener.
    /// </summary>
    public static LogLevel ListenedLogLevels { get; private set; }


    private static void OnSourceEventList(ListEventType type, ILogSource? item)
    {
        if (item == null) return;
        switch (type)
        {
            case ListEventType.Add:
                item.LogEvent += InternalLogEvent;
                break;

            case ListEventType.Clear:
            {
                foreach (var source in Sources)
                    source.LogEvent -= InternalLogEvent;
                break;
            }

            case ListEventType.Remove:
                item.LogEvent -= InternalLogEvent;
                break;
        }
    }

    private static void OnListenerEventList(ListEventType type, ILogListener? item)
    {
        if (item == null) return;
        switch (type)
        {
            case ListEventType.Add:
                ListenedLogLevels |= item.LogLevelFilter;
                break;

            case ListEventType.Clear:
                ListenedLogLevels = LogLevel.None;
                break;
            case ListEventType.Remove:
                ListenedLogLevels = LogLevel.None;
                foreach (var listener in Listeners)
                    ListenedLogLevels |= listener.LogLevelFilter;
                break;
        }
    }

    private static void InternalLogEvent(object sender, LogEventArgs eventArgs)
    {
        foreach (var listener in Listeners.Where(
                                                 listener =>
                                                     (eventArgs.Level & listener.LogLevelFilter)
                                                   !=
                                                     LogLevel.None)
                )
            listener.LogEvent(sender, eventArgs);
    }

    /// <summary>
    ///     Logs an entry to the internal logger instance.
    /// </summary>
    /// <param name="level">The level of the entry.</param>
    /// <param name="data">The data of the entry.</param>
    internal static void Log(LogLevel level, object data) => MainLogSource.Log(level, data);

    /// <summary>
    ///     Logs an entry to the internal logger instance if any log listener wants the message.
    /// </summary>
    /// <param name="level">The level of the entry.</param>
    /// <param name="logHandler">Log handler to resolve log from.</param>
    internal static void Log(LogLevel level,
                             [InterpolatedStringHandlerArgument("level")]
                             BepInExLogInterpolatedStringHandler logHandler) =>
        MainLogSource.Log(level, logHandler);

    /// <summary>
    ///     Logs a message with <see cref="LogLevel.Fatal" /> level.
    /// </summary>
    /// <param name="data">Data to log.</param>
    public static void LogFatal(object data) => Log(LogLevel.Fatal, data);

    /// <summary>
    ///     Logs an interpolated string with <see cref="LogLevel.Fatal" /> level.
    /// </summary>
    /// <param name="logHandler">Handler for the interpolated string.</param>
    public static void LogFatal(BepInExFatalLogInterpolatedStringHandler logHandler) => Log(LogLevel.Fatal, logHandler);

    /// <summary>
    ///     Logs a message with <see cref="LogLevel.Error" /> level.
    /// </summary>
    /// <param name="data">Data to log.</param>
    public static void LogError(object data) => Log(LogLevel.Error, data);

    /// <summary>
    ///     Logs an interpolated string with <see cref="LogLevel.Error" /> level.
    /// </summary>
    /// <param name="logHandler">Handler for the interpolated string.</param>
    public static void LogError(BepInExErrorLogInterpolatedStringHandler logHandler) => Log(LogLevel.Error, logHandler);

    /// <summary>
    ///     Logs a message with <see cref="LogLevel.Warning" /> level.
    /// </summary>
    /// <param name="data">Data to log.</param>
    public static void LogWarning(object data) => Log(LogLevel.Warning, data);

    /// <summary>
    ///     Logs an interpolated string with <see cref="LogLevel.Warning" /> level.
    /// </summary>
    /// <param name="logHandler">Handler for the interpolated string.</param>
    public static void LogWarning(BepInExWarningLogInterpolatedStringHandler logHandler) =>
        Log(LogLevel.Warning, logHandler);

    /// <summary>
    ///     Logs a message with <see cref="LogLevel.Message" /> level.
    /// </summary>
    /// <param name="data">Data to log.</param>
    public static void LogMessage(object data) => Log(LogLevel.Message, data);

    /// <summary>
    ///     Logs an interpolated string with <see cref="LogLevel.Message" /> level.
    /// </summary>
    /// <param name="logHandler">Handler for the interpolated string.</param>
    public static void LogMessage(BepInExMessageLogInterpolatedStringHandler logHandler) =>
        Log(LogLevel.Message, logHandler);

    /// <summary>
    ///     Logs a message with <see cref="LogLevel.Info" /> level.
    /// </summary>
    /// <param name="data">Data to log.</param>
    public static void LogInfo(object data) => Log(LogLevel.Info, data);

    /// <summary>
    ///     Logs an interpolated string with <see cref="LogLevel.Info" /> level.
    /// </summary>
    /// <param name="logHandler">Handler for the interpolated string.</param>
    public static void LogInfo(BepInExInfoLogInterpolatedStringHandler logHandler) => Log(LogLevel.Info, logHandler);

    /// <summary>
    ///     Logs a message with <see cref="LogLevel.Debug" /> level.
    /// </summary>
    /// <param name="data">Data to log.</param>
    public static void LogDebug(object data) => Log(LogLevel.Debug, data);

    /// <summary>
    ///     Logs an interpolated string with <see cref="LogLevel.Debug" /> level.
    /// </summary>
    /// <param name="logHandler">Handler for the interpolated string.</param>
    public static void LogDebug(BepInExDebugLogInterpolatedStringHandler logHandler) => Log(LogLevel.Debug, logHandler);

    /// <summary>
    ///     Creates a new log source with a name and attaches it to <see cref="Sources" />.
    /// </summary>
    /// <param name="sourceName">Name of the log source to create.</param>
    /// <returns>An instance of <see cref="ManualLogSource" /> that allows to write logs.</returns>
    public static ManualLogSource CreateLogSource(string sourceName)
    {
        var source = new ManualLogSource(sourceName);
        Sources.Add(source);
        return source;
    }
}
