﻿using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class SpriteText : IRenderable2D
    {
        public SpriteFont Font;
        public Vector2F Position;
        public Color Color = Color.White;
        public string Text = "";

        public void Render(SpriteBatch batch)
        {
            batch.DrawString(Font, Text, Position, Color);
        }
    }
}
