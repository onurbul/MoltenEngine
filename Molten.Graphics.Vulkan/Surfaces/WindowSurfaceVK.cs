﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.GLFW;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Molten.Graphics
{
    internal unsafe class WindowSurfaceVK : NativeObjectVK<SurfaceKHR>, INativeSurface
    {
        public event WindowSurfaceHandler OnHandleChanged;
        public event WindowSurfaceHandler OnParentChanged;
        public event WindowSurfaceHandler OnClose;
        public event WindowSurfaceHandler OnMinimize;
        public event WindowSurfaceHandler OnRestore;
        public event WindowSurfaceHandler OnFocusGained;
        public event WindowSurfaceHandler OnFocusLost;
        public event TextureHandler OnResize;

        CommandQueueVK _presentQueue;
        WindowHandle* _window;
        PresentModeKHR _mode;
        SurfaceCapabilitiesKHR _cap;
        SwapchainKHR _swapChain;

        string _title;
        uint _width;
        uint _height;
        uint _backBufferSize;
        ColorSpaceKHR _colorSpace = ColorSpaceKHR.SpaceSrgbNonlinearKhr;

        internal unsafe WindowSurfaceVK(DeviceVK device, GraphicsFormat format, string title, uint width, uint height,
            PresentModeKHR presentMode = PresentModeKHR.MailboxKhr)
        {
            DataFormat = format;
            Device = device;
            RendererVK renderer = Device.Renderer;
            _title = title;
            _width = width;
            _height = height;

            KhrSurface extSurface = renderer.Instance.GetExtension<KhrSurface>();
            if (extSurface == null)
            {
                renderer.Log.Error($"VK_KHR_surface extension is unsupported. Unable to initialize WindowSurfaceVK");
                return;
            }

            KhrSwapchain extSwapChain = Device.GetExtension<KhrSwapchain>();
            if (extSwapChain == null)
            {
                renderer.Log.Error($"VK_KHR_swapchain extension is unsupported. Unable to initialize WindowSurfaceVK");
                return;
            }

            renderer.GLFW.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);
            renderer.GLFW.WindowHint(WindowHintBool.Resizable, true);
            _window = renderer.GLFW.CreateWindow((int)width, (int)height, _title, null, null);

            VkHandle instanceHandle = new VkHandle(renderer.Instance.Ptr->Handle);
            VkNonDispatchableHandle surfaceHandle = new VkNonDispatchableHandle();

            Result r = (Result)renderer.GLFW.CreateWindowSurface(instanceHandle, _window, null, &surfaceHandle);
            if (!renderer.CheckResult(r))
                return;

            Native = new SurfaceKHR(surfaceHandle.Handle);
            _presentQueue = renderer.Device.FindPresentQueue(this);

            if (_presentQueue == null)
            {
                renderer.Log.Error($"No command queue found to present window surface");
                return;
            }

            // Check surface capabilities
            r = extSurface.GetPhysicalDeviceSurfaceCapabilities(device.Adapter, Native, out _cap);
            if (!renderer.CheckResult(r))
                return;

            if (!IsFormatSupported(extSurface, format, _colorSpace))
            {
                renderer.Log.Error($"Surface format '{format}' with a '{_colorSpace}' color-space is not supported");
                return;
            }

            _mode = ValidatePresentMode(extSurface, presentMode);
            ValidateSize();
            ValidateBackBufferSize();

            r = CreateSwapChain(extSwapChain);
            renderer.CheckResult(r);
        }

        private Result CreateSwapChain(KhrSwapchain extSwapChain)
        {
            SwapchainCreateInfoKHR createInfo = new SwapchainCreateInfoKHR()
            {
                SType = StructureType.SwapchainCreateInfoKhr,
                Surface = Native,
                MinImageCount = _backBufferSize,
                ImageFormat = DataFormat.ToApi(),
                ImageColorSpace = _colorSpace,
                ImageExtent = new Extent2D(_width, _height),
                ImageArrayLayers = 1,
                ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            };

            // Detect swap-chain sharing mode.
            (createInfo.ImageSharingMode, CommandQueueVK[] sharingWith) = Device.GetSharingMode(Device.GraphicsQueue, _presentQueue);
            uint* familyIndices = stackalloc uint[sharingWith.Length];

            for (int i = 0; i < sharingWith.Length; i++)
                familyIndices[i] = sharingWith[i].FamilyIndex;

            createInfo.QueueFamilyIndexCount = (uint)sharingWith.Length;
            createInfo.PQueueFamilyIndices = familyIndices;
            createInfo.PreTransform = _cap.CurrentTransform;
            createInfo.CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr;
            createInfo.PresentMode = _mode;
            createInfo.Clipped = true;
            createInfo.OldSwapchain = _swapChain;

            return extSwapChain.CreateSwapchain(*Device.Ptr, &createInfo, null, out _swapChain);
        }

        private bool IsFormatSupported(KhrSurface extSurface, GraphicsFormat format, ColorSpaceKHR colorSpace)
        {
            SurfaceFormatKHR[] supportedFormats = Device.Renderer.Enumerate<SurfaceFormatKHR>((count, items) =>
            {
                return extSurface.GetPhysicalDeviceSurfaceFormats(Device.Adapter, Native, count, items);
            }, "surface format");

            Format vkFormat = format.ToApi();

            for(int i = 0; i < supportedFormats.Length; i++)
            {
                if (supportedFormats[i].Format == vkFormat && supportedFormats[i].ColorSpace == colorSpace)
                    return true;
            }

            return false;
        }

        private PresentModeKHR ValidatePresentMode(KhrSurface extSurface, PresentModeKHR requested)
        {
            PresentModeKHR[] supportedModes = Device.Renderer.Enumerate<PresentModeKHR>((count, items) =>
            {
                return extSurface.GetPhysicalDeviceSurfacePresentModes(Device.Adapter, Native, count, items);
            }, "present mode");

            for (int i = 0; i < supportedModes.Length; i++)
            {
                if (supportedModes[i] == requested)
                    return requested;
            }

            return PresentModeKHR.FifoKhr;
        }

        private void ValidateBackBufferSize()
        {
            BackBufferMode mode = Device.Renderer.Settings.Graphics.BackBufferSize;
            if (mode == BackBufferMode.Default)
                _backBufferSize = _cap.MinImageCount + 1;
            else
                _backBufferSize = (uint)mode;

            if (_cap.MaxImageCount > 0)
                _backBufferSize = uint.Clamp(_backBufferSize, _cap.MinImageCount, _cap.MaxImageCount);
            else
                _backBufferSize = uint.Max(_cap.MinImageCount, _backBufferSize);
        }

        private void ValidateSize()
        {
            _width = uint.Clamp(_width, _cap.MinImageExtent.Width, _cap.MaxImageExtent.Width);
            _height = uint.Clamp(_height, _cap.MinImageExtent.Height, _cap.MaxImageExtent.Height);
        }

        protected override void OnDispose()
        {
            if (_swapChain.Handle != 0)
            {
                KhrSwapchain extSwapchain = Device.GetExtension<KhrSwapchain>();
                extSwapchain?.DestroySwapchain(*Device.Ptr, _swapChain, null);
            }

            KhrSurface extSurface = Device.Renderer.Instance.GetExtension<KhrSurface>();
            extSurface?.DestroySurface(*Device.Renderer.Instance.Ptr, Native, null);

            if (_window != null)
                Device.Renderer.GLFW.DestroyWindow(_window);
        }

        public void Present()
        {
            throw new NotImplementedException();
        }

        public void Dispatch(Action callback)
        {
            throw new NotImplementedException();
        }

        public Texture2DProperties Get2DProperties()
        {
            throw new NotImplementedException();
        }

        public void Resize(uint newWidth, uint newHeight, uint newMipMapCount = 0, uint newArraySize = 0, GraphicsFormat newFormat = GraphicsFormat.Unknown)
        {
            throw new NotImplementedException();
        }

        public Texture1DProperties Get1DProperties()
        {
            throw new NotImplementedException();
        }

        public void Resize(uint newWidth, uint newMipMapCount, GraphicsFormat format)
        {
            throw new NotImplementedException();
        }

        public void Resize(uint newWidth)
        {
            throw new NotImplementedException();
        }

        public void GenerateMipMaps()
        {
            throw new NotImplementedException();
        }

        public void CopyTo(ITexture destination)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(uint sourceLevel, uint sourceSlice, ITexture destination, uint destLevel, uint destSlice)
        {
            throw new NotImplementedException();
        }

        public void SetData(TextureData data, uint srcMipIndex, uint srcArraySlice, uint mipCount, uint arrayCount, uint destMipIndex = 0, uint destArraySlice = 0)
        {
            throw new NotImplementedException();
        }

        public void SetData<T>(uint level, T[] data, uint startIndex, uint count, uint pitch, uint arraySlice = 0) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public void SetData(TextureSlice data, uint mipLevel, uint arraySlice)
        {
            throw new NotImplementedException();
        }

        public void SetData<T>(RectangleUI area, T[] data, uint bytesPerPixel, uint level, uint arrayIndex = 0) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public void GetData(ITexture stagingTexture, Action<TextureData> callback)
        {
            throw new NotImplementedException();
        }

        public void GetData(ITexture stagingTexture, uint level, uint arrayIndex, Action<TextureSlice> callback)
        {
            throw new NotImplementedException();
        }

        public bool HasFlags(TextureFlags flags)
        {
            throw new NotImplementedException();
        }

        public void Clear(Color color)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            if(_window != null)
                Device.Renderer.GLFW.SetWindowShouldClose(_window, true);
        }

        /// <summary>
        /// Gets the <see cref="RendererVK"/> instance that the current <see cref="WindowSurfaceVK"/> is bound to.
        /// </summary>
        public DeviceVK Device { get; }

        public bool IsFocused => throw new NotImplementedException();

        public WindowMode Mode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public nint Handle => (nint)_window;

        public nint? ParentHandle { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public nint? WindowHandle => throw new NotImplementedException();

        public ViewportF Viewport => throw new NotImplementedException();

        public TextureFlags Flags => throw new NotImplementedException();

        public GraphicsFormat DataFormat { get; }

        public bool IsBlockCompressed => throw new NotImplementedException();

        public uint Width => _width;

        public uint Height => _height;

        public uint MipMapCount => throw new NotImplementedException();

        public uint ArraySize => throw new NotImplementedException();

        public AntiAliasLevel MultiSampleLevel => throw new NotImplementedException();

        public bool IsMultisampled => throw new NotImplementedException();

        RenderService ITexture.Renderer => throw new NotImplementedException();

        public Rectangle RenderBounds => throw new NotImplementedException();

        /// <inheritdoc/>
        public string Title
        {
            get => _title;
            set
            {
                if (_window != null)
                    Device.Renderer.GLFW.SetWindowTitle(_window, _title);
            }
        }
        public bool IsVisible { get; set; }
    }
}
