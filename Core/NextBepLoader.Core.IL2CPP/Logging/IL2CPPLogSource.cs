using System;
using System.Runtime.InteropServices;
using NextBepLoader.Core.Logging;
using NextBepLoader.Core.Logging.Interface;
using NextBepLoader.Core.PreLoader;

namespace NextBepLoader.Core.IL2CPP.Logging;

internal class IL2CPPLogSource : ILogSource
{
    public IL2CPPLogSource()
    {
        var loggerPointer = Marshal.GetFunctionPointerForDelegate(new IL2CPPLogCallbackDelegate(IL2CPPLogCallback));
        Il2CppInterop.Runtime.IL2CPP.il2cpp_register_log_callback(loggerPointer);
    }

    public string SourceName => "IL2CPP";
    public event EventHandler<LogEventArgs> LogEvent;

    public void Dispose() { }

    private void IL2CPPLogCallback(string message) =>
        LogEvent(this, new LogEventArgs(message.Trim(), LogLevel.Message, this));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void IL2CPPLogCallbackDelegate([In] [MarshalAs(UnmanagedType.LPStr)] string message);
}
