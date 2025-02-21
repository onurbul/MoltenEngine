﻿namespace Molten.Graphics
{
    public sealed class VertexFormat : EngineObject
    {
        internal VertexFormat(ShaderIOLayout structure, uint sizeOf)
        {
            Structure = structure;
            SizeOf = sizeOf;
        }

        protected unsafe override void OnDispose()
        {
            Structure.Dispose();
        }

        /// <summary>Gets the total size of the Vertex Format, in bytes.</summary>
        public uint SizeOf { get; private set; }

        public ShaderIOLayout Structure { get; }
    }
}
