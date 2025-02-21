﻿using Molten.Collections;

namespace Molten.Graphics
{
    internal class RenderChainLink : IPoolable
    {
        List<RenderChainLink> _previous = new List<RenderChainLink>();
        List<RenderChainLink> _next = new List<RenderChainLink>();
        RenderStep _step;
        RenderChain _chain;
        bool _completed;

        internal RenderChainLink(RenderChain chain)
        {
            _chain = chain;
        }

        public void ClearForPool()
        {
            _previous.Clear();
            _next.Clear();
            _step = null;
            _completed = false;
        }

        internal RenderChainLink Set<T>() where T : RenderStep, new()
        {
            _step = _chain.GetRenderStep<T>();
            return this;
        }

        internal RenderChainLink Next<T>() where T : RenderStep, new()
        {
            RenderChainLink next = _chain.LinkPool.GetInstance();
            next.Set<T>();
            _next.Add(next);
            next._previous.Add(this);

            return next;
        }

        /// <summary>
        /// Causes all of the current link's next links toconverge on the specified step type.
        /// </summary>
        /// <typeparam name="T">The type of step to converge at.</typeparam>
        /// <returns>The latest <see cref="RenderChainLink"/> that was added as a result of the requested step being added.</returns>
        internal RenderChainLink ConvergeAt<T>() where T : RenderStep, new()
        {
            RenderChainLink next = _chain.LinkPool.GetInstance();
            next.Set<T>();
            ConvergeAt(this, next);
            return next;
        }

        private static void ConvergeAt(RenderChainLink current, RenderChainLink target)
        {
            if (current._next.Count > 0)
            {
                for (int i = 0; i < current._next.Count; i++)
                    ConvergeAt(current._next[i], target);
            }
            else
            {
                current._next.Add(target);
                target._previous.Add(current);
            }
        }

        internal static void Recycle(RenderChainLink link)
        {
            for (int i = 0; i < link._next.Count; i++)
                Recycle(link._next[i]);

            link._chain.LinkPool.Recycle(link);
        }

        internal void Run(GraphicsQueue queue, RenderCamera camera, RenderChainContext context, Timing time)
        {
            bool canStart = true;

            // Are the previous steps completed?
            do
            {
                canStart = true;
                for (int i = 0; i < _previous.Count; i++)
                    canStart = canStart && _previous[i]._completed;

                // TODO do other things while the thread is waiting for the previous steps to complete?
            } while (canStart == false);

            // TODO update this once the renderer supports running render steps in deferred context threads.
            // TODO also consider spawning extra chain contexts so they can individually 
            queue.BeginEvent($"Step {_step.GetType().Name}");
            _step.Render(queue, camera, context, time);
            _completed = true;
            queue.EndEvent();

            // Push the current step's commands to the GPU.
            queue.Sync();

            // Start the next steps
            for (int i = 0; i < _next.Count; i++)
                _next[i].Run(queue, camera, context, time);
        }
    }
}
