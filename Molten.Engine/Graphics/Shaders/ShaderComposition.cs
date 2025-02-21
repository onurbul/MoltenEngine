﻿namespace Molten.Graphics
{
    public unsafe class ShaderComposition : GraphicsObject
    {
        /// <summary>A list of const buffers the shader stage requires to be bound.</summary>
        public List<uint> ConstBufferIds = new List<uint>();

        /// <summary>A list of resources that must be bound to the shader stage.</summary>
        public List<uint> ResourceIds = new List<uint>();

        /// <summary>A list of samplers that must be bound to the shader stage.</summary>
        public List<uint> SamplerIds = new List<uint>();

        public List<uint> UnorderedAccessIds = new List<uint>();

        public ShaderIOLayout InputLayout;

        public ShaderIOLayout OutputLayout;

        public string EntryPoint { get; internal set; }

        public ShaderType Type { get; internal set; }

        void* _ptrShader;

        internal ShaderComposition(HlslPass parentPass, ShaderType type) : 
            base(parentPass.Device)
        {
            Pass = parentPass;
            Type = type;
        }

        protected override void OnGraphicsRelease() { }

        public void* PtrShader
        {
            get => _ptrShader;
            internal set => _ptrShader = value;
        }

        public HlslPass Pass { get; }
    }
}
