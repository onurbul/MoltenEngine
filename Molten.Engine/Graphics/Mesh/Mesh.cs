﻿namespace Molten.Graphics
{
    /// <summary>A base interface for mesh implementations.</summary>
    public abstract class Mesh : Renderable
    {
        GraphicsBuffer _iBuffer;

        /// <summary>
        /// Creates a new instance of <see cref="Mesh"/>, but can only be called by derived mesh classes.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="maxVertices"></param>
        /// <param name="mode"></param>
        /// <param name="maxIndices">The maximum number of indices to allow in the current <see cref="Mesh"/>.</param>
        /// <param name="initialIndices"></param>
        protected Mesh(RenderService renderer, GraphicsResourceFlags mode, ushort maxVertices, uint maxIndices, ushort[] initialIndices = null) :
            base(renderer)
        {
            IndexFormat = maxIndices > 0 ? GraphicsIndexFormat.UInt16 : GraphicsIndexFormat.None;
            MaxVertices = maxVertices;
            IsDiscard = mode.IsDiscard();

            if (IndexFormat != GraphicsIndexFormat.None)
            {
                _iBuffer = Renderer.Device.CreateIndexBuffer(mode, maxIndices, initialIndices);

                if (initialIndices != null)
                    IndexCount = (uint)initialIndices.Length;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="Mesh"/>, but can only be called by derived mesh classes.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="maxVertices"></param>
        /// <param name="mode"></param>
        /// <param name="maxIndices">The maximum number of indices to allow in the current <see cref="Mesh"/>.</param>
        /// <param name="initialIndices"></param>
        protected Mesh(RenderService renderer, GraphicsResourceFlags mode, uint maxVertices, uint maxIndices, uint[] initialIndices = null) :
            base(renderer)
        {
            IndexFormat = maxIndices > 0 ? GraphicsIndexFormat.UInt32 : GraphicsIndexFormat.None;
            MaxVertices = maxVertices;
            MaxIndices = maxIndices;

            if (IndexFormat != GraphicsIndexFormat.None)
            {
                _iBuffer = Renderer.Device.CreateIndexBuffer(mode, maxIndices, initialIndices);

                if (initialIndices != null)
                    IndexCount = (uint)initialIndices.Length;
            }
        }

        public void SetIndices<I>(I[] data) where I : unmanaged
        {
            SetIndices(data, 0, (uint)data.Length);
        }

        public void SetIndices<I>(I[] data, uint count) where I : unmanaged
        {
            SetIndices(data, 0, count);
        }

        public void SetIndices<I>(I[] data, uint startIndex, uint count) where I : unmanaged
        {
            if (_iBuffer == null)
                throw new InvalidOperationException($"Mesh is not indexed. Must be created with index format that isn't IndexBufferFormat.None.");

            IndexCount = count;
            _iBuffer.SetData(GraphicsPriority.Apply, data, startIndex, count, IsDiscard, 0);
        }

        protected virtual void OnApply(GraphicsQueue queue)
        {
            queue.State.IndexBuffer.Value = _iBuffer;
        }

        protected virtual void OnPostDraw(GraphicsQueue queue)
        {
            queue.State.IndexBuffer.Value = null;
        }

        protected virtual void OnDraw(GraphicsQueue queue)
        {
            if(_iBuffer != null)
                queue.DrawIndexed(Shader, IndexCount);
            else
                queue.Draw(Shader, VertexCount);
        }

        protected override sealed void OnRender(GraphicsQueue queue, RenderService renderer, RenderCamera camera, ObjectRenderData data)
        {
            if (Shader == null)
                return;

            OnApply(queue);
            ApplyResources(Shader);
            Shader.Object.Wvp.Value = Matrix4F.Multiply(data.RenderTransform, camera.ViewProjection);
            Shader.Object.World.Value = data.RenderTransform;
            OnDraw(queue);
            OnPostDraw(queue);
        }

        public virtual void Dispose()
        {
            IsVisible = false;
            _iBuffer.Dispose();
        }

        /// <summary>
        /// Gets whether the mesh is dynamic. 
        /// <para>Dynamic meshes are more performant at handling frequent data changes/updates.</para> 
        /// <para>Static meshes are preferred when their data will not change too often.</para>
        /// </summary>
        public bool IsDynamic { get; }

        /// <summary>Gets the maximum number of vertices the mesh can contain.</summary>
        public uint MaxVertices { get; }

        /// <summary>Gets the number of vertices stored in the mesh.</summary>
        public uint VertexCount { get; set; }

        public uint MaxIndices { get; set; }

        public uint IndexCount { get; set; }

        public GraphicsIndexFormat IndexFormat { get; }

        /// <summary>
        /// Gets or sets the material that should be used when rendering the current <see cref="Mesh"/>.
        /// </summary>
        public HlslShader Shader { get; set; }

        public float EmissivePower { get; set; } = 1.0f;

        protected bool IsDiscard { get; }
    }

    public class Mesh<T> : Mesh
        where T : unmanaged, IVertexType
    {
        GraphicsBuffer _vb;

        internal Mesh(RenderService renderer, 
            GraphicsResourceFlags mode, ushort maxVertices, uint maxIndices,
            T[] initialVertices = null, ushort[] initialIndices = null) :
            base(renderer, mode, maxVertices, maxIndices, initialIndices)
        {
            _vb = renderer.Device.CreateVertexBuffer(mode, maxVertices, initialVertices);

            if (initialVertices != null)
                VertexCount = (uint)initialVertices.Length;
        }

        internal Mesh(RenderService renderer,
             GraphicsResourceFlags mode, uint maxVertices, uint maxIndices,
             T[] initialVertices = null, uint[] initialIndices = null) :
             base(renderer, mode, maxVertices, maxIndices, initialIndices)
        {
            _vb = renderer.Device.CreateVertexBuffer(mode, maxVertices, initialVertices); 
            
            if (initialVertices != null)
                VertexCount = (uint)initialVertices.Length;
        }

        public void SetVertices(T[] data)
        {
            SetVertices(data, 0, (uint)data.Length);
        }

        public void SetVertices(T[] data, uint count)
        {
            SetVertices(data, 0, count);
        }

        public void SetVertices(T[] data, uint startIndex, uint count)
        {
            VertexCount = count;
            _vb.SetData(GraphicsPriority.Apply, data, startIndex, count, IsDiscard, 0);
        }

        protected override void OnApply(GraphicsQueue queue)
        {
            base.OnApply(queue);
            queue.State.VertexBuffers[0] = _vb;
        }

        protected override void OnPostDraw(GraphicsQueue cmd)
        {
            base.OnPostDraw(cmd);
            cmd.State.VertexBuffers[0] = null;
        }

        public override void Dispose()
        {
            base.Dispose();
            _vb.Dispose();
        }
    }
}
