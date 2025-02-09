﻿namespace Molten.Graphics
{
    internal struct BufferSetTask<T> : IGraphicsResourceTask
        where T : unmanaged
    {
        /// <summary>The number of bytes to offset the change, from the start of the provided <see cref="Segment"/>.</summary>
        internal uint ByteOffset;

        /// <summary>The number of elements to be copied.</summary>
        internal uint ElementCount;

        internal GraphicsMapType MapType;

        internal uint DataStartIndex;

        /// <summary>The data to be set.</summary>
        internal T[] Data;

        internal GraphicsBuffer DestBuffer;

        internal Action CompletionCallback;

        public bool Process(GraphicsQueue cmd, GraphicsResource resource)
        {
            if (resource.Flags.Has(GraphicsResourceFlags.CpuWrite))
            {
                using (GraphicsStream stream = cmd.MapResource(resource, 0, ByteOffset, MapType))
                    stream.WriteRange(Data, DataStartIndex, ElementCount);
            }
            else
            {
                GraphicsBuffer staging = cmd.Device.Frame.StagingBuffer;
                using (GraphicsStream stream = cmd.MapResource(staging, 0, ByteOffset, GraphicsMapType.Write))
                    stream.WriteRange(Data, DataStartIndex, ElementCount);

                cmd.CopyResource(staging, resource);
            }

            CompletionCallback?.Invoke();
            return false;
        }
    }
}
