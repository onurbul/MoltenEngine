﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11
{
    public abstract unsafe class ResourceHandleDX11 : GraphicsResourceHandle
    {
        public static implicit operator ID3D11Resource*(ResourceHandleDX11 handle)
        {
            return (ID3D11Resource*)handle.Ptr;
        }

        internal ResourceHandleDX11(GraphicsResource resource) : base(resource)
        {
            Device = resource.Device as DeviceDX11;
            SRV = new SRViewDX11(this);
            UAV = new UAViewDX11(this);
        }

        public override void Dispose()
        {
            SRV.Release();
            UAV.Release();
        }

        internal SRViewDX11 SRV { get; }

        internal UAViewDX11 UAV { get; }

        internal DeviceDX11 Device { get; }
    }

    public unsafe class ResourceHandleDX11<T> : ResourceHandleDX11
        where T : unmanaged
    {
        T* _ptr;

        internal ResourceHandleDX11(GraphicsResource resource) :
            base(resource)
        { }

        public override void Dispose()
        {
            SilkUtil.ReleasePtr(ref _ptr);
        }

        public static implicit operator T*(ResourceHandleDX11<T> handle)
        {
            return handle._ptr;
        }

        public override unsafe void* Ptr => _ptr;

        internal ref T* NativePtr => ref _ptr;
    }
}
