using System;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using UnityEngine;

namespace NextBepLoader.Core.IL2CPP.Utils;

public static class Il2CppUtils
{
    // TODO: Check if we can safely initialize this in Chainloader instead
    private static GameObject managerGo;

    public static Il2CppObjectBase AddComponent(Type t)
    {
        if (managerGo == null)
            managerGo = new GameObject { hideFlags = HideFlags.HideAndDontSave, name = "NextBepLoader" };

        if (!ClassInjector.IsTypeRegisteredInIl2Cpp(t))
            ClassInjector.RegisterTypeInIl2Cpp(t);

        return managerGo.AddComponent(Il2CppType.From(t));
    }

    /// <summary>
    ///     Register and add a Unity Component (for example MonoBehaviour) into BepInEx global manager.
    ///     Automatically registers the type with Il2Cpp type system if it isn't initialised already.
    /// </summary>
    /// <typeparam name="T">Type of the component to add.</typeparam>
    public static T AddComponent<T>() where T : Il2CppObjectBase => AddComponent(typeof(T)).Cast<T>();
}
