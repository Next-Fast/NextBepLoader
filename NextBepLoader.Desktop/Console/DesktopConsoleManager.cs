using System.Text;
using NextBepLoader.Core;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.Logging;
using NextBepLoader.Core.Logging.DefaultListener;
using NextBepLoader.Core.Logging.Extensions;
using NextBepLoader.Core.Logging.Interface;
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
