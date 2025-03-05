using System;
using System.IO;

namespace NextBepLoader.Core.LoaderInterface;

public interface IConsoleManager
{
    public IConsoleDivider? Divider { get; set; }

    public bool EnableConsole { get; set; }

    public bool ActiveConsole { get; internal set; }

    public IConsoleManager Init(ConsoleConfig config);

    public IConsoleManager CreateConsole();

    public IConsoleManager CloseConsole();

    public IConsoleDivider CreateDivider(ConsoleConfig config);
}

public record ConsoleConfig { }

public interface IConsoleDivider
{
    TextWriter? ConsoleOut { get; }
    void CreateConsole(uint codepage);
    void DetachConsole();
    void SetConsoleColor(ConsoleColor color);
    void WriteConsoleLine(string message, ConsoleColor color);

    void SetConsoleTitle(string title);
}
