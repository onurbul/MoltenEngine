﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Silk.NET.Core;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Queue = Silk.NET.Vulkan.Queue;

namespace Molten.Graphics
{
    internal unsafe class DeviceVK : ExtensionManager<Device>
    {
        Instance* _vkInstance;
        RendererVK _renderer;
        CommandSetCapabilityFlags _cap;
        long _allocatedVRAM;
        List<CommandQueueVK> _queues;
        CommandQueueVK _gfxQueue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="adapter"></param>
        /// <param name="instance"></param>
        /// <param name="requiredCap">Required capabilities</param>
        internal DeviceVK(RendererVK renderer, DisplayAdapterVK adapter, Instance* instance, CommandSetCapabilityFlags requiredCap) : 
            base(renderer)
        {
            _queues = new List<CommandQueueVK>();
            _renderer = renderer;
            _vkInstance = instance;
            Adapter = adapter;
            _cap = requiredCap;
        }

        protected override unsafe Result GetLayers(uint* count, LayerProperties* items)
        {
            return Renderer.VK.EnumerateDeviceLayerProperties(Adapter.Native, count, items);
        }

        protected override unsafe Result GetExtensions(uint* count, ExtensionProperties* items)
        {
            byte* nullptr = null;
            return Renderer.VK.EnumerateDeviceExtensionProperties(Adapter.Native, nullptr, count, items);
        }

        /// <summary>
        /// Finds a <see cref="CommandQueueVK"/> that can present the provided <see cref="WindowSurfaceVK"/>.
        /// </summary>
        /// <param name="surface"></param>
        /// <returns></returns>
        internal CommandQueueVK FindPresentQueue(WindowSurfaceVK surface)
        {
            KhrSurface extSurface = _renderer.Instance.GetExtension<KhrSurface>();
            Bool32 presentSupported = false;

            foreach (CommandQueueVK queue in _queues)
            {
                Result r = extSurface.GetPhysicalDeviceSurfaceSupport(Adapter.Native, queue.FamilyIndex, surface.Native, &presentSupported);
                if (_renderer.CheckResult(r) && presentSupported)
                    return queue;
            }

            return null;
        }

        /// <summary>
        /// Retrieves the <see cref="SharingMode"/> for a resource, based on <see cref="CommandQueueVK"/> queues that may potentionally access it.
        /// </summary>
        /// <param name="expectedQueues">The <see cref="CommandQueueVK"/> queues that are expected to share.</param>
        /// <returns></returns>
        internal (SharingMode, CommandQueueVK[]) GetSharingMode(params CommandQueueVK[] expectedQueues)
        {
            HashSet<CommandQueueVK> set = new HashSet<CommandQueueVK>();
            for (int i = 0; i < expectedQueues.Length; i++)
                set.Add(expectedQueues[i]);

            if (set.Count <= 1)
                return (SharingMode.Exclusive, set.ToArray());
            else
                return (SharingMode.Concurrent, set.ToArray());
        }

        protected override bool LoadExtension(RendererVK renderer, VulkanExtension ext, Device* obj)
        {
            return ext.Load(renderer, _vkInstance, obj);
        }

        protected unsafe override void DestroyObject(RendererVK renderer, Device* obj)
        {
            renderer.VK.DestroyDevice(*obj, null);
        }

        protected override unsafe Result OnBuild(RendererVK renderer, VersionVK apiVersion, TempData tmp, ExtensionBinding binding, Device* obj)
        {
            List<SupportedCommandSet> sets = Adapter.Capabilities.CommandSets;
            DeviceQueueCreateInfo* queueInfo = EngineUtil.AllocArray<DeviceQueueCreateInfo>((uint)sets.Count);

            CommandSetCapabilityFlags[] values = Enum.GetValues<CommandSetCapabilityFlags>();
            CommandSetCapabilityFlags capActive = CommandSetCapabilityFlags.None;

            uint queueCount = 0;
            float queuePriority = 1.0f;

            // Iterate over all available command set/queue familes to find the requested capabilities (_cap).
            for (int i = 0; i < sets.Count; i++)
            {
                SupportedCommandSet set = sets[i];

                // Use the queue if it has at least one of the requested capabilities.
                foreach(CommandSetCapabilityFlags flag in values)
                {
                    if (flag == CommandSetCapabilityFlags.None)
                        continue;

                    if((_cap & flag) == flag && (set.CapabilityFlags & flag) == flag)
                    {
                        queueInfo[queueCount++] = new DeviceQueueCreateInfo(StructureType.DeviceQueueCreateInfo)
                        {
                            Flags = DeviceQueueCreateFlags.None,
                            QueueCount = 1,
                            QueueFamilyIndex = (uint)i,
                            PQueuePriorities = &queuePriority
                        };

                        capActive |= set.CapabilityFlags;

                        break;
                    }
                }
            }

            Result r = Result.Success;

            // Check if all requested capabilities were available.
            if ((capActive & _cap) != _cap)
            {
                r = Result.ErrorInitializationFailed;
                _renderer.Log.Error($"Not all requested capabilities were supported. Requested: '{_cap}' -- Found: '{capActive}'");
            }
            else
            {
                DeviceCreateInfo createInfo = new DeviceCreateInfo()
                {
                    SType = StructureType.DeviceCreateInfo,
                    QueueCreateInfoCount = queueCount,
                    PQueueCreateInfos = queueInfo,
                    EnabledLayerCount = (uint)binding.Layers.Count,
                    PpEnabledLayerNames = tmp.LayerNames,
                    EnabledExtensionCount = (uint)binding.Extensions.Count,
                    PpEnabledExtensionNames = tmp.ExtensionNames
                };

                // Create the instance
                r = renderer.VK.CreateDevice(Adapter, &createInfo, null, obj);
                if (renderer.CheckResult(r, () => $"Failed to create logical device on '{Adapter.Name}' adapter"))
                {
                    for (int i = 0; i < queueCount; i++)
                    {
                        ref DeviceQueueCreateInfo qi = ref queueInfo[i];

                        for (uint index = 0; index < qi.QueueCount; index++)
                        {
                            Queue q = new Queue();
                            _renderer.VK.GetDeviceQueue(*obj, qi.QueueFamilyIndex, index, &q);
                            SupportedCommandSet set = sets[(int)qi.QueueFamilyIndex];
                            CommandQueueVK queue = new CommandQueueVK(_renderer, this, qi.QueueFamilyIndex, q, index, set);
                            _queues.Add(queue);

                            // TODO maybe find the best queue, rather than first match?
                            if (_gfxQueue == null && queue.HasFlags(CommandSetCapabilityFlags.Graphics))
                                _gfxQueue = queue;

                            _renderer.Log.Write($"Instantiated command queue -- Family: {qi.QueueFamilyIndex} -- Index: {index} -- Flags: {set.CapabilityFlags}");
                        }
                    }
                }
            }

            EngineUtil.Free(ref queueInfo);
            return r;
        }

        /// <summary>Track a VRAM allocation.</summary>
        /// <param name="bytes">The number of bytes that were allocated.</param>
        internal void AllocateVRAM(long bytes)
        {
            Interlocked.Add(ref _allocatedVRAM, bytes);
        }

        /// <summary>Track a VRAM deallocation.</summary>
        /// <param name="bytes">The number of bytes that were deallocated.</param>
        internal void DeallocateVRAM(long bytes)
        {
            Interlocked.Add(ref _allocatedVRAM, -bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Device(DeviceVK device)
        {
            return *device.Ptr;
        }

        /// <summary>
        /// Gets the underlying <see cref="DisplayAdapterVK"/> that the current <see cref="DeviceVK"/> is bound to.
        /// </summary>
        internal DisplayAdapterVK Adapter { get; }

        /// <summary>
        /// Gets the <see cref="Instance"/> that the current <see cref="DeviceVK"/> is bound to.
        /// </summary>
        internal Instance* Instance => _vkInstance;

        /// <summary>
        /// Gets the underlying <see cref="CommandQueueVK"/> that should execute graphics commands.
        /// </summary>
        internal CommandQueueVK GraphicsQueue => _gfxQueue;

        internal long AllocatedVRAM => _allocatedVRAM;
    }
}
