﻿using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class StartStep : RenderStepBase
    {
        RenderSurface2D _surfaceScene;
        RenderSurface2D _surfaceNormals;
        RenderSurface2D _surfaceEmissive;
        DepthStencilSurface _surfaceDepth;

        internal override void Initialize(RendererDX11 renderer)
        {
            _surfaceScene = renderer.GetSurface<RenderSurface2D>(MainSurfaceType.Scene);
            _surfaceNormals = renderer.GetSurface<RenderSurface2D>(MainSurfaceType.Normals);
            _surfaceEmissive = renderer.GetSurface<RenderSurface2D>(MainSurfaceType.Emissive);
            _surfaceDepth = renderer.GetDepthSurface();
        }

        public override void Dispose() { }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, RenderChain.Context context, Timing time)
        {
            Device device = renderer.Device;

            device.State.SetRenderSurfaces(null);
            bool newSurface = renderer.ClearIfFirstUse(device, _surfaceScene, context.Scene.BackgroundColor);
            renderer.ClearIfFirstUse(device, _surfaceNormals, Color.White * 0.5f);
            renderer.ClearIfFirstUse(device, _surfaceEmissive, Color.Black);

            // Always clear the depth surface at the start of each scene unless otherwise instructed.
            // Will also be cleared if we've just switched to a previously un-rendered surface during this frame.
            if(!camera.Flags.HasFlag(RenderCameraFlags.DoNotClearDepth) || newSurface)
                _surfaceDepth.Clear(device, ClearFlag.ClearDepth | ClearFlag.ClearStencil);
        }
    }
}
