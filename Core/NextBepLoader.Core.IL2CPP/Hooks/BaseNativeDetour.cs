using System;
using MonoMod.Core;
using NextBepLoader.Core.Logging;
using NextBepLoader.Core.Logging.DefaultSource;

namespace NextBepLoader.Core.IL2CPP.Hooks;

internal abstract class BaseNativeDetour<T>(IntPtr originalMethodPtr, IntPtr detourMethod) : ICoreNativeDetour
    where T : BaseNativeDetour<T>
{
    private static readonly ManualLogSource Logger = Core.Logging.Logger.CreateLogSource(typeof(T).Name);

    public bool IsPrepared { get; protected set; }

    public IntPtr Source { get; } = originalMethodPtr;
    public IntPtr Target { get; } = detourMethod;

    public bool HasOrigEntrypoint { get; protected set; }
    public IntPtr OrigEntrypoint { get; protected set; }
    public bool IsApplied { get; private set; }

    public void Dispose()
    {
        if (!IsApplied) return;
        Undo();
        Free();
    }

    public void Apply()
    {
        if (IsApplied) return;

        Prepare();
        ApplyImpl();

        Logger.Log(LogLevel.Debug,
                    $"Original: {Source:X}, Trampoline: {OrigEntrypoint:X}, diff: {Math.Abs(Source - OrigEntrypoint):X}");

        IsApplied = true;
    }

    public void Undo()
    {
        if (IsApplied && IsPrepared) UndoImpl();
    }

    public void Free() => FreeImpl();

    protected abstract void ApplyImpl();

    private void Prepare()
    {
        if (IsPrepared) return;
        Logger.LogDebug($"Preparing detour from 0x{Source:X2} to 0x{Target:X2}");
        PrepareImpl();
        Logger.LogDebug($"Prepared detour; Trampoline: 0x{OrigEntrypoint:X2}");
        IsPrepared = true;
    }

    protected abstract void PrepareImpl();

    protected abstract void UndoImpl();

    protected abstract void FreeImpl();
}
