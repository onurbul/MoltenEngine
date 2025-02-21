﻿using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;
using Semaphore = Silk.NET.Vulkan.Semaphore;
using VKViewport = Silk.NET.Vulkan.Viewport;

namespace Molten.Graphics.Vulkan
{
    public unsafe class GraphicsQueueVK : GraphicsQueue
    {
        DeviceVK _device;
        Vk _vk;
        CommandPoolVK _poolFrame;
        CommandPoolVK _poolTransient;
        CommandListVK _cmd;

        Stack<DebugUtilsLabelEXT> _eventLabelStack;
        IRenderSurfaceVK[] _applySurfaces;
        FrameBufferedArray<Rect2D> _applyScissors;
        FrameBufferedArray<ClearValue> _applyClearValues;
        FrameBufferedArray<VKViewport> _applyViewports;
        FrameBufferedArray<Buffer> _applyVertexBuffers;
        FrameBufferedArray<DescriptorSet> _applyDescSets;

        internal GraphicsQueueVK(RendererVK renderer, DeviceVK device, uint familyIndex, Queue queue, uint queueIndex, SupportedCommandSet set) :
            base(device)
        {
            _vk = renderer.VK;
            Log = renderer.Log;
            Flags = set.CapabilityFlags;
            _device = device;
            FamilyIndex = familyIndex;
            Index = queueIndex;
            Native = queue;
            Set = set;

            uint maxSurfaces = (uint)State.Surfaces.Length;
            uint maxVBuffers = (uint)device.Capabilities.VertexBuffers.MaxSlots;
            uint maxDescSets = 16U; // TODO: (uint)device.Capabilities.MaxBoundDescriptors;
            _applySurfaces = new IRenderSurfaceVK[maxSurfaces];
            _applyScissors = new FrameBufferedArray<Rect2D>(device, maxSurfaces);
            _applyClearValues = new FrameBufferedArray<ClearValue>(device, maxSurfaces);
            _applyViewports = new FrameBufferedArray<VKViewport>(device, maxSurfaces);
            _applyVertexBuffers = new FrameBufferedArray<Buffer>(device, maxVBuffers);
            _applyDescSets = new FrameBufferedArray<DescriptorSet>(device, maxDescSets);

            _eventLabelStack = new Stack<DebugUtilsLabelEXT>();
            _poolFrame = new CommandPoolVK(this, CommandPoolCreateFlags.ResetCommandBufferBit, 1);
            _poolTransient = new CommandPoolVK(this, CommandPoolCreateFlags.ResetCommandBufferBit | CommandPoolCreateFlags.TransientBit, 5);
        }

        public override unsafe void Begin(GraphicsCommandListFlags flags)
        {
            base.Begin();

            CommandBufferLevel level = flags.Has(GraphicsCommandListFlags.Deferred) ?
                CommandBufferLevel.Secondary :
                CommandBufferLevel.Primary;

            CommandBufferBeginInfo beginInfo = new CommandBufferBeginInfo(StructureType.CommandBufferBeginInfo);
            beginInfo.Flags = CommandBufferUsageFlags.None;

            if(flags.Has(GraphicsCommandListFlags.SingleSubmit))
                beginInfo.Flags |= CommandBufferUsageFlags.OneTimeSubmitBit;

            _cmd = _poolFrame.Allocate(level, Device.Frame.BranchCount++, flags);
            Device.Frame.Track(_cmd);
            _vk.BeginCommandBuffer(_cmd, &beginInfo);
        }

        public override GraphicsCommandList End()
        {
            base.End();

            _vk.EndCommandBuffer(_cmd);

            if (_cmd.Flags.Has(GraphicsCommandListFlags.Deferred))
                return _cmd;

            // Use empty fence handle if the CPU doesn't need to wait for the command list to finish.
            Fence fence = new Fence();
            if (_cmd.Fence != null)
                fence = (_cmd.Fence as FenceVK).Ptr;

            // Submit command list and don't return the command list, as it's not deferred.
            SubmitCommandList(_cmd, fence);
            return null;
        }

        /// <inheritdoc/>
        public override unsafe void Execute(GraphicsCommandList list)
        {
            CommandListVK vkList = list as CommandListVK;
            if (vkList.Level != CommandBufferLevel.Secondary)
                throw new InvalidOperationException("Cannot submit a queue-level command list to a queue");

            CommandBuffer* cmdBuffers = stackalloc CommandBuffer[1] { vkList.Ptr };
            _vk.CmdExecuteCommands(_cmd, 1, cmdBuffers);
        }

        /// <inheritdoc/>
        public override unsafe void Sync(GraphicsCommandListFlags flags = GraphicsCommandListFlags.None)
        {
            if (_cmd.Level != CommandBufferLevel.Primary)
            {
                throw new InvalidOperationException($"Cannot submit a secondary command list directly to a command queue.");
            }
            else
            {
                if (flags.Has(GraphicsCommandListFlags.Deferred))
                    throw new InvalidOperationException($"An immediate/primary command list branch cannot use deferred flag during Sync() calls.");
            }

            // Use empty fence handle if the CPU doesn't need to wait for the command list to finish.
            Fence fence = new Fence();
            if (_cmd.Fence != null)
                fence = (_cmd.Fence as FenceVK).Ptr;

            // We're only submitting the current command buffer.
            _vk.EndCommandBuffer(_cmd);
            SubmitCommandList(_cmd, fence);

            // Allocate next command buffer
            _cmd = _poolFrame.Allocate(_cmd.Level, _cmd.BranchIndex, flags);
            CommandBufferBeginInfo beginInfo = new CommandBufferBeginInfo(StructureType.CommandBufferBeginInfo);
            beginInfo.Flags = CommandBufferUsageFlags.OneTimeSubmitBit;

            _vk.BeginCommandBuffer(_cmd, &beginInfo);
            Device.Frame.Track(_cmd);
        }

        private unsafe void SubmitCommandList(CommandListVK cmd, Fence fence)
        {
            CommandBuffer* ptrBuffers = stackalloc CommandBuffer[] { _cmd.Ptr };
            SubmitInfo submit = new SubmitInfo(StructureType.SubmitInfo);
            submit.PCommandBuffers = ptrBuffers;

            // We want to wait on the previous command list's semaphore before executing this one, if any.
            if (_cmd.Previous != null)
            {
                Semaphore* waitSemaphores = stackalloc Semaphore[] { (_cmd.Previous as CommandListVK).Semaphore.Ptr };
                submit.WaitSemaphoreCount = 1;
                submit.PWaitSemaphores = waitSemaphores;
            }
            else
            {
                submit.WaitSemaphoreCount = 0;
                submit.PWaitSemaphores = null;
            }

            // We want to signal the command list's own semaphore so that the next command list can wait on it, if needed.
            _cmd.Semaphore.Start(SemaphoreCreateFlags.None);
            Semaphore* semaphore = stackalloc Semaphore[] { _cmd.Semaphore.Ptr };
            submit.CommandBufferCount = 1;
            submit.SignalSemaphoreCount = 1;
            submit.PSignalSemaphores = semaphore;

            Result r = VK.QueueSubmit(Native, 1, &submit, fence);
            r.Throw(_device, () => "Failed to submit command list");
        }

        protected override void OnResetState()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Queues a texture memory barrier.
        /// </summary>
        /// <param name="srcFlags">The source stage flags.</param>
        /// <param name="destFlags">The destination stage flags.</param>
        /// <param name="barrier">A pointer to one or more image memory barriers.</param>
        /// <param name="barrierCount">The number of memory barriers in the <paramref name="barrier"/> parameter.</param>
        internal unsafe void MemoryBarrier(PipelineStageFlags srcFlags, PipelineStageFlags destFlags, ImageMemoryBarrier* barrier, uint barrierCount = 1)
        {
            _vk.CmdPipelineBarrier(_cmd, srcFlags, destFlags, DependencyFlags.None, 0, null, 0, null, barrierCount, barrier);
        }

        /// <summary>
        /// Queues a buffer memory barrier.
        /// </summary>
        /// <param name="srcFlags">The source stage flags.</param>
        /// <param name="destFlags">The destination stage flags.</param>
        /// <param name="barrier">A pointer to one or more buffer memory barriers.</param>
        /// <param name="barrierCount">The number of memory barriers in the <paramref name="barrier"/> parameter.</param>
        internal unsafe void MemoryBarrier(PipelineStageFlags srcFlags, PipelineStageFlags destFlags, BufferMemoryBarrier* barrier, uint barrierCount = 1)
        {
            _vk.CmdPipelineBarrier(_cmd, srcFlags, destFlags, DependencyFlags.None, 0, null, barrierCount, barrier, 0, null);
        }

        /// <summary>
        /// Queues a global memory barrier command.
        /// </summary>
        /// <param name="srcFlags">The source stage flags.</param>
        /// <param name="destFlags">The destination stage flags.</param>
        /// <param name="barrier">A pointer to one or more global memory barriers.</param>
        /// <param name="barrierCount">The number of memory barriers in the <paramref name="barrier"/> parameter.</param>
        internal unsafe void MemoryBarrier(PipelineStageFlags srcFlags, PipelineStageFlags destFlags, MemoryBarrier* barrier, uint barrierCount = 1)
        {
            _vk.CmdPipelineBarrier(_cmd, srcFlags, destFlags, DependencyFlags.None, barrierCount, barrier, 0, null, 0, null);
        }

        internal unsafe void ClearImage(Image image, ImageLayout layout, Color color, ImageSubresourceRange* pRanges, uint numRanges)
        {
            _vk.CmdClearColorImage(_cmd, image, layout, *(ClearColorValue*)&color, numRanges, pRanges);
        }

        internal unsafe void ClearDepthImage(Image image, ImageLayout layout, float depthValue, uint stencilValue, ImageSubresourceRange* pRanges, uint numRanges)
        {
            ClearDepthStencilValue values = new ClearDepthStencilValue(depthValue, stencilValue);
            _vk.CmdClearDepthStencilImage(_cmd, image, layout, &values, numRanges, pRanges);
        }

        internal bool HasFlags(CommandSetCapabilityFlags flags)
        {
            return (Flags & flags) == flags;
        }

        protected override void OnDispose()
        {
            _applyScissors.Dispose();
            _applyViewports.Dispose();
            _applyClearValues.Dispose();
            _applyVertexBuffers.Dispose();

            _poolFrame.Dispose();
            _poolTransient.Dispose();
        }

        public override unsafe void BeginEvent(string label)
        {
            RendererVK renderer = Device.Renderer as RendererVK;
            byte* ptrString = EngineUtil.StringToPtr(label, Encoding.UTF8);
            DebugUtilsLabelEXT lbl = new DebugUtilsLabelEXT(pLabelName: ptrString);
            float* ptrColor = stackalloc float[] { 1f, 1f, 1f, 1f };

            NativeMemory.Copy(&ptrColor, lbl.Color, sizeof(float) * 4);
            renderer.DebugLayer.Module.CmdBeginDebugUtilsLabel(_cmd, &lbl);
            _eventLabelStack.Push(lbl);
        }

        public unsafe override void EndEvent()
        {
            RendererVK renderer = Device.Renderer as RendererVK;
            DebugUtilsLabelEXT lbl = _eventLabelStack.Pop();

            renderer.DebugLayer.Module.CmdEndDebugUtilsLabel(_cmd);

            EngineUtil.Free(ref lbl.PLabelName);
        }

        public unsafe override void SetMarker(string label)
        {
            RendererVK renderer = Device.Renderer as RendererVK;
            byte* ptrString = EngineUtil.StringToPtr(label, Encoding.UTF8);
            DebugUtilsLabelEXT lbl = new DebugUtilsLabelEXT(pLabelName: ptrString);
            float* ptrColor = stackalloc float[] { 1f, 1f, 1f, 1f };

            NativeMemory.Copy(&ptrColor, lbl.Color, sizeof(float) * 4);
            renderer.DebugLayer.Module.CmdBeginDebugUtilsLabel(_cmd, &lbl);
            renderer.DebugLayer.Module.CmdEndDebugUtilsLabel(_cmd);

            EngineUtil.Free(ref ptrString);
        }

        protected override unsafe ResourceMap GetResourcePtr(GraphicsResource resource, uint subresource, GraphicsMapType mapType)
        {
            ResourceHandleVK handle = (ResourceHandleVK)resource.Handle;
            if (mapType == GraphicsMapType.Discard)
                handle.Discard();

            ResourceMap map = new ResourceMap(null, resource.SizeInBytes, resource.SizeInBytes); // TODO Calculate correct RowPitch value when mapping textures
            Result r = _vk.MapMemory(_device, handle.Memory, 0, resource.SizeInBytes, 0, &map.Ptr);

            if (!r.Check(_device))
                return new ResourceMap();

            return map;
        }

        protected override unsafe void OnUnmapResource(GraphicsResource resource, uint subresource)
        {
            // TODO unmap isn't actually needed in certain circumstances. If the mapped memory will be re-populated every frame (e.g. uniform buffer)
            //      we can permenantly leave the memory mapped via vkMapMemory().
            // TODO optimize this accordingly to buffer type and usage.
            _vk.UnmapMemory(_device, (((ResourceHandleVK)resource.Handle).Memory));
        }

        protected override unsafe void UpdateResource(GraphicsResource resource, uint subresource, ResourceRegion? region, void* ptrData, uint rowPitch, uint slicePitch)
        {
            ResourceHandleVK handle = (ResourceHandleVK)resource.Handle;

            // Can we write directly to image memory?
            if (handle.Memory.Flags.Has(MemoryPropertyFlags.HostVisibleBit))
            {
                // TODO set the offset to match the provided region, writing row-by-row based on the rowPitch.

                using (GraphicsStream stream = MapResource(resource, subresource, 0, GraphicsMapType.Write))
                    stream.WriteRange(ptrData, slicePitch);
            }
            else
            {
                // Use a staging buffer to transfer the data to the provided resource instead.
                using (GraphicsStream stream = MapResource(Device.Frame.StagingBuffer, 0, 0, GraphicsMapType.Write))
                    stream.WriteRange(ptrData, slicePitch);

                CopyResource(Device.Frame.StagingBuffer, resource);
            }
        }

        protected override unsafe void CopyResource(GraphicsResource src, GraphicsResource dest)
        {
            if (src is BufferVK srcBuffer)
            {
                Buffer srcHandle = *srcBuffer.Handle.NativePtr;

                if (dest is BufferVK dstBuffer)
                {
                    Buffer dstHandle = *dstBuffer.Handle.NativePtr;
                    Span<BufferCopy> copy = stackalloc BufferCopy[] {
                        new BufferCopy(0, 0, src.SizeInBytes)
                    };

                    _vk.CmdCopyBuffer(_cmd, srcHandle, dstHandle, copy);
                }
                else if (dest is TextureVK dstTex)
                {
                    Image dstHandle = *dstTex.Handle.NativePtr;
                    Offset3D offset = new Offset3D(0, 0, 0);
                    Extent3D extent = new Extent3D(dstTex.Width, dstTex.Height, dstTex.Depth);

                    // TODO set the image aspect flags based on texture type. e.g. is DepthTextureVK or standard TextureVK/surface.

                    ImageSubresourceLayers layers = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1);
                    Span<BufferImageCopy> regions = stackalloc BufferImageCopy[] {
                        new BufferImageCopy(0, 0, src.SizeInBytes, layers, offset, extent)
                    };

                    _vk.CmdCopyBufferToImage(_cmd, srcHandle, dstHandle, ImageLayout.TransferDstOptimal, regions);
                }
            }
            else if (src is TextureVK srcTex)
            {
                Image srcHandle = *srcTex.Handle.NativePtr;
                Offset3D srcOffset = new Offset3D(0, 0, 0);

                // TODO set the image aspect flags based on texture type. e.g. is DepthTextureVK or standard TextureVK/surface.
                ImageSubresourceLayers srcLayers = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1);
                Extent3D srcExtent = new Extent3D(srcTex.Width, srcTex.Height, srcTex.Depth);

                if (dest is BufferVK dstBuffer)
                {
                    Buffer dstHandle = *dstBuffer.Handle.NativePtr;
                    Span<BufferImageCopy> regions = stackalloc BufferImageCopy[] {
                        new BufferImageCopy(0, 0, src.SizeInBytes, srcLayers, srcOffset, srcExtent)
                    };

                    _vk.CmdCopyImageToBuffer(_cmd, srcHandle, ImageLayout.TransferSrcOptimal, dstHandle, regions);
                }
                else if (dest is TextureVK dstTex)
                {
                    Image dstHandle = *dstTex.Handle.NativePtr;
                    Offset3D destOffset = new Offset3D(0, 0, 0);

                    // TODO set the image aspect flags based on texture type. e.g. is DepthTextureVK or standard TextureVK/surface.

                    ImageSubresourceLayers destLayers = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1);
                    Span<ImageCopy> regions = stackalloc ImageCopy[] {
                        new ImageCopy(srcLayers, srcOffset, destLayers, destOffset, srcExtent),
                    };

                    _vk.CmdCopyImage(_cmd, srcHandle, ImageLayout.TransferSrcOptimal, dstHandle, ImageLayout.TransferDstOptimal, regions);
                }
            }
            else
            {
                throw new InvalidOperationException($"Unsupported copy resource type '{src.GetType().Name}'");
            }
        }

        public override unsafe void CopyResourceRegion(GraphicsResource source, uint srcSubresource, ResourceRegion* sourceRegion, GraphicsResource dest, uint destSubresource, Vector3UI destStart)
        {
            throw new NotImplementedException();
        }

        protected override unsafe GraphicsBindResult DoRenderPass(HlslPass hlslPass, QueueValidationMode mode, Action callback)
        {
            ShaderPassVK pass = hlslPass as ShaderPassVK;
            GraphicsBindResult vResult = Validate(mode);

            if (vResult != GraphicsBindResult.Successful)
                return vResult;

            DepthSurfaceVK depthSurface = State.DepthSurface.Value as DepthSurfaceVK;

            // Gather all surfaces and scissor rectangles.
            uint maxSurfaceCount = (uint)_applySurfaces.Length;
            uint surfaceID = 0;
            for (; surfaceID < maxSurfaceCount; surfaceID++)
            {
                IRenderSurfaceVK surface = State.Surfaces[surfaceID] as IRenderSurfaceVK;
                _applySurfaces[surfaceID] = surface;

                // Get scissor rectangles.
                Rectangle r = State.ScissorRects[surfaceID];
                _applyScissors[surfaceID] = new Rect2D()
                {
                    Offset = new Offset2D(r.X, r.Y),
                    Extent = new Extent2D((uint)r.Width, (uint)r.Height)
                };

                ViewportF vp = State.Viewports[surfaceID];
                _applyViewports[surfaceID] = new VKViewport()
                {
                    X = vp.X,
                    Y = vp.Y,
                    Width = vp.Width,
                    Height = vp.Height,
                    MinDepth = vp.MinDepth,
                    MaxDepth = vp.MaxDepth,
                };

                // Get clear color
                _applyClearValues[surfaceID] = new ClearValue();
                if (surface.ClearColor.HasValue)
                {
                    Color4 color = surface.ClearColor.Value;
                    _applyClearValues[surfaceID].Color = new ClearColorValue() // TODO handle formats that are not RGBA layout (e.g. BGRA)
                    {
                        Float32_0 = color.R,
                        Float32_1 = color.G,
                        Float32_2 = color.B,
                        Float32_3 = color.A,
                    };
                }
            }

            // Get depth surface clear value, if any.
            if (depthSurface != null)
            {
                _applyClearValues[surfaceID] = new ClearValue();
                if (depthSurface.ClearValue.HasValue)
                    _applyClearValues[surfaceID].DepthStencil = depthSurface.ClearValue.Value;
            }

            // Re-render the same pass for K iterations.
            for (int k = 0; k < pass.Iterations; k++)
            {
                BeginEvent($"Iteration {k}"); 

                Buffer iBuffer = new Buffer();
                IndexType iType = IndexType.NoneKhr;
                if (State.IndexBuffer.BoundValue != null)
                {
                    BufferVK vkIndexBuffer = State.IndexBuffer.BoundValue as BufferVK;
                    iBuffer = *vkIndexBuffer.Handle.NativePtr;
                    switch (vkIndexBuffer.ResourceFormat)
                    {
                        case GraphicsFormat.R16_UInt:
                            iType = IndexType.Uint16;
                            break;
                        case GraphicsFormat.R32_UInt:
                            iType = IndexType.Uint32;
                            break;

                        case GraphicsFormat.R8_UInt:
                            iType = IndexType.Uint8Ext;
                            break;

                        default:
                            throw new InvalidOperationException($"Unsupported index format '{vkIndexBuffer.ResourceFormat}'");
                    }
                }

                PipelineStateVK pipeState = pass.State.GetState(_applySurfaces, depthSurface);
                FrameBufferVK frameBuffer = _device.GetFrameBuffer(pipeState.RenderPass, _applySurfaces, depthSurface);

                RenderPassBeginInfo rpInfo = new RenderPassBeginInfo()
                {
                    SType = StructureType.RenderPassBeginInfo,
                    RenderPass = pipeState.RenderPass,
                    Framebuffer = frameBuffer,
                    RenderArea = _applyScissors[0],         // TODO Use a rectangle that contains all of the provided scissor rectangles
                    PClearValues = _applyClearValues,       // TODO Gather clear values of all bound surfaces (value[0].DepthStencil is always for the depth-stencil clear value)
                    ClearValueCount = maxSurfaceCount,      // TODO Gather clear values of all bound surfaces
                    PNext = null
                };

                _device.VK.CmdBeginRenderPass(_cmd, rpInfo, SubpassContents.Inline);
                _device.VK.CmdBindPipeline(_cmd, PipelineBindPoint.Graphics, pipeState);

                _device.VK.CmdSetViewport(_cmd, 0, maxSurfaceCount, _applyViewports); // TODO collect viewport handles and set them all at once.
                _device.VK.CmdSetScissor(_cmd, 0, maxSurfaceCount, _applyScissors);

                _device.VK.CmdBindDescriptorSets(_cmd, PipelineBindPoint.Graphics, pipeState.Layout, 0, 1, _applyDescSets, 0, null);
                _device.VK.CmdBindVertexBuffers(_cmd, 0, 1, null, null);
                _device.VK.CmdBindIndexBuffer(_cmd, iBuffer, 0, iType);

                callback.Invoke();
                _device.VK.CmdEndRenderPass(_cmd);

                Profiler.DrawCalls++;
                EndEvent();
            }

            return GraphicsBindResult.Successful;
        }

        protected override GraphicsBindResult DoComputePass(HlslPass hlslPass)
        {
            ShaderPassVK pass = hlslPass as ShaderPassVK;
            Vector3UI groups = DrawInfo.Custom.ComputeGroups;

            if (groups.X == 0)
                groups.X = pass.ComputeGroups.X;

            if (groups.Y == 0)
                groups.Y = pass.ComputeGroups.Y;

            if (groups.Z == 0)
                groups.Z = pass.ComputeGroups.Z;

            DrawInfo.ComputeGroups = groups;

            GraphicsBindResult vResult = Validate(QueueValidationMode.Compute);
            if (vResult != GraphicsBindResult.Successful)
                return vResult;

            // Re-render the same pass for K iterations.
            // TODO Use sub-passes?
            for (int j = 0; j < pass.Iterations; j++)
            {
                BeginEvent($"Iteration {j}");
                PipelineStateVK pipeState = pass.State.GetState(null, null);
                uint descriptorSetCount = 0;

                _vk.CmdBindPipeline(_cmd, PipelineBindPoint.Compute, pipeState);
                //_vk.CmdBindDescriptorSets(_cmd, PipelineBindPoint.Compute, pipelineLayout, 0, descriptorSetCount, pDescriptorSets, 0, null);
                _vk.CmdDispatch(_cmd, groups.X, groups.Y, groups.Z);

                Profiler.DispatchCalls++;
                EndEvent();
            }

            pass.InvokeCompleted(DrawInfo.Custom);
            return GraphicsBindResult.Successful;
        }

        public override GraphicsBindResult Draw(HlslShader shader, uint vertexCount, uint vertexStartIndex = 0)
        {
            return ApplyState(shader, QueueValidationMode.Unindexed, () => 
                _vk.CmdDraw(_cmd, vertexCount, 1, vertexStartIndex, 0));
        }

        public override GraphicsBindResult DrawInstanced(HlslShader shader, uint vertexCountPerInstance, uint instanceCount, uint vertexStartIndex = 0, uint instanceStartIndex = 0)
        {
            return ApplyState(shader, QueueValidationMode.Instanced, () =>
                _vk.CmdDraw(_cmd, vertexCountPerInstance, instanceCount, vertexStartIndex, instanceStartIndex));
        }

        public override GraphicsBindResult DrawIndexed(HlslShader shader, uint indexCount, int vertexIndexOffset = 0, uint startIndex = 0)
        {
            return ApplyState(shader, QueueValidationMode.Indexed, () => 
                _vk.CmdDrawIndexed(_cmd, indexCount, 1, startIndex, vertexIndexOffset, 0));
        }

        public override GraphicsBindResult DrawIndexedInstanced(HlslShader shader, uint indexCountPerInstance, uint instanceCount, uint startIndex = 0, int vertexIndexOffset = 0, uint instanceStartIndex = 0)
        {
            return ApplyState(shader, QueueValidationMode.InstancedIndexed, () => 
                _vk.CmdDrawIndexed(_cmd, indexCountPerInstance, instanceCount, startIndex, vertexIndexOffset, instanceStartIndex));
        }

        public override GraphicsBindResult Dispatch(HlslShader shader, Vector3UI groups)
        {
            return ApplyState(shader, QueueValidationMode.Compute, null);
        }

        protected override GraphicsBindResult CheckInstancing()
        {
            throw new NotImplementedException();
        }

        internal Vk VK => _vk;

        internal Logger Log { get; }

        internal DeviceVK VKDevice => _device;

        /// <summary>
        /// Gets the Queue family index, in relation to the bound <see cref="DeviceVK"/>.
        /// </summary>
        internal uint FamilyIndex { get; }

        /// <summary>
        /// Gets the command queue index, within its family.
        /// </summary>
        internal uint Index { get; }

        /// <summary>
        /// Gets the underlying command set definition.
        /// </summary>
        internal SupportedCommandSet Set { get; }

        /// <summary>
        /// Gets flags representing the available API command sets.
        /// </summary>
        internal CommandSetCapabilityFlags Flags { get; }

        internal Queue Native { get; private set; }

        /// <summary>
        /// The current command list, if any.
        /// </summary>
        protected override GraphicsCommandList Cmd
        {
            get => _cmd;
            set => _cmd = value as CommandListVK;
        }
    }
}
