using System.Runtime.InteropServices;
using MonoMod.Utils;
using NextBepLoader.Core.Utils;

namespace NextBepLoader.Deskstop.Utils;

#nullable disable
internal static class PlatformUtils
{
    public static Version WindowsVersion { get; set; }
    public static string WineVersion { get; set; }
    public static string LinuxKernelVersion { get; set; }

    [DllImport("libc.so.6", EntryPoint = "uname", CallingConvention = CallingConvention.Cdecl,
               CharSet = CharSet.Ansi)]
    private static extern IntPtr uname_linux(ref utsname_linux utsname);

    [DllImport("ntdll.dll", SetLastError = true)]
    private static extern bool RtlGetVersion(ref WindowsOSVersionInfoExW versionInfo);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibrary(string libraryName);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    /// <summary>
    ///     Recreation of MonoMod's PlatformHelper.DeterminePlatform method, but with libc calls instead of creating processes.
    /// </summary>
    public static void SetDesktopPlatformVersion()
    {
        if (PlatformDetection.OS.Is(OSKind.Windows))
        {
            var windowsVersionInfo = new WindowsOSVersionInfoExW();
            RtlGetVersion(ref windowsVersionInfo);

            WindowsVersion = new Version((int)windowsVersionInfo.dwMajorVersion,
                                         (int)windowsVersionInfo.dwMinorVersion, 0,
                                         (int)windowsVersionInfo.dwBuildNumber);

            var ntDll = LoadLibrary("ntdll.dll");
            if (ntDll != IntPtr.Zero)
            {
                var wineGetVersion = GetProcAddress(ntDll, "wine_get_version");
                if (wineGetVersion != IntPtr.Zero)
                {
                    var getVersion = wineGetVersion.AsDelegate<GetWineVersionDelegate>()!;
                    WineVersion = getVersion();
                }
            }
        }

        if (!PlatformDetection.OS.Is(OSKind.Linux)) return;
        var utsname_linux = new utsname_linux();
        var result = uname_linux(ref utsname_linux);
        if (result != IntPtr.Zero) LinuxKernelVersion = utsname_linux.version;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.LPStr)]
    private delegate string GetWineVersionDelegate();

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct WindowsOSVersionInfoExW()
    {
        public uint dwOSVersionInfoSize = (uint)Marshal.SizeOf(typeof(WindowsOSVersionInfoExW));
        public uint dwMajorVersion = 0;
        public uint dwMinorVersion = 0;
        public uint dwBuildNumber = 0;
        public uint dwPlatformId = 0;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szCSDVersion = null;

        public ushort wServicePackMajor = 0;
        public ushort wServicePackMinor = 0;
        public ushort wSuiteMask = 0;
        public byte wProductType = 0;
        public byte wReserved = 0;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct utsname_linux
    {
        private const int linux_utslen = 65;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = linux_utslen)]
        public string sysname;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = linux_utslen)]
        public string nodename;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = linux_utslen)]
        public string release;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = linux_utslen)]
        public string version;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = linux_utslen)]
        public string machine;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = linux_utslen)]
        public string domainname;
    }
}
