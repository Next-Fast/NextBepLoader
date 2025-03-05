<p align="center">
    <img src="https://avatars2.githubusercontent.com/u/39589027?s=256">
</p>

# NextBeLoader

基于BepInEx进行修改分支的模组加载器  
使用NET9.0  
将IL2CPPInterop和HarmonyX和MonoMod更新到最新版本  
请将BepInEx.Msbuild.IL2CPP替换为NextBepLoader.BepInEx.Msbuild.IL2CPP以适配IL2CPPInterop更新    
仅当使用游戏Nuget包来代替引用Interop文件夹时需要使用Msbuild.IL2CPP

MonoMod 和 HarmonyX 更新来自 https://github.com/BepInEx/BepInEx/pull/946  
Android 相关来自 https://github.com/LemonLoader/MelonLoader

#### 框架兼容性

|              | Windows | OSX | Linux | ARM |
|--------------|---------|-----|-------|-----|
| Unity IL2CPP | WIP     | ?   | ?     | WIP |
| Unity Mono   | WIP     | ?   | ?     | X   |

## 使用库

- [NeighTools/UnityDoorstop](https://github.com/NeighTools/UnityDoorstop) - v4.3.0
- [BepInEx/HarmonyX](https://github.com/BepInEx/HarmonyX) - Latest
- [0x0ade/MonoMod](https://github.com/0x0ade/MonoMod) - Latest
- [jbevain/cecil](https://github.com/jbevain/cecil) - ?
- [SamboyCoding/Cpp2IL](https://github.com/SamboyCoding/Cpp2IL) - Latest
- [BepInEx/Il2CppInterop](https://github.com/BepInEx/Il2CppInterop) - Latest
- Dotnet-Runtime - NET9
- Dobby

## 许可证

The BepInEx project is licensed under the LGPL-2.1 license.
