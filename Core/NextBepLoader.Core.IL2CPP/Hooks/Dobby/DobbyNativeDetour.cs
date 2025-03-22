using System;
using MonoMod.Core;

namespace NextBepLoader.Core.IL2CPP.Hooks.Dobby;

internal class DobbyNativeDetour(IntPtr originalMethodPtr, IntPtr detourMethod) : ICoreNativeDetour
{
    public bool IsPrepared { get; protected set; }
    public IntPtr Source { get; } = originalMethodPtr;
    public IntPtr Target { get; } = detourMethod;
    
    public bool HasOrigEntrypoint { get; private set; }
    public IntPtr OrigEntrypoint { get; private set; }
    public bool IsApplied { get; private set; }
    
    public void Dispose()
    {
        if (!IsApplied) return;
        Undo();
    }
    
    private unsafe void Prepare()
    {
        if (IsPrepared) return;
        Logger.LogDebug($"Preparing detour from 0x{Source:X2} to 0x{Target:X2}");
        
        nint trampolinePtr = 0;
        _ = DobbyLib.Prepare(Source, Target, &trampolinePtr);
        OrigEntrypoint = trampolinePtr;
        HasOrigEntrypoint = true;
        
        Logger.LogDebug($"Prepared detour; Trampoline: 0x{OrigEntrypoint:X2}");
        IsPrepared = true;
    }
    
    public void Apply()
    {
        if (IsApplied) return;

        Prepare();
        _ = DobbyLib.Commit(Source);

        Logger.LogDebug($"Original: {Source:X}, Trampoline: {OrigEntrypoint:X}, diff: {Math.Abs(Source - OrigEntrypoint):X}");

        IsApplied = true;
    }
    
    public void Undo()
    {
        if (!IsApplied || !IsPrepared) return;
        _ = DobbyLib.Destroy(Source);
    }
}
