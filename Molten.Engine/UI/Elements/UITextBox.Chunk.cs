﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Collections;
using Molten.Graphics;

namespace Molten.UI
{
    public partial class UITextBox
    {
        const int CHUNK_CAPACITY = 128;

        private class Chunk
        {
            internal ThreadedList<Line> Lines = new ThreadedList<Line>(CHUNK_CAPACITY);
            int _width;
            int _height;
            int _startLineNumber;

            public Chunk(int firstLineNumber)
            {
                _startLineNumber = firstLineNumber;
            }

            private void FastAppendLine(Line line)
            {
                Lines.Add(line);
                _width = Math.Max(_width, (int)Math.Ceiling(line.Width));
                _height += line.Height; 

                if (Next != null)
                    Next.StartLineNumber++;
            }

            private void FastInsertLine(Line line, int index)
            {
                Lines.Insert(index, line);
                _width = Math.Max(_width, (int)Math.Ceiling(line.Width));
                _height += line.Height;

                if (Next != null)
                    Next.StartLineNumber++;
            }

            internal Chunk AppendLine(Line line)
            {
                if(Lines.Count < CHUNK_CAPACITY)
                {
                    FastAppendLine(line);
                }
                else
                {
                    if (Next == null || Next.Capacity == 0)
                        NewNext();

                    Next.FastInsertLine(line, 0);
                    return Next;
                }

                return this;
            }

            internal Chunk InsertLine(Line line, int index)
            {
                if (Lines.Count < CHUNK_CAPACITY)
                {
                    FastInsertLine(line, index);
                }
                else
                {
                    if (index == 0)
                    {
                        if (Previous == null || Previous.Capacity == 0)
                            NewPrevious();

                        Previous.FastAppendLine(line);
                        return Previous;
                    }
                    else if (index == CHUNK_CAPACITY - 1)
                    {
                        if (Next == null || Next.Capacity == 0)
                            NewNext();

                        // Directly insert line to avoid duplicated checks
                        Next.FastInsertLine(line, 0);
                        return Next;
                    }
                    else
                    {
                        Split(index);
                        FastAppendLine(line);
                    }
                }

                return this;
            }

            /// <summary>
            /// Splits the current <see cref="Chunk"/>, moving all items from at and beyond the given index, into a new <see cref="Chunk"/>.
            /// </summary>
            /// <param name="splitIndex">All lines at and beyond the current index are cut off into a new chunk, added after the current one.</param>
            private void Split(int splitIndex)
            {
                int nextCount = Lines.Count - splitIndex;
                if (Next == null || Next.Capacity < nextCount)
                    NewNext();

                Next.Lines.AddRange(Lines, splitIndex, nextCount);
                Lines.RemoveRange(splitIndex, nextCount);

                CalculateSize();
                Next.CalculateSize();
            }

            internal void CalculateSize()
            {
                _width = 0;
                _height = 0;
                Line line = null;

                for (int i = Lines.Count - 1; i >= 0; i--)
                {
                    line = Lines[i];
                    _width = Math.Max(_width, (int)Math.Ceiling(line.Width));
                    _height += line.Height;
                }
            }

            private void NewPrevious()
            {
                Chunk prev = new Chunk(StartLineNumber-1);

                if (Previous != null)
                {
                    Previous.Next = prev;
                    prev.Previous = Previous;
                }

                Previous = prev;
                Previous.Next = this;
            }

            private void NewNext()
            {
                Chunk next = new Chunk(StartLineNumber + Lines.Count);

                // Update the current "Next".
                if (Next != null)
                {
                    Next.Previous = next;
                    next.Next = Next;
                }

                // Update the new "Next".
                Next = next;
                Next.Previous = this;
            }

            internal (Line line, Segment seg, RectangleF segBounds, Rectangle lineBounds) Pick(Vector2I pos, ref Rectangle bounds)
            {
                Rectangle lBounds = bounds;

                if (bounds.Contains(pos))
                {
                    Line l = null;
                    for(int i = Lines.Count - 1; i >= 0; i--)
                    {
                        l = Lines[i];
                        lBounds.Height = l.Height;
                        lBounds.Y += l.Height;

                        if (lBounds.Contains(pos))
                        {
                            Segment seg = l.First;
                            RectangleF segBounds = lBounds;

                            while(seg != null)
                            {
                                segBounds.Width = seg.Size.X;
                                if (segBounds.Contains(pos))
                                    break;

                                segBounds.X += seg.Size.X;
                                seg = seg.Next;
                            }

                            return (l, seg, segBounds, lBounds);
                        }
                    }
                }

                return (null, null, RectangleF.Empty, Rectangle.Empty);
            }

            public void Render(SpriteBatcher sb, ref Rectangle bounds)
            {
                RectangleF rBounds = bounds;
                Line line = null;
                Segment seg = null;

                for(int i = 0; i < Lines.Count; i++)
                {
                    line = Lines[i];
                    seg = line.First;

                    while(seg != null)
                    {
                        rBounds.Width = seg.Size.X;
                        rBounds.Height = seg.Size.Y;

                        seg.Render(sb, line.Parent, ref rBounds);

                        rBounds.X += seg.Size.X;
                        seg = seg.Next;
                    }

                    rBounds.X = bounds.X;
                    rBounds.Y += line.Height;
                }
            }

            internal Chunk Previous { get; set; }
            
            internal Chunk Next { get; set; }

            internal int StartLineNumber
            {
                get => _startLineNumber;
                set
                {
                    if(_startLineNumber != value)
                    {
                        _startLineNumber = value;
                        if (Next != null)
                            Next.StartLineNumber = StartLineNumber + Lines.Count;
                    }
                }
            }

            public int Capacity => CHUNK_CAPACITY - Lines.Count;

            public int Width => _width;

            public int Height => _height;
        }
    }
}
