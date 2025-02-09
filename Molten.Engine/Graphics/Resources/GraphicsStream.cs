﻿using Molten.IO;

namespace Molten.Graphics
{
    /// <summary>
    /// A graphics-specific implementation of <see cref="RawStream"/> intended for use with mapped <see cref="GraphicsResource"/> instances.
    /// </summary>
    public class GraphicsStream : RawStream
    {
        public unsafe GraphicsStream(GraphicsQueue queue, GraphicsResource resource, ref ResourceMap map) : 
            base(map.Ptr, 
                map.DepthPitch, 
                resource.Flags.Has(GraphicsResourceFlags.CpuRead), 
                resource.Flags.Has(GraphicsResourceFlags.CpuWrite))
        {
            Map = map;
            Queue = queue;
            Resource = resource;
        }

        protected override void Dispose(bool disposing)
        {
            Queue.UnmapResource(Resource);
            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets the <see cref="GraphicsQueue"/> that the current <see cref="GraphicsStream"/> is bound to.
        /// </summary>
        public GraphicsQueue Queue { get; }

        /// <summary>
        /// Gets the <see cref="GraphicsResource"/> that the current <see cref="GraphicsStream"/> points to.
        /// </summary>
        public GraphicsResource Resource { get; }

        /// <summary>
        /// Gets the index of the sub-resource of <see cref="Resource"/> that the current <see cref="GraphicsStream"/> points to.
        /// </summary>
        public uint SubResourceIndex { get; }

        /// <summary>
        /// Gets the number of bytes in a single row of the mapped resource. E.g. a row of pixels in an uncompressed texture, or a row of blocks in a compressed texture.
        /// </summary>
        public ResourceMap Map { get; }
    }
}
