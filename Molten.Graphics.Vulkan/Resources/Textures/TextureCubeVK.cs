﻿using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public class TextureCubeVK : Texture2DVK, ITextureCube
    {
        public TextureCubeVK(GraphicsDevice device, uint width, uint height, uint mipCount, uint arraySize, uint cubeCount, 
            GraphicsFormat format, GraphicsResourceFlags flags, bool allowMipMapGen, string name) : 
            base(device, GraphicsTextureType.TextureCube, width, height, mipCount, arraySize, 
                AntiAliasLevel.None, 
                MSAAQuality.Default, 
                format, 
                flags, 
                allowMipMapGen, 
                name)
        {
            CubeCount = cubeCount;
        }

        protected override void SetCreateInfo(DeviceVK device, ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo)
        {
            base.SetCreateInfo(device, ref imgInfo, ref  viewInfo);

            imgInfo.Flags |= ImageCreateFlags.CreateCubeCompatibleBit;
            imgInfo.ArrayLayers *= 6;

            viewInfo.ViewType = CubeCount == 1 ? ImageViewType.TypeCube : ImageViewType.TypeCubeArray;
        }

        public uint CubeCount { get; }
    }
}
