using System.IO;
using System.Text;
using AssetRipper.Primitives;

namespace NextBepLoader.Core.IL2CPP.Utils;

public class ManagerLookup(string fileName, params int[] lookupOffsets)
{
    
    public string FileRootPath { get; set; }
    public string FileName { get; } = fileName;
    public string FilePath => Path.Combine(FileRootPath, FileName);
    public int[] LookupOffsets { get; } = lookupOffsets;

    public UnityVersion? LookupVersion { get; private set; }
    
    public string? Engine { get; private set; }

    public bool Looked = false;

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

    public static explicit operator UnityVersion(ManagerLookup lookup) => lookup.LookupVersion ?? UnityVersion.MinVersion;
}
