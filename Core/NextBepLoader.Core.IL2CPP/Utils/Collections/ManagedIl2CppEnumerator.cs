using System;
using HarmonyLib;
using Il2CppSystem.Collections;

namespace NextBepLoader.Core.IL2CPP.Utils.Collections;

#nullable disable
public class ManagedIl2CppEnumerator(IEnumerator enumerator) : System.Collections.IEnumerator
{
    private static readonly Func<IEnumerator, bool> moveNext = AccessTools
                                                               .Method(typeof(IEnumerator), "MoveNext")
                                                               ?.CreateDelegate<Func<IEnumerator, bool>>();

    private static readonly Action<IEnumerator> reset = AccessTools
                                                        .Method(typeof(IEnumerator), "Reset")
                                                        ?.CreateDelegate<Action<IEnumerator>>();

    private readonly IEnumerator enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
    public bool MoveNext() => moveNext?.Invoke(enumerator) ?? false;
    public void Reset() => reset?.Invoke(enumerator);
    public object Current => enumerator.Current;
}
