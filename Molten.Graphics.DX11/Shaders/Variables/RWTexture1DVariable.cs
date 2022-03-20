﻿namespace Molten.Graphics
{
    internal class RWTexture1DVariable : RWVariable
    {
        Texture1D _texture;

        internal RWTexture1DVariable(HlslShader shader) : base(shader) { }

        protected override ContextBindableResource OnSetUnorderedResource(object value)
        {
            _texture = value as Texture1D;

            if (_texture != null)
            {
                if ((_texture.Flags & TextureFlags.AllowUAV) != TextureFlags.AllowUAV)
                    throw new InvalidOperationException("A texture cannot be passed to a RWTexture2D resource constant without .AllowUAV flags.");
            }

            return _texture;
        }
    }
}
