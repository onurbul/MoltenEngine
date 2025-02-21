﻿using System.Runtime.Serialization;
using Molten.Graphics;

namespace Molten
{
    [DataContract]
    public class GraphicsSettings : SettingBank
    {
        /// <summary>
        /// The minimum tessellation factor for batched lights.
        /// </summary>
        public const float MIN_LIGHT_TESS_FACTOR = 4.0f;

        /// <summary>
        /// The maximum tessellation factor for batched lights.
        /// </summary>
        public const float MAX_LIGHT_TESS_FACTOR = 8.0f;

        public const float LIGHT_TESS_FACTOR_RANGE = MAX_LIGHT_TESS_FACTOR - MIN_LIGHT_TESS_FACTOR;

        public GraphicsSettings()
        {
            AdapterID = AddSetting("adapter_id", new DeviceID());
            DisplayOutputIds = AddSettingList<int>("display_id");
            VSync = AddSetting("vsync", true);
            MSAA = AddSetting<AntiAliasLevel>("msaa", 0);
            FrameBufferMode = AddSetting("back_buffer_size", Graphics.FrameBufferMode.Double);
            EnableDebugLayer = AddSetting<bool>("renderer_debug");
            TargetFPS = AddSetting<int>("target_fps", 60);
        }

        /// <summary>
        /// Gets <see cref="FrameBufferMode"/> as a <see cref="uint"/> value.
        /// </summary>
        /// <returns></returns>
        public uint GetBackBufferSize()
        {
            uint backBufferSize = 1;
            if (FrameBufferMode.Value != Graphics.FrameBufferMode.Default)
                backBufferSize = (uint)FrameBufferMode.Value;

            return backBufferSize;
        }

        /// <summary>Gets or sets the UID of the <see cref="GraphicsDevice"/> that was last used.</summary>
        [DataMember]
        public SettingValue<DeviceID> AdapterID { get; }

        /// <summary>Gets or sets the UID of the <see cref="IDisplayOutput"/> that was last used on the last used <see cref="AdapterID"/>.</summary>
        [DataMember]
        public SettingValueList<int> DisplayOutputIds { get; }

        /// <summary>Gets or sets whether Vsync is enabled.</summary>
        [DataMember]
        public SettingValue<bool> VSync { get; }

        /// <summary>
        /// Gets or sets the target frame rate of render services, if available.
        /// </summary>
        [DataMember]
        public SettingValue<int> TargetFPS { get; }

        /// <summary>Gets or sets the multi-sampled anti-aliasing (MSAA) level.</summary>
        [DataMember]
        public SettingValue<AntiAliasLevel> MSAA { get; }

        /// <summary>Gets or sets the number of back-buffer surfaces. A larger back-buffer tends to increase performance, but also consumes more video memory.</summary>
        [DataMember]
        public SettingValue<FrameBufferMode> FrameBufferMode { get; }

        /// <summary>Gets or sets whether to enable a renderer's debug layer, if available.</summary>
        [DataMember]
        public SettingValue<bool> EnableDebugLayer { get; }

        /// <summary>
        /// Gets or sets the size of per-frame staging buffer segments, in megabytes. 
        /// <para>Smaller segment sizes lead to a larger number of segments per frame, which can reduce VRAM consumption at the expense of performance.
        /// In contrast, larger segment sizes lead to smaller numbers of segments per frame, 
        /// which can improve performance at the expense of potentially higher VRAM consumption.</para>
        /// </summary>
        [DataMember]
        public double FrameStagingSize { get; set; } = 5.5;
    }
}
