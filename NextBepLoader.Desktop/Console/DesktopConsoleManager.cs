using System.Text;
using NextBepLoader.Core;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.Logging;
using NextBepLoader.Core.Logging.DefaultListener;
using NextBepLoader.Deskstop.Console.Windows;

namespace NextBepLoader.Deskstop.Console;

public class DesktopConsoleManager : IConsoleManager
{
    public static DesktopConsoleManager Instance =>
        LoaderInstance.ConsoleRegister.GetOrCreateCurrent<DesktopConsoleManager>();
    
    public ConsoleConfig ConsoleConfig { get; private set; }
    public ILogListener? LoggerListener { get; private set; }

    public IConsoleManager Init(ConsoleConfig config)
    {
        ConsoleConfig = config;
        CreateDivider(config);
        return this;
    }

    public IConsoleManager CreateConsole()
    {
        if (Divider == null)
            CreateDivider(ConsoleConfig);

        if (Divider != null)
        {
            Divider.CreateConsole((uint)Encoding.UTF8.CodePage);
            ActiveConsole = true;
            LoggerListener = new ConsoleListener(Divider).Register();
        }
        else
        {
            Logger.LogError("Failed to create console NoDriver");
        }
        return this;
    }

    public IConsoleManager CloseConsole()
    {
        if (Divider == null)
            CreateDivider(ConsoleConfig);

        if (Divider != null)
        {
            Divider.DetachConsole();
            ActiveConsole = false;
            
            LoggerListener?.Dispose();
            LoggerListener = null;
        }
        else
        {
            Logger.LogError("Failed to close console NoDriver");
        }
        return this;
    }

    public IConsoleDivider CreateDivider(ConsoleConfig config)
    {
        var divider = new WindowsConsoleDriver();
        Divider = divider;
        return Divider;
    }

    public IConsoleDivider? Divider { get; set; }
    public bool EnableConsole { get; set; }
    public bool ActiveConsole { get; set; }
}

/*public static class ConsoleManager
{
    public enum ConsoleOutRedirectType
    {
        [Description("Auto")]
        Auto = 0,

        [Description("Console Out")]
        ConsoleOut,

        [Description("Standard Out")]
        StandardOut
    }

    private const uint SHIFT_JIS_CP = 932;

    private const string ENABLE_CONSOLE_ARG = "--enable-console";

    public static readonly ConfigEntry<bool> ConfigConsoleEnabled = ConfigFile.CoreConfig.Bind(
     "Logging.Console", "Enabled",
     true,
     "Enables showing a console for log output.");

    public static readonly ConfigEntry<bool> ConfigPreventClose = ConfigFile.CoreConfig.Bind(
     "Logging.Console", "PreventClose",
     false,
     "If enabled, will prevent closing the console (either by deleting the close button or in other platform-specific way).");

    public static readonly ConfigEntry<bool> ConfigConsoleShiftJis = ConfigFile.CoreConfig.Bind(
     "Logging.Console", "ShiftJisEncoding",
     false,
     "If true, console is set to the Shift-JIS encoding, otherwise UTF-8 encoding.");

    public static readonly ConfigEntry<ConsoleOutRedirectType> ConfigConsoleOutRedirectType =
        ConfigFile.CoreConfig.Bind(
                                   "Logging.Console", "StandardOutType",
                                   ConsoleOutRedirectType.Auto,
                                   new StringBuilder()
                                       .AppendLine("Hints console manager on what handle to assign as StandardOut. Possible values:")
                                       .AppendLine("Auto - lets BepInEx decide how to redirect console output")
                                       .AppendLine("ConsoleOut - prefer redirecting to console output; if possible, closes original standard output")
                                       .AppendLine("StandardOut - prefer redirecting to standard output; if possible, closes console out")
                                       .ToString()
                                  );

    private static readonly bool? EnableConsoleArgOverride;

    static ConsoleManager()
    {
        // Ensure GetCommandLineArgs failing (e.g. on unix) does not kill bepin
        try
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                var res = args[i];
                if (res == ENABLE_CONSOLE_ARG && i + 1 < args.Length && bool.TryParse(args[i + 1], out var enable))
                    EnableConsoleArgOverride = enable;
            }
        }
        catch (Exception)
        {
            // Skip
        }
    }

    public static bool ConsoleEnabled => EnableConsoleArgOverride ?? ConfigConsoleEnabled.Value;

    internal static IConsoleDriver Driver { get; set; }

    /// <summary>
    ///     True if an external console has been started, false otherwise.
    /// </summary>
    public static bool ConsoleActive => Driver?.ConsoleActive ?? false;

    /// <summary>
    ///     The stream that writes to the standard out stream of the process. Should never be null.
    /// </summary>
    public static TextWriter StandardOutStream => Driver?.StandardOut;

    /// <summary>
    ///     The stream that writes to an external console. Null if no such console exists
    /// </summary>
    public static TextWriter ConsoleStream => Driver?.ConsoleOut;


    public static void Initialize(bool alreadyActive)
    {
        if (PlatformDetection.OS.Is(OSKind.Linux))
            Driver = new LinuxConsoleDriver();
        else if (PlatformDetection.OS.Is(OSKind.Windows))
            Driver = new WindowsConsoleDriver();
        else
            throw new PlatformNotSupportedException("Was unable to determine console driver for platform " +
                                                    PlatformDetection.OS);

        Driver.Initialize(alreadyActive);
    }

    private static void DriverCheck()
    {
        if (Driver == null)
            throw new InvalidOperationException("Driver has not been initialized");
    }

    public static void CreateConsole()
    {
        if (ConsoleActive)
            return;

        DriverCheck();

        // Apparently some versions of Mono throw a "Encoding name 'xxx' not supported"
        // if you use Encoding.GetEncoding
        // That's why we use of codepages directly and handle then in console drivers separately
        var codepage = ConfigConsoleShiftJis.Value ? SHIFT_JIS_CP : (uint) Encoding.UTF8.CodePage;

        Driver.CreateConsole(codepage);

        if (ConfigPreventClose.Value)
            Driver.PreventClose();
    }

    public static void DetachConsole()
    {
        if (!ConsoleActive)
            return;

        DriverCheck();

        Driver.DetachConsole();
    }

    public static void SetConsoleTitle(string title)
    {
        DriverCheck();

        Driver.SetConsoleTitle(title);
    }

    public static void SetConsoleColor(ConsoleColor color)
    {
        DriverCheck();

        Driver.SetConsoleColor(color);
    }
}*/