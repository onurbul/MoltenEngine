﻿namespace Molten.Graphics
{
    internal abstract class ContextStateProvider : EngineObject
    {
        internal ContextStateProvider(DeviceContextState parent) { }

        /// <summary>
        /// Called when the current <see cref="ContextStateProvider"/> is to be bound to it's parent <see cref="DeviceContext"/>
        /// </summary>
        internal abstract void Bind(DeviceContextState state, DeviceContext context);
    }
}
