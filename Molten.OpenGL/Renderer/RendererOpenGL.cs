﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Collections;
using OpenTK;

namespace Molten.Graphics
{
    public class RendererOpenGL : MoltenRenderer
    {
        ThreadedList<ISwapChainSurface> _outputSurfaces;
        RenderProfiler _profiler;
        DisplayManagerGL _displayManager;

        protected override void OnInitializeAdapter(GraphicsSettings settings)
        {
            NativeWindow dummyWindow = new NativeWindow();
            _displayManager = new DisplayManagerGL();
            _displayManager.Initialize(Log, settings);

            dummyWindow.Dispose();
        }

        protected override void OnInitialize(GraphicsSettings settings)
        {
            _profiler = new RenderProfiler();
            _outputSurfaces = new ThreadedList<ISwapChainSurface>();
        }

        protected override void OnDispose()
        {
            _displayManager.Dispose();
        }

        protected override SceneRenderData OnCreateRenderData()
        {
            throw new NotImplementedException();
        }

        protected override IRenderChain GetRenderChain()
        {
            throw new NotImplementedException();
        }

        protected override void OnPreRenderScene(SceneRenderData sceneData, Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPostRenderScene(SceneRenderData sceneData, Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPreRenderCamera(SceneRenderData sceneData, RenderCamera camera, Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPostRenderCamera(SceneRenderData sceneData, RenderCamera camera, Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnRebuildSurfaces(int requiredWidth, int requiredHeight)
        {
            throw new NotImplementedException();
        }

        protected override void OnPrePresent(Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPostPresent(Timing time)
        {
            throw new NotImplementedException();
        }

        public string Namer => null;

        public override IComputeManager Compute => null;

        public override string Name => "OpenGL";

        public override IDisplayManager DisplayManager => throw new NotImplementedException();

        public override IResourceManager Resources => throw new NotImplementedException();
    }
}
