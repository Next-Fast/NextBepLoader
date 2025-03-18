using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NextBepLoader.Core.Logging;

namespace NextBepLoader.Core.PreLoader.NextPreLoaders;

public static class NextPreLoaderExtension
{
    internal static readonly List<BasePreLoader> Cache = [];

    public static IEnumerable<BasePreLoader> GetPreLoaders(params Assembly[] allAssembly) =>
        allAssembly.SelectMany(assembly =>
                                   assembly.GetPreLoadersFormAssembly(type => type.Namespace != null &&
                                                                              type.Namespace
                                                                                  .EndsWith("NextPreLoaders")));

    public static List<BasePreLoader> GetPreLoadersFormDef() =>
        typeof(NextPreLoaderExtension).Assembly.GetPreLoadersFormAssembly(type => type.Namespace ==
                                                                              typeof(NextPreLoaderExtension).Namespace);

    public static List<BasePreLoader> GetPreLoadersFormAssembly(this Assembly assembly,
                                                                Predicate<Type>? predicate = null)
    {
        var findList = new List<BasePreLoader>();

        foreach (var findType in assembly.GetTypes().Where(IsTarget))
        {
            if (TryGetCachePreLoader(findType, out var preLoader))
            {
                findList.Add(preLoader);
                continue;
            }

            var instance = (BasePreLoader)Activator.CreateInstance(findType)!;
            Cache.Add(instance);
            findList.Add(instance);
        }

        return findList;

        bool IsTarget(Type type)
        {
            if (predicate != null) return type.BaseType == typeof(BasePreLoader) && predicate(type);

            return type.BaseType == typeof(BasePreLoader);
        }
    }

    public static T? GetCachePreLoader<T>() where T : BasePreLoader => Cache.FirstOrDefault(n => n is T) as T;

    public static BasePreLoader? GetCachePreLoader(Type type) => Cache.FirstOrDefault(n => n.GetType() == type);

    public static bool TryGetCachePreLoader(Type type, [MaybeNullWhen(false)] out BasePreLoader preLoader)
    {
        var loader = GetCachePreLoader(type);
        if (loader == null)
        {
            preLoader = null;
            return false;
        }

        preLoader = loader;
        return true;
    }

    public static IServiceCollection AddPreLoader<T>(this IServiceCollection collection) where T : BasePreLoader
    {
        return collection.AddSingleton<BasePreLoader, T>();
    }

    public static void SortLoaders(this List<BasePreLoader> preLoaders) =>
        preLoaders.Sort((x, y) =>
        {
            if (x.WaitLoadLoader.Contains(y.GetType()))
                return x.Priority > y.Priority ? 2 : 1;

            if (!y.WaitLoadLoader.Contains(x.GetType()))
                return 0;

            if (x.Priority < y.Priority)
                return -2;

            return -1;
        });

    public static IServiceCollection AddStartRunner(this IServiceCollection collection)
    {
        collection.AddSingleton<OnStartRunner>();
        return collection;
    }

    public static void StartRunner(this IServiceProvider provider)
    {
        var startRunner = provider.GetService<OnStartRunner>();
        if (startRunner == null)
        {
            Logger.LogError("OnStartRunner is null");
            return;
        }

        startRunner.Run(provider);
    }
}
