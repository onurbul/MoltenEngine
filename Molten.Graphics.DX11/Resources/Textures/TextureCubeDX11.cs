﻿using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX11
{
    public unsafe class TextureCubeDX11 : Texture2DDX11, ITextureCube
    {
        internal TextureCubeDX11(GraphicsDevice device, uint width, uint height, GraphicsResourceFlags flags, 
            GraphicsFormat format = GraphicsFormat.R8G8B8A8_UNorm, uint mipCount = 1, uint cubeCount = 1,
            bool allowMipMapGen = false, string name = null)
            : base(device, width, height, flags, format, mipCount, 6 * cubeCount, AntiAliasLevel.None, MSAAQuality.Default, allowMipMapGen, name)
        {
            CubeCount = cubeCount;

            _desc = new Texture2DDesc1()
            {
                Width = width,
                Height = height,
                MipLevels = mipCount,
                ArraySize = ArraySize,
                Format = format.ToApi(),
                BindFlags = (uint)(flags.Has(GraphicsResourceFlags.NoShaderAccess) ? BindFlag.None : BindFlag.ShaderResource),
                CPUAccessFlags = (uint)Flags.ToCpuFlags(),
                SampleDesc = new SampleDesc(1, 0),
                Usage = Flags.ToUsageFlags(),
                MiscFlags = (uint)(Flags.ToMiscFlags(allowMipMapGen) | ResourceMiscFlag.Texturecube),
                TextureLayout = 0U, // TextureLayout.None
            };
        }

        protected override void SetSRVDescription(ref ShaderResourceViewDesc1 desc)
        {
            desc.Format = DxgiFormat;
            desc.ViewDimension = D3DSrvDimension.D3DSrvDimensionTexturecubearray;
            desc.TextureCubeArray = new TexcubeArraySrv()
            {
                MostDetailedMip = 0,
                MipLevels = _desc.MipLevels,
                NumCubes = CubeCount,
                First2DArrayFace = 0,
            };
        }

        protected override void SetUAVDescription(ref ShaderResourceViewDesc1 srvDesc, ref UnorderedAccessViewDesc1 desc)
        {
            desc.Format = srvDesc.Format;
            desc.ViewDimension = UavDimension.Texture2Darray;

            desc.Texture2DArray = new Tex2DArrayUav1()
            {
                ArraySize = _desc.ArraySize,
                FirstArraySlice = srvDesc.Texture2DArray.FirstArraySlice,
                MipSlice = 0,
                PlaneSlice = 0
            };

            desc.Buffer = new BufferUav()
            {
                FirstElement = 0,
                NumElements = _desc.Width * _desc.Height * _desc.ArraySize,
            };
        }

        protected override void UpdateDescription(uint newWidth, uint newHeight, uint newDepth, uint newMipMapCount, uint newArraySize, Format newFormat)
        {
            _desc.Width = newWidth;
            _desc.Height = newHeight;
            _desc.MipLevels = newMipMapCount;
            _desc.Format = newFormat;
        }

        public void Resize(GraphicsPriority priority, uint newWidth, uint newHeight, uint newMipMapCount)
        {
            QueueTask(priority, new TextureResizeTask()
            {
                NewWidth = newWidth,
                NewHeight = newHeight,
                NewMipMapCount = newMipMapCount,
            });
        }

        public void Resize(GraphicsPriority priority, uint newWidth, uint newMipMapCount)
        {
            QueueTask(priority, new TextureResizeTask()
            {
                NewWidth = newWidth,
                NewHeight = Height,
                NewMipMapCount = newMipMapCount,
            });
        }

        /// <summary>Gets information about the texture.</summary>
        internal ref Texture2DDesc1 Description => ref _desc;

        /// <summary>Gets the number of cube maps stored in the texture. This is greater than 1 if the texture is a cube-map array.</summary>
        public uint CubeCount { get; private set; }
    }
}
