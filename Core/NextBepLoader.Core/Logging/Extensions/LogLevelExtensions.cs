using System;
using System.Linq;

namespace NextBepLoader.Core.Logging.Extensions;

/// <summary>
///     Helper methods for log level handling.
/// </summary>
public static class LogLevelExtensions
{
    /// <summary>
    ///     Gets the highest log level when there could potentially be multiple levels provided.
    /// </summary>
    /// <param name="levels">The log level(s).</param>
    /// <returns>The highest log level supplied.</returns>
    public static LogLevel GetHighestLevel(this LogLevel levels)
    {
        var enums = Enum.GetValues(typeof(LogLevel));
        Array.Sort(enums);

        return enums.Cast<LogLevel>().FirstOrDefault(e => (levels & e) != LogLevel.None);
    }

    /// <summary>
    ///     Returns a translation of a log level to it's associated console colour.
    /// </summary>
    /// <param name="level">The log level(s).</param>
    /// <returns>A console color associated with the highest log level supplied.</returns>
    public static ConsoleColor GetConsoleColor(this LogLevel level)
    {
        level = GetHighestLevel(level);
        return level switch
        {
            LogLevel.Fatal   => ConsoleColor.Red,
            LogLevel.Error   => ConsoleColor.DarkRed,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Message => ConsoleColor.White,
            LogLevel.Info    => ConsoleColor.DarkGray,
            LogLevel.Debug   => ConsoleColor.DarkGray,
            var _            => ConsoleColor.Gray
        };
    }
}
