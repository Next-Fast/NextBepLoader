using System.Reflection;
using HarmonyLib;
using Microsoft.Win32.SafeHandles;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.Utils;

namespace NextBepLoader.Deskstop.Console.Windows;

internal class WindowsConsoleDriver : IConsoleDivider
{
    // Apparently on some versions of Unity (e.g. 2018.4) using old mono causes crashes on game close if
    // IntPtr overload is used for file streams (check #139).
    // On the other hand, not all Unity games come with SafeFileHandle overload for FileStream
    // As such, we're trying to use SafeFileHandle when it's available and go back to IntPtr overload if not available
    private static readonly ConstructorInfo? FileStreamCtor = new[]
    {
        AccessTools.Constructor(typeof(FileStream), [typeof(SafeFileHandle), typeof(FileAccess)]),
        AccessTools.Constructor(typeof(FileStream), [typeof(IntPtr), typeof(FileAccess)])
    }.FirstOrDefault(m => m != null);

    private readonly Func<int>? getWindowHeight = AccessTools
                                                  .PropertyGetter(typeof(System.Console),
                                                                  nameof(System.Console.WindowHeight))
                                                  ?.CreateDelegate<Func<int>>();

    private readonly Func<int>? getWindowWidth = AccessTools
                                                 .PropertyGetter(typeof(System.Console),
                                                                 nameof(System.Console.WindowWidth))
                                                 ?.CreateDelegate<Func<int>>();

    private int ConsoleWidth
    {
        get
        {
            try
            {
                return getWindowWidth?.Invoke() ?? 0;
            }
            catch (IOException)
            {
                return 0;
            }
        }
    }

    private int ConsoleHeight
    {
        get
        {
            try
            {
                return getWindowHeight?.Invoke() ?? 0;
            }
            catch (IOException)
            {
                return 0;
            }
        }
    }

    public TextWriter StandardOut { get; private set; }

    public bool ConsoleActive { get; private set; }
    public TextWriter? ConsoleOut { get; private set; }


    public void CreateConsole(uint codepage)
    {
        ConsoleWindow.Attach();

        // Make sure of ConsoleEncoding helper class because on some Monos
        // Encoding.GetEncoding throws NotImplementedException on most codepages
        // NOTE: We don't set Console.OutputEncoding because it resets any existing Console.Out writers
        ConsoleEncoding.ConsoleEncoding.ConsoleCodePage = codepage;

        // If stdout exists, write to it, otherwise make it the same as console out
        // Not sure if this is needed? Does the original Console.Out still work?
        var stdout = GetOutHandle();
        if (stdout == IntPtr.Zero)
        {
            StandardOut = TextWriter.Null;
            ConsoleOut = TextWriter.Null;
            return;
        }

        var originalOutStream = OpenFileStream(stdout);
        StandardOut = new StreamWriter(originalOutStream, Utility.UTF8NoBom)
        {
            AutoFlush = true
        };

        var consoleOutStream = OpenFileStream(ConsoleWindow.ConsoleOutHandle);
        ConsoleOut = new StreamWriter(consoleOutStream, Utility.UTF8NoBom)
        {
            AutoFlush = true
        };
        ConsoleActive = true;
    }

    public void DetachConsole()
    {
        ConsoleWindow.Detach();

        ConsoleOut?.Close();
        ConsoleOut = null;

        ConsoleActive = false;
    }

    public void SetConsoleColor(ConsoleColor color)
    {
        SafeConsole.ForegroundColor = color;
        Kon.ForegroundColor = color;
    }

    public void WriteConsoleLine(string message, ConsoleColor color)
    {
        if (ConsoleOut == null) return;
        SetConsoleColor(color);
        ConsoleOut.WriteLine(message);
        SetConsoleColor(ConsoleColor.Gray);
    }

    public void SetConsoleTitle(string title) => ConsoleWindow.Title = title;

    public static void PreventClose() => ConsoleWindow.PreventClose();

    private static Stream OpenFileStream(IntPtr handle)
    {
        if (CoreUtils.IsCore)
        {
            var windowsConsoleStreamType = Type.GetType("System.ConsolePal+WindowsConsoleStream, System.Console", true);
            var constructor = AccessTools.Constructor(windowsConsoleStreamType,
                                                      [typeof(IntPtr), typeof(FileAccess), typeof(bool)]);
            return (Stream)constructor.Invoke([handle, FileAccess.Write, true]);
        }

        var fileHandle = new SafeFileHandle(handle, false);
        var ctorParams = AccessTools.ActualParameters(FileStreamCtor,
        [
            fileHandle, fileHandle.DangerousGetHandle(),
            FileAccess.Write
        ]);
        return (FileStream)Activator.CreateInstance(typeof(FileStream), ctorParams)!;
    }

    private IntPtr GetOutHandle() =>
        /*switch (ConsoleManager.ConfigConsoleOutRedirectType.Value)
        {
            case ConsoleManager.ConsoleOutRedirectType.ConsoleOut:
                return ConsoleWindow.ConsoleOutHandle;
            case ConsoleManager.ConsoleOutRedirectType.StandardOut:
                return ConsoleWindow.OriginalStdoutHandle;
            case ConsoleManager.ConsoleOutRedirectType.Auto:
            default:
                return ConsoleWindow.OriginalStdoutHandle != IntPtr.Zero
                           ? ConsoleWindow.OriginalStdoutHandle
                           : ConsoleWindow.ConsoleOutHandle;
        }*/
        ConsoleWindow.OriginalStdoutHandle != IntPtr.Zero
            ? ConsoleWindow.OriginalStdoutHandle
            : ConsoleWindow.ConsoleOutHandle;
}
