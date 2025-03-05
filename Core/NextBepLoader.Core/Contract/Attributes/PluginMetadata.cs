using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace NextBepLoader.Core.Contract.Attributes;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Class)]
public class PluginMetadata(LoaderPlatformType type, string id) : Attribute
{
    public PluginMetadata(LoaderPlatformType type, string id, string name = "", Version? version = null) :
        this(type, id)
    {
        Name = name;
        SetVersion(version);
    }

    public PluginMetadata(LoaderPlatformType type, string id, string name = "", string version = "")
        : this(type, id, name, Version.TryParse(version, out var result) ? result : null) { }

    public LoaderPlatformType Type { get; } = type;
    public string ID { get; } = id;


    public string Name { get; set; }
    public Version Version { get; set; }
    public string VersionString { get; set; }

    public List<PluginDependency> Dependencies { get; set; }

    public List<PluginCompatibility> Compatibilities { get; set; }

    public List<PluginProcess> Processes { get; set; }

    private void SetVersion(Version? version)
    {
        Version = version ?? new Version(1, 0, 0);
        VersionString = Version.ToString();
    }
}
