﻿namespace Molten.Graphics
{
    internal class BufferVariable : ShaderResourceVariable
    {
        BufferSegment _bufferSegment;

        internal BufferVariable(HlslShader shader) : base(shader) { }

        protected override ContextBindableResource OnSetResource(object value)
        {
            _bufferSegment = value as BufferSegment;
            return _bufferSegment;
        }
    }
}
