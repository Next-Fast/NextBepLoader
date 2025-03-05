using System;
using System.Collections.Generic;
using NextBepLoader.Core.Logging.Extensions;
using NextBepLoader.Core.Logging.Interface;
using MicrosoftLogging = Microsoft.Extensions.Logging;

namespace NextBepLoader.Core.Logging;

public class NextLoggerProvider : MicrosoftLogging.ILoggerProvider
{
    private readonly List<NextLogger> loggers = [];

    public void Dispose()
    {
        foreach (var logger in loggers)
            logger.Dispose();
        loggers.Clear();
    }

    public MicrosoftLogging.ILogger CreateLogger(string categoryName)
    {
        var logger = new NextLogger(categoryName);
        logger.Register();
        return logger;
    }

    public class NextLogger(string name) : ILogSource, MicrosoftLogging.ILogger
    {
        public void Log<TState>(MicrosoftLogging.LogLevel logLevel,
                                MicrosoftLogging.EventId eventId,
                                TState state,
                                Exception? exception,
                                Func<TState, Exception?, string> formatter)
        {
            var logLine = state?.ToString() ?? string.Empty;

            if (exception != null)
                logLine += $"\nException: {exception}";

            LogEvent.Invoke(this, new LogEventArgs(logLine, MSLogLevelTo(logLevel),
                                                   this));
        }

        public bool IsEnabled(MicrosoftLogging.LogLevel logLevel) => Logger.Sources.Contains(this);

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => new EmptyScope();

        public void Dispose() => Logger.Sources.Remove(this);

        public string SourceName { get; } = name;
        public event EventHandler<LogEventArgs> LogEvent;

        private static LogLevel MSLogLevelTo(MicrosoftLogging.LogLevel logLevel) => logLevel switch
        {
            MicrosoftLogging.LogLevel.Trace => LogLevel.Debug,
            MicrosoftLogging.LogLevel.Debug => LogLevel.Debug,
            MicrosoftLogging.LogLevel.Information => LogLevel.Info,
            MicrosoftLogging.LogLevel.Warning => LogLevel.Warning,
            MicrosoftLogging.LogLevel.Error => LogLevel.Error,
            MicrosoftLogging.LogLevel.Critical => LogLevel.Fatal,
            MicrosoftLogging.LogLevel.None => LogLevel.None,
            var _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };

        private class EmptyScope : IDisposable
        {
            public void Dispose() { }
        }
    }
}
