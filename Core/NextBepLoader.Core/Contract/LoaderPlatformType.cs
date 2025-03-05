using System;

namespace NextBepLoader.Core.Contract;

[Flags]
public enum LoaderPlatformType
{
    Core = 0,
    PreLoader = 1,
    IL2CPP = 2,
    Desktop = Core | PreLoader | IL2CPP | 3,
    Android = Core | PreLoader | IL2CPP | 4,
    Unknown = 5
}
