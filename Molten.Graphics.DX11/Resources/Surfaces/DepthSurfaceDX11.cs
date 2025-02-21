﻿using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX11
{
    /// <summary>A special kind of render surface for use as a depth-stencil buffer.</summary>
    public unsafe class DepthSurfaceDX11 : Texture2DDX11, IDepthStencilSurface
    {
        ID3D11DepthStencilView* _depthView;
        ID3D11DepthStencilView* _readOnlyView;
        DepthStencilViewDesc _depthDesc;
        DepthFormat _depthFormat;
        ViewportF _vp;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="format"></param>
        /// <param name="mipCount"></param>
        /// <param name="arraySize"></param>
        /// <param name="aaLevel"></param>
        /// <param name="msaa"></param>
        /// <param name="flags">Texture flags</param>
        internal DepthSurfaceDX11(GraphicsDevice device,
            uint width, 
            uint height,
            GraphicsResourceFlags flags = GraphicsResourceFlags.GpuWrite,
            DepthFormat format = DepthFormat.R24G8_Typeless,
            uint mipCount = 1, 
            uint arraySize = 1, 
            AntiAliasLevel aaLevel = AntiAliasLevel.None,
            MSAAQuality msaa = MSAAQuality.Default,
            bool allowMipMapGen = false,
            string name = "surface")
            : base(device, width, height, flags, format.ToGraphicsFormat(), mipCount, arraySize, aaLevel, msaa, allowMipMapGen, name)
        {
            _depthFormat = format;
            Desc.ArraySize = arraySize;
            Desc.Format = format.ToGraphicsFormat().ToApi();
            _depthDesc = new DepthStencilViewDesc();
            _depthDesc.Format = format.ToDepthViewFormat().ToApi();

            name ??= "surface";
            Name = $"depth_{name}";

            if (MultiSampleLevel >= AntiAliasLevel.X2)
            {
                _depthDesc.ViewDimension = DsvDimension.Texture2Dmsarray;
                _depthDesc.Flags = 0U; // DsvFlag.None;
                _depthDesc.Texture2DMSArray = new Tex2DmsArrayDsv()
                {
                    ArraySize = Desc.ArraySize,
                    FirstArraySlice = 0,
                };
            }
            else
            {
                _depthDesc.ViewDimension = DsvDimension.Texture2Darray;
                _depthDesc.Flags = 0U; //DsvFlag.None;
                _depthDesc.Texture2DArray = new Tex2DArrayDsv()
                {
                    ArraySize = Desc.ArraySize,
                    FirstArraySlice = 0,
                    MipSlice = 0,
                };
            }

            UpdateViewport();
        }

        protected override void SetSRVDescription(ref ShaderResourceViewDesc1 desc)
        {
            base.SetSRVDescription(ref desc);

            switch (_depthFormat)
            {
                default:
                case DepthFormat.R24G8_Typeless:
                    desc.Format = Format.FormatR24UnormX8Typeless;
                    break;

                case DepthFormat.R32_Typeless:
                    desc.Format = Format.FormatR32Float;
                    break;
            }
        }

        private void UpdateViewport()
        {
            _vp = new ViewportF(0, 0, Desc.Width, Desc.Height);
        }

        private DsvFlag GetReadOnlyFlags()
        {
            switch (_depthFormat)
            {
                default:
                case DepthFormat.R24G8_Typeless:
                    return DsvFlag.Depth | DsvFlag.Stencil;
                case DepthFormat.R32_Typeless:
                    return DsvFlag.Depth;
            }
        }

        protected override void CreateTexture(DeviceDX11 device, ResourceHandleDX11<ID3D11Resource> handle, uint handleIndex)
        {
            SilkUtil.ReleasePtr(ref _depthView);
            SilkUtil.ReleasePtr(ref _readOnlyView);

            Desc.Width = Math.Max(1, Desc.Width);
            Desc.Height = Math.Max(1, Desc.Height);

            // Create render target texture
            base.CreateTexture(device, handle, handleIndex);

            _depthDesc.Flags = 0; // DsvFlag.None;
            SubresourceData* subData = null;
            ID3D11Resource* res = handle.NativePtr;

            fixed(DepthStencilViewDesc* pDesc = &_depthDesc)
                device.Ptr->CreateDepthStencilView(res, pDesc, ref _depthView);

            // Create read-only depth view for passing to shaders.
            _depthDesc.Flags = (uint)GetReadOnlyFlags();
            fixed (DepthStencilViewDesc* pDesc = &_depthDesc)
                device.Ptr->CreateDepthStencilView(res, pDesc, ref _readOnlyView);
            _depthDesc.Flags = 0U; // (uint)DsvFlag.None;
        }

        protected override void UpdateDescription(TextureDimensions dimensions, GraphicsFormat newFormat)
        {
            base.UpdateDescription(dimensions, newFormat);
            UpdateViewport();
        }

        internal void OnClear(GraphicsQueueDX11 cmd, ref DepthClearTask task)
        {
            cmd.Ptr->ClearDepthStencilView(_depthView, (uint)task.Flags, task.DepthClearValue, task.StencilClearValue);
        }

        public void Clear(GraphicsPriority priority, DepthClearFlags flags, float depth = 1.0f, byte stencil = 0)
        {
            QueueTask(priority, new DepthClearTask()
            {
                Flags = flags,
                Surface = this,
                DepthClearValue = depth,
                StencilClearValue = stencil
            });
        }

        protected override void OnGraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref _depthView);
            SilkUtil.ReleasePtr(ref _readOnlyView);

            base.OnGraphicsRelease();
        }

        /// <summary>Gets the DepthStencilView instance associated with this surface.</summary>
        internal ID3D11DepthStencilView* DepthView => _depthView;

        /// <summary>Gets the read-only DepthStencilView instance associated with this surface.</summary>
        internal ID3D11DepthStencilView* ReadOnlyView => _readOnlyView;

        /// <summary>Gets the depth-specific format of the surface.</summary>
        public DepthFormat DepthFormat => _depthFormat;

        /// <summary>Gets the viewport of the <see cref="DepthSurfaceDX11"/>.</summary>
        public ViewportF Viewport => _vp;
    }
}
