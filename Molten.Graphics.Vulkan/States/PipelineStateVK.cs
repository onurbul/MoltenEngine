﻿using System.Text;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal unsafe class PipelineStateVK : GraphicsObject
    {
        /// <summary>
        /// A list of all shader stages in the order they are expected by Vulkan and Molten.
        /// </summary>
        internal static readonly ShaderType[] ShaderTypes = new ShaderType[] {
            ShaderType.Vertex,
            ShaderType.Hull,
            ShaderType.Domain,
            ShaderType.Geometry,
            ShaderType.Pixel,
            ShaderType.Compute
        };

        internal static readonly Dictionary<ShaderType, ShaderStageFlags> ShaderStageLookup = new Dictionary<ShaderType, ShaderStageFlags>()
        {
            [ShaderType.Vertex] = ShaderStageFlags.VertexBit,
            [ShaderType.Hull] = ShaderStageFlags.TessellationControlBit,
            [ShaderType.Domain] = ShaderStageFlags.TessellationEvaluationBit,
            [ShaderType.Geometry] = ShaderStageFlags.GeometryBit,
            [ShaderType.Pixel] = ShaderStageFlags.FragmentBit
        };

        GraphicsPipelineCreateInfo _info;
        Pipeline _pipeline;
        List<PipelineStateVK> _derivatives = new List<PipelineStateVK>();

        BlendStateVK _blendState;
        DepthStateVK _depthState;
        RasterizerStateVK _rasterizerState;
        DynamicStateVK _dynamicState;
        InputAssemblyStateVK _inputState;
        PipelineLayoutVK _pipelineLayout;
        RenderPassVK _renderPass;

        public PipelineStateVK(DeviceVK device, ShaderPassVK pass, ref ShaderPassParameters parameters) : 
            base(device)
        {
            _info = new GraphicsPipelineCreateInfo();
            _info.SType = StructureType.GraphicsPipelineCreateInfo;
            _info.Flags = PipelineCreateFlags.None;
            _info.PNext = null;

            // Populate dynamic state
            _blendState = new BlendStateVK(device, ref parameters);
            device.Cache.Object<BlendStateVK, PipelineColorBlendStateCreateInfo>(ref _blendState);

            _depthState = new DepthStateVK(device, ref parameters);
            device.Cache.Object<DepthStateVK, PipelineDepthStencilStateCreateInfo>(ref _depthState);

            _rasterizerState = new RasterizerStateVK(device, ref parameters);
            device.Cache.Object<RasterizerStateVK, PipelineRasterizationStateCreateInfo>(ref _rasterizerState);

            _inputState = new InputAssemblyStateVK(device, ref parameters);
            device.Cache.Object<InputAssemblyStateVK, PipelineInputAssemblyStateCreateInfo>(ref _inputState);

            DynamicState[] dynamics = new DynamicState[]
            {
                DynamicState.ViewportWithCount,
                DynamicState.ScissorWithCount,
            };

            _dynamicState = new DynamicStateVK(device, ref parameters, dynamics);
            device.Cache.Object<DynamicStateVK, PipelineDynamicStateCreateInfo>(ref _dynamicState);

            // Setup shader stage info
            _info.PStages = EngineUtil.AllocArray<PipelineShaderStageCreateInfo>((uint)pass.CompositionCount);
            _info.StageCount = 0;

            // Iterate over and add pass compositions in the order Vulkan expects.
            foreach (ShaderType type in ShaderTypes)
            {
                ShaderComposition c = pass[type];
                if (c == null)
                    continue;

                ref PipelineShaderStageCreateInfo stageDesc = ref _info.PStages[_info.StageCount++];
                stageDesc.SType = StructureType.PipelineShaderStageCreateInfo;
                stageDesc.PName = EngineUtil.StringToPtr(c.EntryPoint, Encoding.UTF8);
                stageDesc.Stage = ShaderStageLookup[type];
                stageDesc.Module = *(ShaderModule*)c.PtrShader;
                stageDesc.Flags = PipelineShaderStageCreateFlags.None;
                stageDesc.PNext = null;
            }

            _pipelineLayout = new PipelineLayoutVK(device, pass.DescriptorLayout);
            device.Cache.Object<PipelineLayoutVK, PipelineLayoutCreateInfo>(ref _pipelineLayout);

            _info.PMultisampleState = null;                         // TODO initialize
            _info.BasePipelineIndex = 0;                            // TODO initialize
            _info.BasePipelineHandle = new Pipeline();              // TODO initialize 
            _info.PTessellationState = null;                        // TODO initialize
            _info.PVertexInputState = null;                         // TODO initialize
            _info.PViewportState = null;                            // Ignored. Set in dynamic state.
            _info.Subpass = 0;                                      // TODO initialize

            _info.PColorBlendState = _blendState.Desc;
            _info.PRasterizationState = _rasterizerState.Desc;
            _info.PDepthStencilState = _depthState.Desc;
            _info.PDynamicState = _dynamicState.Desc;
            _info.PInputAssemblyState = _inputState.Desc;
            _info.Layout = _pipelineLayout.Handle;

            // TODO initialize - This should be stored in RenderStep based on input and output surfaces.
            if (_renderPass != null)
                _info.RenderPass = _renderPass.Handle;
            else
                _info.RenderPass = new RenderPass();

            /* TODO Implement derivative pipeline support:
             *   - ShaderPass will provide a base PassPipelineVK instance
             *   - GraphicsDevice will store PassPipelineVK instances by ShaderPassVK and render surface count + type
             *   - GraphicsQueue.ApplyRenderState() or ApplyComputeState() must check the pipeline cache for a matching pipelines based on: 
             *      - Bound render surfaces
             *   - See: https://registry.khronos.org/vulkan/specs/1.3-extensions/html/vkspec.html#pipelines-pipeline-derivatives
             *      - Ensure VK_PIPELINE_CREATE_DERIVATIVE_BIT is set on derivative pipelines.
             *      - Base pipeline must set VK_PIPELINE_CREATE_ALLOW_DERIVATIVES_BIT 
             *      
             *   - Implement PipelineCacheVK:
             *      - Implement PipelineVK and move all properties out of ShaderPassVK and into PipelineVK
             */

            // Create pipeline.
            fixed (Pipeline* ptrPipeline = &_pipeline)
                device.VK.CreateGraphicsPipelines(device, new PipelineCache(), 1, _info, null, ptrPipeline);

            // TODO after render/compute pass, reset the load-op of surfaces.
        }

        private PipelineStateVK(DeviceVK device, PipelineStateVK baseState, IRenderSurfaceVK[] surfaces, DepthSurfaceVK depthSurface) :
            base(device)
        {
            if (baseState == null)
                throw new ArgumentNullException(nameof(baseState), "Base state cannot be null");

            _info = new GraphicsPipelineCreateInfo();
            _info.SType = StructureType.GraphicsPipelineCreateInfo;
            _info.Flags = PipelineCreateFlags.CreateAllowDerivativesBit | PipelineCreateFlags.CreateDerivativeBit;
            _info.PNext = null;
            _info.BasePipelineHandle = baseState;
            _info.RenderPass = device.GetRenderPass(surfaces, depthSurface).Handle;
            BaseState = baseState;
        }

        private PipelineStateVK(DeviceVK device, PipelineStateVK baseState, FrameBufferVK frameBuffer) : 
            base(device)
        {
            if (baseState == null)
                throw new ArgumentNullException(nameof(baseState), "Base state cannot be null");

            _info = new GraphicsPipelineCreateInfo();
            _info.SType = StructureType.GraphicsPipelineCreateInfo;
            _info.Flags = PipelineCreateFlags.CreateAllowDerivativesBit | PipelineCreateFlags.CreateDerivativeBit;
            _info.PNext = null;
            _info.BasePipelineHandle = baseState;
            BaseState = baseState;
        }

        internal PipelineStateVK GetState(IRenderSurfaceVK[] surfaces, DepthSurfaceVK depthSurface)
        {
            DeviceVK device = Device as DeviceVK;
            if (_renderPass != null && _renderPass.DoSurfacesMatch(device, surfaces, depthSurface))
                return this;


            // Check if we have an existing derivative that matches our surface attachments.
            foreach (PipelineStateVK derivative in _derivatives)
            {
                if (derivative._renderPass == null)
                    continue;

                if (derivative._renderPass.DoSurfacesMatch(device, surfaces, depthSurface))
                    return derivative;
            }

            PipelineStateVK derivation = new PipelineStateVK(device, this, surfaces, depthSurface);
            _derivatives.Add(derivation);
            return derivation;
        }

        protected override void OnGraphicsRelease()
        {
            DeviceVK device = Device as DeviceVK;

            // Release indirect memory allocations for pipleine shader stages
            for (uint i = 0; i < _info.StageCount; i++)
            {
                ref PipelineShaderStageCreateInfo stageDesc = ref _info.PStages[i];
                EngineUtil.Free(ref stageDesc.PName);
            }

            EngineUtil.Free(ref _info.PStages);

            _pipelineLayout.Dispose();

            if (_pipeline.Handle != 0)
            {
                device.VK.DestroyPipeline(device, _pipeline, null);
                _pipeline = new Pipeline();
            }
        }

        public static implicit operator Pipeline(PipelineStateVK state)
        {
            return state._pipeline;
        }

        internal PipelineStateVK BaseState { get; }

        internal RenderPassVK RenderPass => _renderPass;

        internal PipelineLayoutVK Layout => _pipelineLayout;
    }
}
