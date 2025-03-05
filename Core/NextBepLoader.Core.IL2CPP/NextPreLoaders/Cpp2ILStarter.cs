using System;
using System.Collections.Generic;
using System.IO;
using AsmResolver.DotNet;
using Cpp2IL.Core;
using Cpp2IL.Core.Api;
using Cpp2IL.Core.InstructionSets;
using Cpp2IL.Core.OutputFormats;
using Cpp2IL.Core.ProcessingLayers;
using LibCpp2IL;
using Microsoft.Extensions.Logging;
using NextBepLoader.Core.IL2CPP.Utils;
using NextBepLoader.Core.LoaderInterface;
using NextBepLoader.Core.PreLoader;
using NextBepLoader.Core.Utils;

namespace NextBepLoader.Core.IL2CPP.NextPreLoaders;

public class Cpp2ILStarter(INextBepEnv env, ILogger<Cpp2ILStarter> logger, UnityInfo unityInfo) : BasePreLoader
{
    private IL2CPPCheckEventArg il2CPPCheckEventArg;
    public override Type[] WaitLoadLoader => [typeof(HashComputer)];

    public override void PreLoad(PreLoadEventArg arg)
    {
        il2CPPCheckEventArg = env.GetOrCreateEventArgs<IL2CPPCheckEventArg>();
        InstructionSetRegistry.RegisterInstructionSet<X86InstructionSet>(DefaultInstructionSets.X86_32);
        InstructionSetRegistry.RegisterInstructionSet<X86InstructionSet>(DefaultInstructionSets.X86_64);
        LibCpp2IlBinaryRegistry.RegisterBuiltInBinarySupport();
    }

    public override void Start()
    {
        if (il2CPPCheckEventArg is { UpdateIL2CPPInteropAssembly: false }) return;
        logger.LogInformation("Running Cpp2IL to generate dummy assemblies");
        CPP2ILUtils.SetLogger(null, logger);

        List<AssemblyDefinition> result = [];
        var runTime = CoreUtils.StartStopwatch(() =>
        {
            Cpp2IlApi.InitializeLibCpp2Il(Paths.GameAssemblyPath, Paths.GameMetaDataPath, unityInfo);

            List<Cpp2IlProcessingLayer> processingLayers = [new AttributeInjectorProcessingLayer()];

            foreach (var cpp2IlProcessingLayer in processingLayers)
                cpp2IlProcessingLayer.PreProcess(Cpp2IlApi.CurrentAppContext, processingLayers);

            foreach (var cpp2IlProcessingLayer in processingLayers)
                cpp2IlProcessingLayer.Process(Cpp2IlApi.CurrentAppContext);

            var outputFormat = new AsmResolverDllOutputFormatDefault();
            result = outputFormat.BuildAssemblies(Cpp2IlApi.CurrentAppContext);

            LibCpp2IlMain.Reset();
            Cpp2IlApi.CurrentAppContext = null;
        });
        logger.LogInformation("Cpp2IL finished in {time}", runTime);

        if (il2CPPCheckEventArg.CacheCPP2ILAssembly)
        {
            var id = 0;
            var time = CoreUtils.StartStopwatch(() =>
            {
                foreach (var assembly in result)
                    assembly.Write(Path.Combine(Paths.CPP2ILCacheDir, assembly.Name ?? id++ + ".dll"));
            });
            logger.LogInformation("CPP2IL Cache Write Time {time}", time);
        }

        il2CPPCheckEventArg.ResolverAssemblies = result;
    }
}
