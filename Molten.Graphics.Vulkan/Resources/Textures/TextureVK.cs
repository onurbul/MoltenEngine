﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public unsafe abstract class TextureVK : GraphicsTexture
    {
        ImageCreateInfo _desc;
        Image* _native;
        ImageView* _view;
        ResourceHandleVK* _handle;
        MemoryAllocationVK _allocation;

        public TextureVK(GraphicsDevice device, 
            uint width, uint height, uint depth, 
            uint mipCount, uint arraySize, 
            AntiAliasLevel aaLevel,
            MSAAQuality sampleQuality, 
            GraphicsFormat format, 
            GraphicsResourceFlags flags, bool allowMipMapGen, string name) : 
            base(device, width, height, depth, mipCount, arraySize, aaLevel, sampleQuality, format, flags, allowMipMapGen, name)
        {
            _native = EngineUtil.Alloc<Image>();
            _view = EngineUtil.Alloc<ImageView>();

            CreateImage();
        }

        protected void CreateImage()
        {
            DeviceVK device = Device as DeviceVK;

            ImageUsageFlags flags = ImageUsageFlags.None;
            if (Flags.Has(GraphicsResourceFlags.GpuRead))
                flags |= ImageUsageFlags.TransferSrcBit;

            if(Flags.Has(GraphicsResourceFlags.GpuWrite))
                flags |= ImageUsageFlags.TransferDstBit;

            if (Flags.Has(GraphicsResourceFlags.UnorderedAccess))
                flags |= ImageUsageFlags.StorageBit;

            if (!Flags.Has(GraphicsResourceFlags.NoShaderAccess))
                flags |= ImageUsageFlags.SampledBit;

            _desc = new ImageCreateInfo(StructureType.ImageCreateInfo);
            _desc.Extent.Width = Width;
            _desc.Extent.Height = Height;
            _desc.Extent.Depth = Depth;
            _desc.MipLevels = MipMapCount;
            _desc.ArrayLayers = ArraySize;
            _desc.Format = ResourceFormat.ToApi();
            _desc.Tiling = ImageTiling.Optimal;
            _desc.InitialLayout = ImageLayout.Undefined;
            _desc.Usage = flags;
            _desc.SharingMode = SharingMode.Exclusive;
            _desc.Samples = SampleCountFlags.Count1Bit;
            _desc.Flags = ImageCreateFlags.None;

            // Queue properties are ignored if sharing mode is not VK_SHARING_MODE_CONCURRENT.
            if (_desc.SharingMode == SharingMode.Concurrent)
            {
                _desc.PQueueFamilyIndices = EngineUtil.AllocArray<uint>(1);
                _desc.PQueueFamilyIndices[0] = (Device.Queue as GraphicsQueueVK).Index;
                _desc.QueueFamilyIndexCount = 1;
            }

            ImageViewCreateInfo viewInfo = new ImageViewCreateInfo(StructureType.ImageViewCreateInfo);
            viewInfo.Format = _desc.Format;
            viewInfo.SubresourceRange.AspectMask = ImageAspectFlags.ColorBit;
            viewInfo.SubresourceRange.BaseMipLevel = 0;
            viewInfo.SubresourceRange.LevelCount = MipMapCount;
            viewInfo.SubresourceRange.BaseArrayLayer = 0;
            viewInfo.SubresourceRange.LayerCount = ArraySize;
            viewInfo.Flags = ImageViewCreateFlags.None;

            SetCreateInfo(ref _desc, ref viewInfo);

            // Creation of images with tiling VK_IMAGE_TILING_LINEAR may not be supported unless other parameters meet all of the constraints
            if (_desc.Tiling == ImageTiling.Linear)
            {
                //if (this is DepthSurfaceVK depthSurface)
                //    throw new GraphicsResourceException(this, "A depth surface texture cannot use linear tiling mode");

                if (_desc.ImageType != ImageType.ImageType2D)
                    throw new GraphicsResourceException(this, "A non-2D texture cannot use linear tiling mode");

                if(_desc.MipLevels != 1)
                    throw new GraphicsResourceException(this, "Texture linear-tiled texture must have only 1 mip-map level.");

                if(_desc.ArrayLayers != 1)
                    throw new GraphicsResourceException(this, "Texture linear-tiled texture must have only 1 array layer.");

                if (_desc.Samples != SampleCountFlags.Count1Bit)
                    throw new GraphicsResourceException(this, "Texture linear-tiled texture must have a sample count of 1.");

                if(_desc.Usage > (ImageUsageFlags.TransferSrcBit | ImageUsageFlags.TransferDstBit))
                    throw new GraphicsResourceException(this, "A linear-tiled texture must have only source and/or destination transfer bits set. Any other usage flags are invalid.")
            }

            if (_desc.ImageType == 0)
                throw new GraphicsResourceException(this, "Image type not set during image creation");

            if (viewInfo.ViewType == 0)
                throw new GraphicsResourceException(this, "View type not set during image-view creation");

            Result r = device.VK.CreateImage(device, _desc, null, _native);
            if (r.Check(device, () => "Failed to create image resource"))
                return;

            r = device.VK.CreateImageView(device, &viewInfo, null, _view);
            if (r.Check(device, () => "Failed to create image view"))
                return;
        }

        protected abstract void SetCreateInfo(ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo);

        private void DestroyResources()
        {
            DeviceVK device = Device as DeviceVK;
            if (_view != null)
                device.VK.DestroyImageView(device, *_view, null);
            if (_native != null)
                device.VK.DestroyImage(device, *_native, null);
        }

        public override void GraphicsRelease()
        {
            DestroyResources();
            EngineUtil.Free(ref _view);
            EngineUtil.Free(ref _native);
        }

        protected override void OnSetSize()
        {
            throw new NotImplementedException();
        }

        public override unsafe void* Handle => _native;

        public override unsafe void* SRV => _view;

        public override unsafe void* UAV => throw new NotImplementedException();
    }
}
