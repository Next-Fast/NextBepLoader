using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;

namespace NextBepLoader.Core.Contract.Attributes;

/// <summary>
///     This attribute specifies other plugins that are incompatible with this plugin.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class PluginCompatibility : Attribute /*, ICacheable*/
{
    /// <summary>
    ///     Marks this <see cref="BaseUnityPlugin" /> as incompatible with another plugin.
    ///     If the other plugin exists, this plugin will not be loaded and a warning will be shown.
    /// </summary>
    /// <param name="IncompatibilityGUID">The GUID of the referenced plugin.</param>
    public PluginCompatibility(string IncompatibilityGUID)
    {
        this.IncompatibilityGUID = IncompatibilityGUID;
    }

    /// <summary>
    ///     The GUID of the referenced plugin.
    /// </summary>
    public string IncompatibilityGUID { get; protected set; }

    /*void ICacheable.Save(BinaryWriter bw) => bw.Write(IncompatibilityGUID);

    void ICacheable.Load(BinaryReader br) => IncompatibilityGUID = br.ReadString();*/

    internal static IEnumerable<PluginCompatibility> FromCecilType(TypeDefinition td)
    {
        var attrs = MetadataHelper.GetCustomAttributes<PluginCompatibility>(td, true);
        return attrs.Select(customAttribute =>
        {
            var dependencyGuid = (string)customAttribute.Signature!.NamedArguments[0].Argument.Element!;
            return new PluginCompatibility(dependencyGuid);
        }).ToList();
    }
}
