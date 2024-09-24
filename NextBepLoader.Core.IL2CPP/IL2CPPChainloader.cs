using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Il2CppInterop.Runtime.InteropTypes;
using MonoMod.RuntimeDetour;
using NextBepLoader.Core.Configuration;
using NextBepLoader.Core.Contract;
using NextBepLoader.Core.IL2CPP.Logging;
using NextBepLoader.Core.IL2CPP.Utils;
using NextBepLoader.Core.Logging;
using NextBepLoader.Core.PreLoader;
using UnityEngine;

namespace NextBepLoader.Core.IL2CPP;

public class IL2CPPChainloader/* : BaseChainloader<BasePlugin>*/
{
    private static readonly ConfigEntry<bool> ConfigUnityLogging = ConfigFile.CoreConfig.Bind(
     "Logging", "UnityLogListening",
     true,
     "Enables showing unity log messages in the BepInEx logging system.");

    private static readonly ConfigEntry<bool> ConfigDiskWriteUnityLog = ConfigFile.CoreConfig.Bind(
     "Logging.Disk", "WriteUnityLog",
     false,
     "Include unity log messages in log file output.");
    

    public static IL2CPPChainloader Instance { get; set; }

    /// <summary>
    ///     Register and add a Unity Component (for example MonoBehaviour) into BepInEx global manager.
    ///     Automatically registers the type with Il2Cpp type system if it isn't initialised already.
    /// </summary>
    /// <typeparam name="T">Type of the component to add.</typeparam>
    public static T AddUnityComponent<T>() where T : Il2CppObjectBase => AddUnityComponent(typeof(T)).Cast<T>();

    /// <summary>
    ///     Register and add a Unity Component (for example MonoBehaviour) into BepInEx global manager.
    ///     Automatically registers the type with Il2Cpp type system if it isn't initialised already.
    /// </summary>
    /// <param name="t">Type of the component to add</param>
    public static Il2CppObjectBase AddUnityComponent(Type t) => Il2CppUtils.AddComponent(t);

    /// <summary>
    ///     Occurs after a plugin is instantiated and just before <see cref="BasePlugin.Load" /> is called.
    /// </summary>
    /*public event Action<PluginInfo, Assembly, BasePlugin> PluginLoad;*/

    public /*override */void Initialize(string gameExePath = null)
    {
        /*base.Initialize(gameExePath);*/
        Instance = this;
    }

    /// <summary>
    /// </summary>
    protected/* override */void InitializeLoggers()
    {
        /*base.InitializeLoggers();

        if (!ConfigDiskWriteUnityLog.Value) DiskLogListener.BlacklistedSources.Add("Unity");

        ChainloaderLogHelper.RewritePreloaderLogs();
        Logger.Sources.Add(new IL2CPPLogSource());*/
    }

    /*public/* override #1#BasePlugin? LoadPlugin(PluginInfo pluginInfo, Assembly pluginAssembly)
    {
        Logger.Log(LogLevel.Debug, $"{pluginInfo.TypeName}");
        var type = pluginAssembly.GetType(pluginInfo.TypeName);
        if (type == null)
            return null;

        if (Activator.CreateInstance(type) is not BasePlugin pluginInstance)
            return null;

        PluginLoad(pluginInfo, pluginAssembly, pluginInstance);
        pluginInstance.Load();

        return pluginInstance;
    }*/
    
}