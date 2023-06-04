﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public class Texture3DVK : TextureVK, ITexture3D
    {
        public Texture3DVK(GraphicsDevice device, 
            TextureDimensions dimensions, GraphicsFormat format, 
            GraphicsResourceFlags flags, bool allowMipMapGen, string name, bool isSurface = false) : 
            base(device, 
                isSurface ? GraphicsTextureType.Texture3D : GraphicsTextureType.Surface3D, 
                dimensions, 
                AntiAliasLevel.None, 
                MSAAQuality.Default, 
                format, 
                flags, 
                allowMipMapGen, 
                name)
        {
        }

        protected override void SetCreateInfo(DeviceVK device, ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo)
        {
            imgInfo.ImageType = ImageType.Type3D;
            viewInfo.ViewType = ImageViewType.Type3D;
        }
    }
}
