﻿using Molten.Graphics;
using Molten.Threading;

namespace Molten.Input
{
    public class AndroidInputService : InputService
    {
        AndroidInputNavigation _navigation;

        public override IClipboard Clipboard => throw new NotImplementedException();

        public override IInputNavigation Navigation => _navigation;

        protected override ThreadingMode OnInitialize(EngineSettings settings)
        {
            ThreadingMode mode = base.OnInitialize(settings);

            _navigation = new AndroidInputNavigation();

            return mode;
        }

        protected override void OnBindSurface(INativeSurface surface)
        {
            _navigation.SetSurface(surface);
        }

        protected override GamepadDevice OnGetGamepad(int index, GamepadSubType subtype)
        {
            throw new NotImplementedException();
        }

        public override KeyboardDevice GetKeyboard()
        {
            throw new NotImplementedException();
        }

        public override MouseDevice GetMouse()
        {
            throw new NotImplementedException();
        }

        public override TouchDevice GetTouch()
        {
            return GetCustomDevice<AndroidTouchDevice>();
        }

        /// <summary>Update's the current input manager. Avoid calling directly unless you know what you're doing.</summary>
        /// <param name="time">An instance of timing for the current thread.</param>
        protected override void OnUpdate(Timing time)
        {
            _navigation.Update(time);

            base.OnUpdate(time);
        }

        protected override void OnClearState()
        {
            _navigation.ClearState();
        }
    }
}
