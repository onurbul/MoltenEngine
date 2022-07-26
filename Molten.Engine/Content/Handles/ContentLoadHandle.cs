﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public delegate void ContentLoadCallbackHandler<T>(T asset, bool isReload);

    public class ContentLoadHandle<T> : ContentHandle
    {
        internal ContentLoadCallbackHandler<T> _completionCallback;

        bool _canHotReload;
        bool _isLoaded;
        ContentWatcher _watcher;

        internal ContentLoadHandle(
            ContentManager manager, 
            string path,
            IContentProcessor processor, 
            IContentParameters parameters,
            ContentLoadCallbackHandler<T> completionCallback, 
            bool canHotReload) : 
            base(manager, path, typeof(T), processor, parameters, ContentHandleType.Load)
        {
            _completionCallback = completionCallback;
            _canHotReload = canHotReload;
            Asset = default(T);
        }

        protected override void OnComplete()
        {
            _completionCallback.Invoke((T)Asset, _isLoaded);
            _isLoaded = true;
            UpdateWatcher();
        }

        private void UpdateWatcher()
        {
            // Delete the watcher if we have one to prevent reloads.
            if (!_canHotReload)
            {
                if (_watcher != null)
                    Manager.StopWatching(_watcher, this);
            }
            else
            {
                if (_watcher == null)
                    _watcher = Manager.StartWatching(this);
            }
        }

        protected override bool OnProcess()
        {
            bool reload = Asset != null;

            if (reload)
            {
                Type assetType = Asset.GetType();
                object[] att = assetType.GetCustomAttributes(typeof(ContentReloadAttribute), true);
                if (att.Length > 0)
                {
                    // Can we reuse the existing asset, or reinstantiate it?
                    if (att[0] is ContentReloadAttribute attReload && attReload.Reinstantiate)
                    {
                        // Reinstantiating, so dispose of existing asset if possible.
                        if (Asset is IDisposable disposable)
                            disposable.Dispose();
                    }
                }
            }

            return Processor.Read(this, Asset, out Asset);
        }


        /// <summary>
        /// Gets the asset held by the current <see cref="ContentLoadHandle{T}"/>.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">If the asset has not been loaded yet.</exception>
        public T Get()
        {
            if (Status != ContentHandleStatus.Completed)
                throw new ContentNotLoadedException("Unable to retrieve asset that has not loaded yet.");

            return Asset != null ? (T)Asset : default(T);
        }

        public bool HasAsset()
        {
            return Asset != null;
        }

        /// <summary>
        /// Gets or sets whether the asset is allowed to hot-reload when changes occur to it's file.
        /// </summary>
        public bool CanHotReload
        {
            get => _canHotReload;
            set
            {
                if (_canHotReload != value)
                {
                    _canHotReload = value;
                    UpdateWatcher();
                }
            }
        }

        public static implicit operator T(ContentLoadHandle<T> handle)
        {
            return handle.Get();
        }
    }
}
