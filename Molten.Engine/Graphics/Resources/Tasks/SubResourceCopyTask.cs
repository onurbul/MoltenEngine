﻿namespace Molten.Graphics
{
    public struct SubResourceCopyTask : IGraphicsResourceTask
    {
        public ResourceRegion? SrcRegion;

        public uint SrcSubResource;

        /// <summary>The start offset within the resource.
        /// <para>For a buffer, only the X dimension needs to be set equal to the number of bytes to offset.</para>
        /// <para>For textures, this will vary depending on the number of texture dimensions.</para></summary>
        public Vector3UI DestStart;

        public GraphicsResource DestResource;

        public uint DestSubResource;

        public Action<GraphicsResource> CompletionCallback;

        public unsafe bool Process(GraphicsQueue cmd, GraphicsResource resource)
        {
            if (DestResource.Flags.Has(GraphicsResourceFlags.GpuWrite))
                throw new ResourceCopyException(resource, DestResource, "The destination resource must have GPU write access for writing the copied data.");

            if (resource is GraphicsBuffer buffer && buffer.BufferType == GraphicsBufferType.Staging)
                resource.Ensure(cmd);

            if (SrcRegion.HasValue)
            {
                ResourceRegion region = SrcRegion.Value;
                cmd.CopyResourceRegion(resource, SrcSubResource, &region, DestResource, DestSubResource, DestStart);
            }
            else
            {
                cmd.CopyResourceRegion(resource, SrcSubResource, null, DestResource, DestSubResource, DestStart);
            }

            cmd.Profiler.SubResourceCopyCalls++;
            CompletionCallback?.Invoke(resource);

            return false;
        }
    }
}
