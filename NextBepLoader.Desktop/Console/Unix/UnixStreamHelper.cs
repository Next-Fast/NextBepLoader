using MonoMod.Utils;
using NextBepLoader.Core.Utils;

namespace NextBepLoader.Deskstop.Console.Unix;

internal static class UnixStreamHelper
{
    public delegate int dupDelegate(int fd);

    public delegate int fcloseDelegate(IntPtr stream);

    public delegate IntPtr fdopenDelegate(int fd, string mode);

    public delegate int fflushDelegate(IntPtr stream);

    public delegate IntPtr freadDelegate(IntPtr ptr, IntPtr size, IntPtr nmemb, IntPtr stream);

    public delegate int fwriteDelegate(IntPtr ptr, IntPtr size, IntPtr nmemb, IntPtr stream);

    public delegate int isattyDelegate(int fd);

    private static readonly IntPtr libcHandle;

    public static dupDelegate dup;

    public static fdopenDelegate fdopen;

    public static freadDelegate fread;

    public static fwriteDelegate fwrite;

    public static fcloseDelegate fclose;

    public static fflushDelegate fflush;

    public static isattyDelegate isatty;

    static UnixStreamHelper()
    {
        libcHandle = DynDll.OpenLibrary(PlatformDetection.OS.Is(OSKind.OSX) ? "/usr/lib/libSystem.dylib" : "libc");
        dup = libcHandle.GetExportAsDelegate<dupDelegate>("dup")!;
        fdopen = libcHandle.GetExportAsDelegate<fdopenDelegate>("fdopen")!;
        fread = libcHandle.GetExportAsDelegate<freadDelegate>("fread")!;
        fwrite = libcHandle.GetExportAsDelegate<fwriteDelegate>("fwrite")!;
        fclose = libcHandle.GetExportAsDelegate<fcloseDelegate>("fclose")!;
        fflush = libcHandle.GetExportAsDelegate<fflushDelegate>("fflush")!;
        isatty = libcHandle.GetExportAsDelegate<isattyDelegate>("isatty")!;
    }

    public static Stream CreateDuplicateStream(int fileDescriptor)
    {
        var newFd = dup(fileDescriptor);

        return new UnixStream(newFd, FileAccess.Write);
    }
}
