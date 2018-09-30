﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal abstract class PipelineBindSlot : PipelineBindSlotBase
    {
        internal PipelineBindSlot(PipelineComponent parent, int slotID) : base(slotID)
        {
            Parent = parent;
        }

        public PipelineComponent Parent { get; private set; }
    }

    internal class PipelineBindSlot<T> : PipelineBindSlot where T : PipelineObject
    {
        internal PipelineBindSlot(PipelineComponent parent, int slotID) :
            base(parent, slotID)
        { }

        protected override void UnbindDisposedObject(PipelineObjectBase obj)
        {
            BoundObject = null;
        }

        protected override void OnForceUnbind()
        {
            BoundObject?.Unbind(this);
            BoundObject = null;
        }

        /// <summary>Binds a new object to the slot. If null, the existing object (if any) will be unbound.</summary>
        /// <param name="pipe">The <see cref="GraphicsPipe"/> to use to perform any binding operations..</param>
        /// <param name="obj">The <see cref="PipelineObject"/> to be bound to the object, or null to clear the existing one.</param>
        /// <returns></returns>
        internal bool Bind(GraphicsPipe pipe, T obj, PipelineBindType bindType)
        {
            if(obj != null)
            {
                obj.Refresh(pipe, this);
                if (BoundObject == obj && BindType == bindType)
                    return false;

                BoundObject?.Unbind(this);
                BoundObject = obj;
                BindType = bindType;
                obj.Bind(this);
            }
            else
            {
                if (BoundObject == null && BindType == bindType)
                    return false;

                BoundObject?.Unbind(this);
                BoundObject = obj;
                BindType = bindType;
            }            

            // Return true to signal a difference between old and new object.
            pipe.Profiler.Current.Bindings++;
            return true;
        }

        internal T BoundObject { get; private set; }

        internal override PipelineObjectBase Object => BoundObject;
    }
}
