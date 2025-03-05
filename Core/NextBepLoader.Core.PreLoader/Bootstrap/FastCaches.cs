using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using NextBepLoader.Core.Logging;
using NextBepLoader.Core.Utils;

namespace NextBepLoader.Core.PreLoader.Bootstrap;

public class FastCaches<T>(string cacheName) where T : ICacheableData, new()
{
    public readonly List<CacheInfo<T>> CacheInfos = [];
    public string CacheName { get; } = cacheName;
    public string CachePath { get; } = Path.Combine(Paths.CachePath, $"{cacheName}.cache");


    public bool TryGet(string entryIdentifier, [MaybeNullWhen(false)] out List<T> items)
    {
        if (CacheInfos.TryGet(n => n.EntryIdentifier == entryIdentifier, out var item))
        {
            items = item.FileItem;
            return true;
        }

        items = null;
        return false;
    }

    public void OnReadAssembly(string entryIdentifier, Stream dllStream) =>
        CacheInfos.GetOrCreate(n => n.EntryIdentifier == entryIdentifier,
                               () => new CacheInfo<T>(entryIdentifier, Utility.HashStream(dllStream)));

    public void OnAddAssembly(string entryIdentifier, List<T> items) =>
        CacheInfos.GetAndSet(n => n.EntryIdentifier == entryIdentifier, n => n.FileItem = items);

    public void Read()
    {
        if (!File.Exists(CachePath))
            return;

        try
        {
            CacheInfos.Clear();
            using var readStream = File.OpenRead(CachePath);
            using var reader = new BinaryReader(readStream);
            var entriesCount = reader.ReadInt32();
            for (var i = 0; i < entriesCount; i++)
            {
                var entryIdentifier = reader.ReadString();
                var hash = reader.ReadString();
                var itemsCount = reader.ReadInt32();
                var items = new List<T>();

                for (var j = 0; j < itemsCount; j++)
                {
                    var entry = new T();
                    entry.Load(reader);
                    items.Add(entry);
                }

                CacheInfos.GetOrCreate(
                                       n => n.EntryIdentifier == entryIdentifier,
                                       () => new CacheInfo<T>(entryIdentifier, hash)
                                       {
                                           FileItem = items
                                       }
                                      );
            }
        }
        catch (Exception e)
        {
            Logger.LogWarning($"Failed to load cache \"{CacheName}\"; skipping loading cache. Reason: {e.Message}");
        }
    }


    public void Write()
    {
        if (!File.Exists(CachePath))
            return;

        try
        {
            using var stream = File.OpenWrite(CachePath);
            using var writer = new BinaryWriter(stream);
            writer.Write(CacheInfos.Count);

            foreach (var info in CacheInfos.Where(info => info.FileItem.Count != 0))
            {
                writer.Write(info.EntryIdentifier);
                writer.Write(info.FileHash);
                writer.Write(info.FileItem.Count);

                foreach (var item in info.FileItem)
                    item.Save(writer);
            }
        }
        catch (Exception e)
        {
            Logger.LogWarning($"Failed to save cache \"{CacheName}\"; skipping saving cache. Reason: {e.Message}.");
        }
    }
}

public record CacheInfo<T>(string EntryIdentifier, string FileHash) where T : ICacheableData
{
    public List<T> FileItem { get; set; } = [];
}
