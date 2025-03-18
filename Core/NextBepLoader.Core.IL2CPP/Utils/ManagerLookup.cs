using System.IO;
using System.Text;
using AssetRipper.Primitives;

namespace NextBepLoader.Core.IL2CPP.Utils;

internal class ManagerLookup(string fileName, params int[] lookupOffsets)
{
    // ReSharper disable once NotAccessedField.Global
    public bool Looked;

    private string FileRootPath { get; set; }
    private string FileName { get; } = fileName;
    private string FilePath => Path.Combine(FileRootPath, FileName);
    private int[] LookupOffsets { get; } = lookupOffsets;

    private UnityVersion? LookupVersion { get; set; }

    public string? Engine { get; private set; }

    public ManagerLookup SetFileRootPath(string path)
    {
        FileRootPath = path;
        return this;
    }

    public bool TryLookup()
    {
        if (!File.Exists(FilePath))
            return false;

        using var fs = File.OpenRead(FilePath);
        foreach (var offset in LookupOffsets)
        {
            var sb = new StringBuilder();
            fs.Position = offset;

            byte b;
            while ((b = (byte)fs.ReadByte()) != 0)
                sb.Append((char)b);

            if (!UnityVersion.TryParse(sb.ToString(), out var lookupVersion, out var engine)) continue;

            LookupVersion = lookupVersion;
            Engine = engine;
            Looked = true;
            return true;
        }

        return false;
    }

    public static explicit operator UnityVersion(ManagerLookup lookup) =>
        lookup.LookupVersion ?? UnityVersion.MinVersion;
}
