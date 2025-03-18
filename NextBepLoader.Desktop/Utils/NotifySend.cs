using System.Diagnostics;

namespace NextBepLoader.Deskstop.Utils;

internal static class NotifySend
{
    private const string EXECUTABLE_NAME = "notify-send";

    public static bool IsSupported => Find(EXECUTABLE_NAME) != null;

    private static string? Find(string fileName)
    {
        if (File.Exists(fileName))
            return Path.GetFullPath(fileName);

        var paths = Environment.GetEnvironmentVariable("PATH");
        return paths?.Split(Path.PathSeparator).Select(path => Path.Combine(path, fileName))
                    .FirstOrDefault(File.Exists);
    }

    public static void Send(string summary, string body)
    {
        if (!IsSupported) throw new NotSupportedException();

        var fileName = Find(EXECUTABLE_NAME);
        if (fileName == null)
            return;

        var processStartInfo = new ProcessStartInfo(fileName)
        {
            ArgumentList =
            {
                summary,
                body,
                "--app-name=NextBepLoader"
            }
        };

        Process.Start(processStartInfo);
    }
}
