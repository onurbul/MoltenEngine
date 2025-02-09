﻿using System;
using System.Reflection;
using Molten.Graphics.Dxc;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Vulkan;
using SpirvReflector;
using DxcBuffer = Silk.NET.Direct3D.Compilers.Buffer;

namespace Molten.Graphics.Vulkan
{
    internal class SpirvCompiler : DxcCompiler
    {
        Vk _vk;
        ReflectionLogAdapter _logger;
        SpirvReflection _reflector;

        public SpirvCompiler(Vk vk, RenderService renderer, string includePath, Assembly includeAssembly, SpirvCompileTarget spirvTarget) : 
            base(renderer, includePath, includeAssembly)
        {
            _vk = vk;
            _logger = new ReflectionLogAdapter(renderer.Log);
            _reflector = new SpirvReflection(_logger);

            string cTarget = null;
            switch (spirvTarget)
            {
                default:
                case SpirvCompileTarget.Vulkan1_0:
                    cTarget = "vulkan1.0";
                    break;

                case SpirvCompileTarget.Vulkan1_1:
                    cTarget = "vulkan1.1";
                    break;

                case SpirvCompileTarget.Vulkan1_1Spirv1_4:
                    cTarget = "vulkan1.1spirv1.4";
                    break;

                case SpirvCompileTarget.Vulkan1_3:
                    cTarget = "vulkan1.3";
                    break;

                case SpirvCompileTarget.Universal1_5:
                    cTarget = "universal1.5";
                    break; ;
            }

            AddBaseArg(DxcCompilerArg.SpirV);
            AddBaseArg(DxcCompilerArg.HlslVersion, "2021");
            AddBaseArg(DxcCompilerArg.VulkanVersion, cTarget);
            AddBaseArg(DxcCompilerArg.SkipOptimizations);
            AddBaseArg(DxcCompilerArg.Debug);
        }

        protected override unsafe void* BuildShader(HlslPass parent, ShaderType type, void* byteCode, nuint numBytes)
        {
            IDxcBlob* blob = (IDxcBlob*)byteCode;
            byteCode = blob->GetBufferPointer();

            ShaderModuleCreateInfo info = new ShaderModuleCreateInfo(StructureType.ShaderModuleCreateInfo);
            info.CodeSize = numBytes;
            info.PCode = (uint*)byteCode;
            info.Flags = ShaderModuleCreateFlags.None;

            DeviceVK device = parent.Device as DeviceVK;
            ShaderModule* shader = EngineUtil.Alloc<ShaderModule>();
            Result r = _vk.CreateShaderModule(device, info, null, shader);
            if (!r.Check(device, () => $"Failed to create {type} shader module"))
                EngineUtil.Free(ref shader);

            return shader;
        }

        protected override unsafe ShaderReflection OnBuildReflection(ShaderCompilerContext context, IDxcBlob* byteCode, DxcBuffer* reflectionBuffer)
        {
            // Output to file.
            //string fn = $"{context.Source.Filename}_{context.Type}_{context.EntryPoint}.spirv";
            /*using (FileStream stream = new FileStream(fn, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    Span<byte> t = new Span<byte>(byteCode->GetBufferPointer(), (int)byteCode->GetBufferSize());
                    writer.Write(t.ToArray(), 0, (int)byteCode->GetBufferSize());
                    context.AddDebug($"Saved SPIR-V bytecode to {fn}");
                }
            }*/

            SpirvReflectionFlags spirvFlags = SpirvReflectionFlags.LogDebug | SpirvReflectionFlags.NoSafeCopy;
            SpirvReflectionResult rr = _reflector.Reflect(byteCode->GetBufferPointer(), byteCode->GetBufferSize(), spirvFlags);
            ShaderReflection result = new ShaderReflection();

            foreach(string ext in rr.Extensions)
                result.RequiredExtensions.Add(ext);

            // Populate uniform/constant buffer binding info.
            foreach(SpirvVariable v in rr.Uniforms)
                PopulateConstantBuffer(v, result);

            // Populate input/output resource parameters
            foreach (SpirvEntryPoint ep in rr.EntryPoints)
            {
                PopulateReflectionParamters(context, result, ep, ShaderIOLayoutType.Input);
                PopulateReflectionParamters(context, result, ep, ShaderIOLayoutType.Output);

                if (context.Type == ShaderType.Geometry)
                    result.GSInputPrimitive = GetGeometryTopology(ep);

                break; //  TODO support multiple entry points in reflection
            }

            return result;
        }

        protected override bool Validate(HlslPass pass, ShaderCompilerContext context, ShaderCodeResult result)
        {
            DeviceVK device = pass.Device as DeviceVK;
            for(int i = 0; i < result.Reflection.RequiredExtensions.Count; i++)
            {
                string extName = result.Reflection.RequiredExtensions[i];
                extName = "VK_" + extName.Substring(4); // Convert from SPIR-V extension name to Vulkan extension name.

                if(!device.HasExtension(extName))
                {
                    context.AddError($"Required extension '{extName}' is not enabled on the current device.");
                    return false;
                }
            }

            return true;
        }

        private GeometryHullTopology GetGeometryTopology(SpirvEntryPoint ep)
        {
            foreach(SpirvExecutionMode mode in ep.Execution.Modes)
            {
                switch (mode)
                {
                    case SpirvExecutionMode.InputLines:
                        return GeometryHullTopology.Line;

                    case SpirvExecutionMode.InputLinesAdjacency:
                        return GeometryHullTopology.LineAdjacency;

                    case SpirvExecutionMode.InputPoints:
                        return GeometryHullTopology.Point;

                    case SpirvExecutionMode.InputTrianglesAdjacency:
                        return GeometryHullTopology.TriangleAdjaccency;
                }
            }

            return GeometryHullTopology.Triangle;
        }

        private void PopulateConstantBuffer(SpirvVariable v, ShaderReflection result)
        {
            ShaderResourceInfo bindInfo = new ShaderResourceInfo()
            {
                Name = v.Name,
                BindCount = 1,
                BindPoint = v.Binding.HasValue ? v.Binding.Value : 0,
                Dimension = ShaderResourceDimension.Buffer,
                Type = ShaderInputType.CBuffer,
                NumSamples = 1,
                ResourceReturnType = ShaderReturnType.None,
                Flags = ShaderInputFlags.None, // TODO populate. This is needed for checking if a sampler is a comparison sampler.
            };

            result.BoundResources.Add(bindInfo);

            ConstantBufferInfo cBufferInfo = new ConstantBufferInfo()
            {
                Name = v.Name,
                Type = ConstantBufferType.CBuffer, // TODO Can a TBuffer be detected from SPIR-V?
                Flags = ConstantBufferFlags.UserPacked, // TODO Can we detect ForceDWord mode in SPIR-V?
                Size = v.Type.NumBytes,
            };

            result.ConstantBuffers.Add(bindInfo.Name, cBufferInfo);

            for (int i = 0; i < v.Type.Members.Count; i++)
            {
                SpirvTypeMember member = v.Type.Members[i];
                ShaderReflection.ReflectionPtr ptrDefault = null;

                // TODO fix this once struct members can have default values.
                /*if (v.DefaultValue != null)
                {
                    ptrDefault = result.NewPtr(desc.Size);
                    System.Buffer.MemoryCopy(desc.DefaultValue, ptrDefault, desc.Size, desc.Size);
                }*/

                ConstantBufferVariableInfo cVarInfo = new ConstantBufferVariableInfo()
                {
                    DefaultValue = ptrDefault,
                    Name = member.Name,
                    Size = member.Type.NumBytes,
                    StartOffset = member.ByteOffset,
                    SamplerSize = 0, // TODO Populate this
                    StartSampler = 0, // TODO Populate this
                    StartTexture = 0, // TODO Popualte this
                    TextureSize = 0, // TODO Populate this
                    Flags = ShaderVariableFlags.None, // TODO Can these flags be detected in SpirvReflector?
                };
                
                cBufferInfo.Variables.Add(cVarInfo);
                cVarInfo.Type.Name = member.Name;
                cVarInfo.Type.Offset = member.ByteOffset;
                cVarInfo.Type.Type = GetMemberVariableType(member.Type);
                PopulateTypeInfo(cVarInfo.Type, member);
            }
        }
        
        private void PopulateTypeInfo(ShaderTypeInfo typeInfo, SpirvTypeMember member)
        {
            SpirvType memberType = member.Type;

            switch (memberType.Kind)
            {
                case SpirvTypeKind.Bool:
                case SpirvTypeKind.Float:
                case SpirvTypeKind.Int:
                    typeInfo.Class = ShaderVariableClass.Scalar;
                    typeInfo.ColumnCount = 1;
                    typeInfo.RowCount = 1;
                    break;

                case SpirvTypeKind.Matrix:
                    if (member.Decorations.Has(SpirvDecoration.RowMajor))
                    {
                        typeInfo.Class = ShaderVariableClass.MatrixRows;
                        typeInfo.ColumnCount = memberType.Length;
                        typeInfo.RowCount = memberType.ElementType.Length;
                    }
                    else
                    {
                        typeInfo.Class = ShaderVariableClass.MatrixColumns;
                        typeInfo.ColumnCount = memberType.ElementType.Length;
                        typeInfo.RowCount = memberType.Length;
                    }
                    break;

                case SpirvTypeKind.Vector:
                    typeInfo.Class = ShaderVariableClass.Vector;
                    typeInfo.ColumnCount = memberType.Length;
                    typeInfo.RowCount = 1;
                    break;

                case SpirvTypeKind.Struct:
                    typeInfo.Class = ShaderVariableClass.Struct;
                    break;
            }
        }

        private ShaderVariableType GetMemberVariableType(SpirvType type)
        {
            switch (type.Kind)
            {
                default:
                case SpirvTypeKind.Bool:
                    return ShaderVariableType.Bool;

                case SpirvTypeKind.Int:
                    if(type.NumBytes == 8)
                        return ShaderVariableType.Int64;
                    else if(type.NumBytes == 4)
                        return ShaderVariableType.Int;
                    else if(type.NumBytes == 2)
                        return ShaderVariableType.Int16;
                    else
                        throw new ArgumentException($"GetVariableType() encountered an unknown Int size: {type.NumBytes}");

                case SpirvTypeKind.Float:
                    if(type.NumBytes == 8)
                        return ShaderVariableType.Double;
                    else if(type.NumBytes == 4)
                        return ShaderVariableType.Float;
                    else if(type.NumBytes == 2)
                        return ShaderVariableType.Float16;
                    else
                        throw new ArgumentException($"GetVariableType() encountered an unknown Float size: {type.NumBytes}");

                case SpirvTypeKind.UInt:
                    if (type.NumBytes == 8)
                        return ShaderVariableType.UInt64;
                    else if (type.NumBytes == 4)
                        return ShaderVariableType.UInt;
                    else if (type.NumBytes == 2)
                        return ShaderVariableType.UInt16;
                    else
                        throw new ArgumentException($"GetVariableType() encountered an unknown UInt size: {type.NumBytes}");
            }
        }

        private unsafe void PopulateReflectionParamters(ShaderCompilerContext context, ShaderReflection result, SpirvEntryPoint ep, ShaderIOLayoutType type)
        {
            List<ShaderParameterInfo> parameters;
            IReadOnlyList<SpirvVariable> variables;

            switch (type)
            {
                case ShaderIOLayoutType.Input:
                    variables = ep.Inputs;
                    parameters = result.InputParameters;
                    break;

                case ShaderIOLayoutType.Output:
                    variables = ep.Outputs;
                    parameters = result.OutputParameters;
                    break;

                default:
                    return;
            }

            for (int i = 0; i < variables.Count; i++)
            {
                SpirvVariable v = variables[i];

                ShaderParameterInfo p = new ShaderParameterInfo()
                {
                    ComponentType = GetRegisterType(v.Type),
                    Mask = ShaderComponentMaskFlags.All,
                    ReadWriteMask = 255,
                    MinPrecision = GetMinPrecision(v.Type),
                    Register = v.Binding.HasValue ? v.Binding.Value : 0,
                    SemanticIndex = v.Binding.HasValue ? v.Binding.Value : 0,
                    Stream = 0,
                };

                ProcessDecorations(p, v);

                // Try to get the semantic name using the variable name.
                if (string.IsNullOrWhiteSpace(p.SemanticName))
                {
                    if(v.Name.StartsWith("in.var."))
                        p.SemanticName = v.Name.Substring(7);
                    else if(v.Name.StartsWith("out.var."))
                        p.SemanticName = v.Name.Substring(8);
                }

                // Try to figure out the HLSL system value (SV_) type.
                if (!string.IsNullOrWhiteSpace(p.SemanticName))
                {
                    p.SystemValueType = GetSystemValue(context.Type, p);
                    if (p.SystemValueType != ShaderSVType.Undefined)
                        p.SemanticName = $"SV_{p.SystemValueType}".ToUpper();

                    p.SemanticNamePtr = (void*)SilkMarshal.StringToPtr(p.SemanticName, NativeStringEncoding.UTF8);
                }

                parameters.Add(p);
            }
        }

        private ShaderRegisterType GetRegisterType(SpirvType t)
        {
            if (t.Kind == SpirvTypeKind.Vector)
                t = t.ElementType;
            else if (t.Kind == SpirvTypeKind.Matrix)
                t = t.ElementType.ElementType;

            switch (t.Kind)
            {
                default:
                    return ShaderRegisterType.Unknown;

                case SpirvTypeKind.Float:
                    switch (t.NumBytes)
                    {
                        default: throw new Exception($"Unsupported float length: {t.NumBytes}");
                        case 2: return ShaderRegisterType.Float16;
                        case 4: return ShaderRegisterType.Float32;
                        case 8: return ShaderRegisterType.Float64;
                    }

                case SpirvTypeKind.Int:
                    switch (t.NumBytes)
                    {
                        default: throw new Exception($"Unsupported int length: {t.NumBytes}");
                        case 1: return ShaderRegisterType.SInt8;
                        case 2: return ShaderRegisterType.SInt16;
                        case 4: return ShaderRegisterType.SInt32;
                        case 8: return ShaderRegisterType.SInt64;
                    }

                case SpirvTypeKind.UInt:
                    switch (t.NumBytes)
                    {
                        default: throw new Exception($"Unsupported uint length: {t.NumBytes}");
                        case 1: return ShaderRegisterType.UInt8;
                        case 2: return ShaderRegisterType.UInt16;
                        case 4: return ShaderRegisterType.UInt32;
                        case 8: return ShaderRegisterType.UInt64;
                    }
            }
        }

        private ShaderMinPrecision GetMinPrecision(SpirvType type)
        {
            switch (type.Kind)
            {
                case SpirvTypeKind.Int:
                    switch (type.NumBytes)
                    {
                        default: return ShaderMinPrecision.Default;
                        case 2: return ShaderMinPrecision.Sint16;
                        case 10: return ShaderMinPrecision.Any10;
                    }

                case SpirvTypeKind.Float:
                    switch (type.NumBytes)
                    {
                        default: throw new Exception($"Unsupported float length: {type.NumBytes}");
                        case 2: return ShaderMinPrecision.Float16;
                        case 10: return ShaderMinPrecision.Float28;
                    }

                case SpirvTypeKind.UInt:
                    switch (type.NumBytes)
                    {
                        default: return ShaderMinPrecision.Default;
                        case 2: return ShaderMinPrecision.Uint16;
                        case 10: return ShaderMinPrecision.Any10;
                    }

                default:
                    return ShaderMinPrecision.Default;
            }  
        }

        private void ProcessDecorations(ShaderParameterInfo p, SpirvVariable v)
        {
            // Prioritize UserSemantic decoration semantics
            foreach (SpirvDecoration dec in v.Decorations.Keys)
            {
                IReadOnlyList<object> parameters = v.Decorations[dec];
                if (dec != SpirvDecoration.UserSemantic)
                    continue;

                p.SemanticName = parameters[0] as string;
                break;
            }

            // Now for core SPIR-V decorations
            foreach (SpirvDecoration dec in v.Decorations.Keys)
            {
                IReadOnlyList<object> parameters = v.Decorations[dec];

                switch (dec)
                {
                    case SpirvDecoration.BuiltIn:
                        if (string.IsNullOrWhiteSpace(p.SemanticName))
                        {
                            SpirvBuiltIn bi = (SpirvBuiltIn)parameters[0];
                            p.SemanticName = BuiltInToSemantic(bi);
                        }
                        break;

                    case SpirvDecoration.Location:
                        p.SemanticIndex = (uint)parameters[0];
                        break;
                }
            }
        }

        /// <summary>
        /// See for more info: https://registry.khronos.org/SPIR-V/specs/1.0/SPIRV.html#BuiltIn
        /// </summary>
        /// <param name="builtIn"></param>
        /// <returns></returns>
        private string BuiltInToSemantic(SpirvBuiltIn builtIn)
        {
            switch (builtIn)
            {
                default:
                    return $"GL_{builtIn.ToString().ToUpper()}";

                case SpirvBuiltIn.InstanceId:
                case SpirvBuiltIn.PrimitiveId:
                case SpirvBuiltIn.Position:
                case SpirvBuiltIn.VertexId:
                    return $"SV_{builtIn.ToString().ToUpper()}";
            }
        }

        private ShaderSVType GetSystemValue(ShaderType shaderType, ShaderParameterInfo p)
        {
            string semanticName = p.SemanticName.ToLower();

            // Remove the semantic index from the semantic name, if present.
            if(semanticName.EndsWith(p.SemanticIndex.ToString()))
                semanticName = semanticName.Substring(0, semanticName.Length - p.SemanticIndex.ToString().Length);

            // Translate GLSL to HLSL semantics
            if (semanticName.StartsWith("gl_"))
            {
                switch (semanticName)
                {
                    case "gl_vertexid":
                    case "gl_vertexindex":
                        return ShaderSVType.VertexID;

                    case "gl_clipdistance":
                        return ShaderSVType.ClipDistance;

                    case "gl_culldistance":
                        return ShaderSVType.CullDistance;

                    case "gl_samplemaskin":
                    case "gl_samplemask":
                        return ShaderSVType.Coverage;

                    case "gl_fragdepth":
                        // TODO intepret whether layout() is depth_less or depth_greater
                        //      and determine whether this is SV_DepthLessEqual or SV_DepthGreaterEqual
                        return ShaderSVType.Depth;

                    case "gl_globalinvocationid":
                        return ShaderSVType.DispatchThreadID;

                    case "gl_tesscord":
                        return ShaderSVType.DomainLocation;

                    case "gl_workgroupid":
                        return ShaderSVType.GroupID;

                    case "gl_localinvocationindex":
                        return ShaderSVType.GroupIndex;

                    case "gl_localinvocationid":
                        return ShaderSVType.GroupThreadID;

                    case "gl_invocationid":
                        if (shaderType == ShaderType.Hull)
                            return ShaderSVType.OutputControlPointID;
                        else
                            return ShaderSVType.GSInstanceID;

                    case "gl_tesslevelinner":
                        return ShaderSVType.InsideTessFactor;

                    case "gl_instnaceid":
                    case "gl_instanceindex":
                        return ShaderSVType.InstanceID;

                    case "gl_frontfacing":
                        return ShaderSVType.IsFrontFace;

                    case "gl_patchverticesin":
                        return ShaderSVType.Undefined; // TODO support?

                    case "gl_position":
                    case "gl_fragcoord":
                        return ShaderSVType.Position;

                    case "gl_primitiveid":
                        return ShaderSVType.PrimitiveID;

                    case "gl_layer":
                        return ShaderSVType.RenderTargetArrayIndex;

                    case "gl_sampleid":
                        return ShaderSVType.SampleIndex;

                    case "gl_sampleposition":
                        return ShaderSVType.Undefined; // TODO The equivalent functionality is available through EvaluateAttributeAtSample

                    case "gl_stencilref":
                        return ShaderSVType.StencilRef;

                    case "gl_tesslevelouter":
                        return ShaderSVType.TessFactor;

                    case "gl_viewportindex":
                        return ShaderSVType.ViewportArrayIndex;
                }
            }
            else
            {
                // Only translate HLSL SV_ semantics.
                if (semanticName.StartsWith("sv_"))
                {
                    semanticName = semanticName.Substring(3);
                    if (Enum.TryParse(semanticName, true, out ShaderSVType result))
                        return result;
                }
            }

            return ShaderSVType.Undefined;
        }
    }
}
