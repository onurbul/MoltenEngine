﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Molten.Graphics
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SpriteSheet 
    {
        ITexture2D _texture;
        Dictionary<string, SpriteData> _sprites = new Dictionary<string, SpriteData>();

        public SpriteSheet()
        {
            Width = 0;
            Height = 0;
        }

        public SpriteSheet(ITexture2D texture)
        {
            SetTexture(texture);
        }

        public SpriteSheet(string texturePath)
        {
            TexturePath = texturePath;
        }

        /// <summary>
        /// Tries to load the texture at <see cref="TexturePath"/> via the provided <see cref="ContentManager"/>.
        /// </summary>
        /// <param name="content"></param>
        public void LoadTexture(ContentManager content, ContentRequestHandler onLoadedCallback = null, string rootDirectory = null)
        {
            ContentRequest request = content.BeginRequest(rootDirectory);
            request.Load<ITexture2D>(TexturePath);
            request.OnCompleted += Request_OnCompleted;

            if(onLoadedCallback != null)
                request.OnCompleted += onLoadedCallback;

            request.Commit();
        }

        private void Request_OnCompleted(ContentRequest request)
        {
            ITexture2D tex = request.Get<ITexture2D>(0);
            if (tex != null)
                SetTexture(tex);
        }

        private void SetTexture(ITexture2D texture)
        {
            if (texture == _texture)
                return;

            if (texture != null)
            {
                Width = _texture.Width;
                Height = _texture.Height;   
            }
            else
            {
                Width = 0;
                Height = 0;
            }

            _texture = texture;

            foreach (SpriteData data in _sprites.Values)
                data.Texture = _texture;
        }

        public bool TryGetData(string key, out SpriteData data)
        {
            return _sprites.TryGetValue(key, out data);
        }

        [JsonProperty]
        public string TexturePath { get; set; }

        public ITexture2D Texture { get; private set; }

        public uint Width { get; private set; }

        public uint Height { get; private set; }

        public SpriteData this[string key]
        {
            get => _sprites[key];
            set => _sprites[key] = value;
        }
    }
}
