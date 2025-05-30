using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.DotNet;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using MonoMod.Utils;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.Logging;

namespace NextBepLoader.Core.Utils;

public static class CoreUtils
{
    public static readonly bool IsMono = PlatformDetection.Runtime == RuntimeKind.Mono;

    public static readonly bool IsCore = PlatformDetection.Runtime == RuntimeKind.CoreCLR;
    public static string TimeStamp => DateTime.Now
                                              .ToString("G")
                                              .Replace("/", "_")
                                              .Replace(" ", "_")
                                              .Replace(":", "_");

    public static string PlatformPostFix => PlatformPostFixGet();
    public static string PlatformGameAssemblyName => PlatformGameAssemblyNameGet();

    public static T? GetExportAsDelegate<T>(this IntPtr s, string name) where T : class =>
        s.GetExport(name).AsDelegate<T>();

    public static T? AsDelegate<T>(this IntPtr s) where T : class =>
        Marshal.GetDelegateForFunctionPointer(s, typeof(T)) as T;

    private static string PlatformGameAssemblyNameGet()
    {
        if (PlatformDetection.OS.Is(OSKind.Android))
            return "libil2cpp";

        return "GameAssembly";
    }

    private static string PlatformPostFixGet()
    {
        if (PlatformDetection.OS.Is(OSKind.Windows))
            return "dll";
        
        if (PlatformDetection.OS.Is(OSKind.Android))
            return "so";

        if (PlatformDetection.OS.Is(OSKind.OSX))
            return "dylib";

        if (PlatformDetection.OS.Is(OSKind.Posix))
            return "sp";

        return "dll";
    }

    public static TimeSpan StartStopwatch(Action action)
    {
        var watch = new Stopwatch();
        watch.Start();
        action.Invoke();
        watch.Stop();
        return watch.Elapsed;
    }

    public static void DeleteAllFiles(string dir)
    {
        if (!Directory.Exists(dir))
            return;

        Directory
            .GetFiles(dir)
            .Do(File.Delete);
    }

    public static void HashString(this ICryptoTransform hash, string str)
    {
        var buffer = Encoding.UTF8.GetBytes(str);
        hash.TransformBlock(buffer, 0, buffer.Length, buffer, 0);
    }

    public static void HashFile(this ICryptoTransform hash, string file)
    {
        const int defaultCopyBufferSize = 81920;
        using var fs = File.OpenRead(file);
        var buffer = new byte[defaultCopyBufferSize];
        int read;
        while ((read = fs.Read(buffer)) > 0)
            hash.TransformBlock(buffer, 0, read, buffer, 0);
    }

    public static T GetOrCreate<T>(this List<T> list, Func<T, bool> predicate, Func<T> create) where T : class
    {
        var item = list.FirstOrDefault(predicate);
        if (item is not null) return item;
        item = create();
        list.Add(item);

        return item;
    }

    public static bool GetAndSet<T>(this List<T> list, Func<T, bool> predicate, Action<T> set)
    {
        var item = list.FirstOrDefault(predicate);
        if (item is null) return false;
        set(item);
        return true;
    }

    public static bool TryGet<T>(this List<T> list, Func<T, bool> predicate, [MaybeNullWhen(false)] out T item)
        where T : class
    {
        item = list.FirstOrDefault(predicate);
        return item is not null;
    }

    public static bool HasBase<T>(this Type type) => type.HasBase(typeof(T));

    public static bool HasBase(this Type type, Type baseType)
    {
        if (type.BaseType == null) return false;

        return
            type.BaseType == baseType
          ||
            type.BaseType.HasBase(baseType);
    }

    public static bool HasBase<T>(this TypeDefinition type) => type.HasBase(typeof(T));

    public static bool HasBase(this TypeDefinition? type, Type baseType)
    {
        if (type?.BaseType == null) return false;

        return
            type.BaseType.FullName == baseType.FullName
          ||
            type.BaseType.Resolve().HasBase(baseType);
    }

    public static IServiceProvider TryRunOnStart(this IServiceProvider provider)
    {

        try
        {
            var allStart = provider.GetServices<IOnLoadStart>().ToList();
            allStart.Sort((x, y) => x.Priority.CompareTo(y.Priority));
            foreach (var start in allStart)
            {
                start.OnLoadStart();
                Logger.LogInfo($"On LoadStart:{start.GetType().Name}");
            }
        }
        catch (Exception e)
        {
            Logger.LogWarning(e);
        }

        return provider;
    }

    public static IServiceCollection SingleService<TInterface, TClass>(this IServiceCollection collection)
        where TClass : class, TInterface where TInterface : class
    {
        return collection
            .AddSingleton<TClass>()
            .AddSingleton<TInterface, TClass>(provider => provider.GetRequiredService<TClass>());
    }
    
    public static IServiceCollection SingleOnStartService<TClass>(this IServiceCollection collection) where TClass : class, IOnLoadStart
    {
        return collection.AddSingleton<TClass>()
                         .AddSingleton<IOnLoadStart>(provider => provider.GetRequiredService<TClass>());
    }

    public static IServiceCollection SingleOnStartService<TInterface, TClass>(this IServiceCollection collection) where TClass : class, IOnLoadStart, TInterface where TInterface : class
    {
        return collection.SingleService<TInterface, TClass>()
                         .AddSingleton<IOnLoadStart>(provider => provider.GetRequiredService<TClass>());
    }
}
