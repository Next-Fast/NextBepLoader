using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AOTProxy;

internal static class Export
{
    [UnmanagedCallersOnly(EntryPoint = "DllMain")]
    [RequiresAssemblyFiles("Calls System.Reflection.Assembly.Location")]
    public static unsafe bool DllMain(nint hModule, uint ulReasonForCall, nint lpReserved)
    {
        var filename = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
        var path = Path.Combine(Environment.SystemDirectory, filename);
        var proxy = NativeLibrary.Load(path);
        if (proxy == IntPtr.Zero)
            return false;
        
        return true;
    }
}
