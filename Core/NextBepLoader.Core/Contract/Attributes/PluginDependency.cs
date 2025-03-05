using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;
using Range = SemanticVersioning.Range;

namespace NextBepLoader.Core.Contract.Attributes;

/// <summary>
///     This attribute specifies any dependencies that this plugin has on other plugins.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class PluginDependency : Attribute /*, ICacheable*/
{
    /// <summary>
    ///     Flags that are applied to a dependency
    /// </summary>
    [Flags]
    public enum DependencyFlags
    {
        /// <summary>
        ///     The plugin has a hard dependency on the referenced plugin, and will not run without it.
        /// </summary>
        HardDependency = 1,

        /// <summary>
        ///     This plugin has a soft dependency on the referenced plugin, and is able to run without it.
        /// </summary>
        SoftDependency = 2
    }

    /// <summary>
    ///     Marks this <see cref="BaseUnityPlugin" /> as dependent on another plugin. The other plugin will be loaded before
    ///     this one.
    ///     If the other plugin doesn't exist, what happens depends on the <see cref="Flags" /> parameter.
    /// </summary>
    /// <param name="DependencyGUID">The GUID of the referenced plugin.</param>
    /// <param name="Flags">The flags associated with this dependency definition.</param>
    public PluginDependency(string DependencyGUID, DependencyFlags Flags = DependencyFlags.HardDependency)
    {
        this.DependencyGUID = DependencyGUID;
        this.Flags = Flags;
        VersionRange = null;
    }

    /// <summary>
    ///     Marks this <see cref="BaseUnityPlugin" /> as dependent on another plugin. The other plugin will be loaded before
    ///     this one.
    ///     If the other plugin doesn't exist or is of a version not satisfying <see cref="VersionRange" />, this plugin will
    ///     not load and an error will be logged instead.
    /// </summary>
    /// <param name="guid">The GUID of the referenced plugin.</param>
    /// <param name="version">The version range of the referenced plugin.</param>
    /// <remarks>When version is supplied the dependency is always treated as HardDependency</remarks>
    public PluginDependency(string guid, string version) : this(guid)
    {
        VersionRange = Range.Parse(version);
    }

    /// <summary>
    ///     The GUID of the referenced plugin.
    /// </summary>
    public string DependencyGUID { get; protected set; }

    /// <summary>
    ///     The flags associated with this dependency definition.
    /// </summary>
    public DependencyFlags Flags { get; protected set; }

    /// <summary>
    ///     The version <see cref="SemVer.Range">range</see> of the referenced plugin.
    /// </summary>
    public Range? VersionRange { get; protected set; }

    /*void ICacheable.Save(BinaryWriter bw)
    {
        bw.Write(DependencyGUID);
        bw.Write((int)Flags);
        bw.Write(VersionRange?.ToString() ?? string.Empty);
    }

    void ICacheable.Load(BinaryReader br)
    {
        DependencyGUID = br.ReadString();
        Flags = (DependencyFlags)br.ReadInt32();

        var versionRange = br.ReadString();
        VersionRange = versionRange == string.Empty ? null : Range.Parse(versionRange);
    }*/

    internal static IEnumerable<PluginDependency> FromCecilType(TypeDefinition td)
    {
        var attrs = MetadataHelper.GetCustomAttributes<PluginDependency>(td, true);
        return attrs.Select(customAttribute =>
        {
            var dependencyGuid = (string)customAttribute.Signature!.NamedArguments[0].Argument.Element!;
            var secondArg = customAttribute.Signature!.NamedArguments[1].Argument.Element!;
            if (secondArg is string minVersion) return new PluginDependency(dependencyGuid, minVersion);
            return new PluginDependency(dependencyGuid, (DependencyFlags)secondArg);
        }).ToList();
    }
}
