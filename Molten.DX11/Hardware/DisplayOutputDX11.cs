﻿using Silk.NET.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public unsafe class DisplayOutputDX11 : GraphicsOutput
    {
        internal IDXGIOutput1 Native;
        OutputDesc _desc;
        DisplayAdapterDX11 _adapter;

        internal DisplayOutputDX11(DisplayAdapterDX11 adapter, ref IDXGIOutput1 output) : 
            base(adapter)
        {
            _adapter = adapter;
            Native = output;
            Native.GetDesc(ref _desc);

            fixed (char* d = _desc.DeviceName)
                Name = new string(d);

            Name = Name.Replace("\0", string.Empty);
        }

        public DisplayMode[] GetSupportedModes(Format format)
        {
            uint flags = DXGI.EnumModesInterlaced | DXGI.EnumModesScaling;
            uint* modeCount = (uint*)0;
            ModeDesc1* modeDescs = (ModeDesc1*)0;

            Native.GetDisplayModeList1(format, flags, modeCount, modeDescs);
            ModeDesc1[] m = new ModeDesc1[(int)modeCount];
            DisplayMode[] modes = new DisplayMode[m.Length];

            //build a list of all valid display modes
            for (int i = 0; i < m.Length; i++)
                modes[i] = new DisplayMode(ref m[i]);

            return modes;
        }

        /// <summary>Gets the resolution/size of the dekstop bound to the output, if any.</summary>
        public override Rectangle DesktopBounds => _desc.DesktopCoordinates.FromApi();

        /// <summary>Gets whether or not the output is bound to a desktop.</summary>
        public bool IsBoundToDesktop { get { return _desc.AttachedToDesktop > 0; } }

        /// <summary>Gets the orientation of the current <see cref="T:Molten.IDisplayOutput" />.</summary>
        public override DisplayOrientation Orientation => (DisplayOrientation)_desc.Rotation;
    }
}
