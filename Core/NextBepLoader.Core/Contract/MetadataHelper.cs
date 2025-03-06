using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AsmResolver.DotNet;
using NextBepLoader.Core.Contract.Attributes;

namespace NextBepLoader.Core.Contract;

public static class MetadataHelper
{
    internal static IEnumerable<CustomAttribute> GetCustomAttributes<T>(TypeDefinition td, bool inherit)
        where T : Attribute
    {
        var result = new List<CustomAttribute>();
        var type = typeof(T);
        var currentType = td;

        do
        {
            result.AddRange(currentType?.CustomAttributes.Where(ca => ca.Constructor!.FullName == type.FullName)!);
            currentType = currentType?.BaseType?.Resolve();
        } while (inherit && currentType?.FullName != "System.Object");


        return result;
    }


    /// <summary>
    ///     Gets the specified attributes of a type, if they exist.
    /// </summary>
    /// <typeparam name="T">The attribute type to retrieve.</typeparam>
    /// <param name="pluginType">The plugin type.</param>
    /// <returns>The attributes of the type, if existing.</returns>
    public static T[] GetAttributes<T>(Type pluginType) where T : Attribute =>
        (T[])pluginType.GetCustomAttributes(typeof(T), true);

    /// <summary>
    ///     Gets the specified attributes of an assembly, if they exist.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <typeparam name="T">The attribute type to retrieve.</typeparam>
    /// <returns>The attributes of the type, if existing.</returns>
    public static T[] GetAttributes<T>(Assembly assembly) where T : Attribute =>
        (T[])assembly.GetCustomAttributes(typeof(T), true);

    /// <summary>
    ///     Gets the specified attributes of an instance, if they exist.
    /// </summary>
    /// <typeparam name="T">The attribute type to retrieve.</typeparam>
    /// <param name="plugin">The plugin instance.</param>
    /// <returns>The attributes of the instance, if existing.</returns>
    public static IEnumerable<T> GetAttributes<T>(object plugin) where T : Attribute =>
        GetAttributes<T>(plugin.GetType());

    /// <summary>
    ///     Gets the specified attributes of a reflection metadata type, if they exist.
    /// </summary>
    /// <typeparam name="T">The attribute type to retrieve.</typeparam>
    /// <param name="member">The reflection metadata instance.</param>
    /// <returns>The attributes of the instance, if existing.</returns>
    public static T[] GetAttributes<T>(MemberInfo member) where T : Attribute =>
        (T[])member.GetCustomAttributes(typeof(T), true);

    /// <summary>
    ///     Retrieves the dependencies of the specified plugin type.
    /// </summary>
    /// <param name="plugin">The plugin type.</param>
    /// <returns>A list of all plugin types that the specified plugin type depends upon.</returns>
    public static IEnumerable<PluginDependency> GetDependencies(this Type plugin) =>
        plugin.GetCustomAttributes(typeof(PluginDependency), true).Cast<PluginDependency>();


    public static IEnumerable<PluginCompatibility> CompatibilityFromAsmType(this TypeDefinition td)
    {
        var attrs = GetCustomAttributes<PluginCompatibility>(td, true);
        return attrs.Select(customAttribute =>
        {
            var dependencyGuid = (string)customAttribute.Signature!.NamedArguments[0].Argument.Element!;
            return new PluginCompatibility(dependencyGuid);
        }).ToList();
    }

    public static PluginMetadata? GetMetadataFromAsmType(this TypeDefinition td)
    {
        var attr = GetCustomAttributes<PluginMetadata>(td, false).FirstOrDefault();
        if (attr == null) return null;

        var type =
            Enum.Parse<LoaderPlatformType>(attr.Signature!.NamedArguments[0].Argument.Element!.ToString());
        var id = attr.Signature!.NamedArguments[1].Argument.Element!.ToString();
        var metadata = new PluginMetadata(type, id);

        if (attr.Signature.NamedArguments.Count <= 2) return metadata;
        metadata.Name = attr.Signature.NamedArguments[2].Argument.Element!.ToString();
        metadata.Version = new Version(attr.Signature.NamedArguments[3].Argument.Element!.ToString());

        return metadata;
    }
}
