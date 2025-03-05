using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;

namespace NextBepLoader.Core.Contract.Attributes;

/// <summary>
///     This attribute specifies which processes this plugin should be run for. Not specifying this attribute will load the
///     plugin under every process.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class PluginProcess : Attribute
{
    /// <param name="ProcessName">The name of the process that this plugin will run under.</param>
    public PluginProcess(string ProcessName)
    {
        this.ProcessName = ProcessName;
    }

    /// <summary>
    ///     The name of the process that this plugin will run under.
    /// </summary>
    public string ProcessName { get; protected set; }

    internal static List<PluginProcess> FromCecilType(TypeDefinition td)
    {
        var attrs = MetadataHelper.GetCustomAttributes<PluginProcess>(td, true);
        return attrs.Select(customAttribute =>
                                new PluginProcess(((string)customAttribute.Signature!.NamedArguments[0].Argument
                                                                          .Element!)!)).ToList();
    }
}
