﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    /// <summary>
    /// Represents a line of text that is displayed by an implemented <see cref="UIElement"/>.
    /// </summary>
    public class UITextLine
    {
        int _height;

        internal UITextLine(UITextElement element)
        {
            Parent = element;
            HasText = false;
        }

        public void Clear()
        {
            Width = 0;
            _height = 0;
            HasText = false;
            First = new UITextSegment("", Color.White, null, UITextSegmentType.Text);
            Last = First;
        }

        public UITextSegment NewSegment(string text, Color color, SpriteFont font, UITextSegmentType type)
        {
            UITextSegment segment = new UITextSegment(text, color, font, type);
            AppendSegment(segment);
            return segment;
        }

        public void AppendSegment(UITextSegment seg)
        {
            if (seg != null)
            {
                if (Last != null)
                {
                    Last.Next = seg;
                    seg.Previous = Last;
                }

                Last = seg;
                Width += seg.Size.X;
                _height = Math.Max(_height, (int)Math.Ceiling(seg.Size.Y));
            }
        }

        /// <summary>
        /// Creates and inserts a new <see cref="UITextSegment"/> after the specified <see cref="UITextSegment"/>.
        /// </summary>
        /// <param name="seg">The <see cref="UITextSegment"/> that the new segment should be inserted after.
        /// <para>If null, the new segment will be insert at the beginning of the line.</para></param>
        /// <param name="color">The color of the new segment's text.</param>
        /// <param name="font">The font of the new segment.</param>
        /// <param name="type">The type of the new segment.</param>
        /// <returns></returns>
        public UITextSegment InsertSegment(UITextSegment seg, Color color, SpriteFont font, UITextSegmentType type)
        {
            UITextSegment next = new UITextSegment("", Color.White, font, type);

            // Do we need to insert before another "next" segment also?
            if(seg.Next != null)
            {
                seg.Next.Previous = next;
                next.Next = seg.Next;
            }

            if (seg != null)
            {
                seg.Next = next;
                next.Previous = seg;
            }

            Width += seg.Size.X;
            _height = Math.Max(_height, (int)Math.Ceiling(seg.Size.Y));

            return next;
        }

        /// <summary>
        /// Gets the measured width of the current <see cref="UITextLine"/>.
        /// </summary>
        public float Width { get; private set; }

        /// <summary>
        /// Gets the measured height of the current <see cref="UITextLine"/>.
        /// <para>If the line contains no text, the default line height of the <see cref="Parent"/> will be used instead.</para>
        /// </summary>
        public int Height => HasText ? _height : Parent.DefaultLineHeight;

        /// <summary>
        /// Gets the parent <see cref="UITextElement"/>, or null if none.
        /// </summary>
        public UITextElement Parent { get; internal set; }

        /// <summary>
        /// Gets the first <see cref="UITextSegment"/> on the current <see cref="UITextLine"/>.
        /// </summary>
        public UITextSegment First { get; private set; }

        /// <summary>
        /// Gets the last <see cref="UITextSegment"/> on the current <see cref="UITextLine"/>.
        /// </summary>
        public UITextSegment Last { get; private set; }

        /// <summary>
        /// Gets whether or not the current <see cref="UITextLine"/> contains text.
        /// </summary>
        public bool HasText { get; private set; }
    }
}
