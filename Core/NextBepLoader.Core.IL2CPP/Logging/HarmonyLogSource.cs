using System;
using System.Collections.Generic;
using NextBepLoader.Core.Logging;
using NextBepLoader.Core.Logging.Interface;
using HarmonyLogger = HarmonyLib.Tools.Logger;

namespace NextBepLoader.Core.IL2CPP.Logging;

public class HarmonyLogSource : ILogSource
{
    /*
    private static readonly ConfigEntry<HarmonyLogger.LogChannel> LogChannels = ConfigFile.CoreConfig.Bind(
         "Harmony.Logger",
         "LogChannels",
         HarmonyLogger.LogChannel.Warn | HarmonyLogger.LogChannel.Error,
         "Specifies which Harmony log channels to listen to.\nNOTE: IL channel dumps the whole patch methods, use only when needed!");
         */

    private static readonly Dictionary<HarmonyLogger.LogChannel, LogLevel> LevelMap = new()
    {
        [HarmonyLogger.LogChannel.Info] = LogLevel.Info,
        [HarmonyLogger.LogChannel.Warn] = LogLevel.Warning,
        [HarmonyLogger.LogChannel.Error] = LogLevel.Error,
        [HarmonyLogger.LogChannel.IL] = LogLevel.Debug
    };

    public HarmonyLogSource()
    {
        HarmonyLogger.ChannelFilter = HarmonyLogger.LogChannel.Warn | HarmonyLogger.LogChannel.Error;
        HarmonyLogger.MessageReceived += HandleHarmonyMessage;
    }

    public void Dispose() => HarmonyLogger.MessageReceived -= HandleHarmonyMessage;

    public string SourceName { get; } = "HarmonyX";
    public event EventHandler<LogEventArgs> LogEvent;

    private void HandleHarmonyMessage(object? sender, HarmonyLogger.LogEventArgs e)
    {
        if (!LevelMap.TryGetValue(e.LogChannel, out var level))
            return;

        LogEvent(this, new LogEventArgs(e.Message, level, this));
    }
}
