using System.ComponentModel;
using System.Runtime.InteropServices;
using MonoMod.Utils;

namespace NextBepLoader.Deskstop.Utils;

internal static class MessageBox
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int MessageBoxA(IntPtr hWnd, string lpText, string lpCaption, uint uType);

    public static void Show(string text, string caption)
    {
        if (!PlatformDetection.OS.Is(OSKind.Windows))
            throw new PlatformNotSupportedException();
        if (MessageBoxA(IntPtr.Zero, text, caption, 0) == 0)
            throw new Win32Exception();
    }
}
