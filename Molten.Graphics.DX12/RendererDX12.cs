﻿using Molten.Graphics.Dxgi;
using Silk.NET.Direct3D12;

namespace Molten.Graphics
{
    public class RendererDX12 : RenderService
    {
        D3D12 _api;
        DisplayManagerDXGI _displayManager;

        public RendererDX12()
        {
            
        }

        protected override GraphicsDisplayManager OnInitializeDisplayManager(GraphicsSettings settings)
        {
            _api = D3D12.GetApi();
            Builder = new DeviceBuilderDX12(_api, this);
            _displayManager = new DisplayManagerDXGI(Builder.GetCapabilities);
            return _displayManager;
        }


        protected override GraphicsDevice OnCreateDevice(GraphicsSettings settings, GraphicsDisplayManager manager)
        {
            NativeDevice = new DeviceDX12(this, settings, Builder, _displayManager.SelectedAdapter);
            return NativeDevice;
        }

        protected override void OnInitializeRenderer(EngineSettings settings)
        {
            
        }

        protected override void OnPostPresent(Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPrePresent(Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnDisposeBeforeRender()
        {
            NativeDevice?.Dispose();
            _api.Dispose();
        }

        internal DeviceDX12 NativeDevice { get; private set; }

        internal DeviceBuilderDX12 Builder { get; private set; }

        public override DxcCompiler Compiler { get; }
    }
}
