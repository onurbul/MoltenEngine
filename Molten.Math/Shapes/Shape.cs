﻿using Molten.DoublePrecision;

namespace Molten.Shapes
{
    public partial class Shape
    {
        /// <summary>
        /// A list of shapes that make up the current <see cref="Shape"/>.
        /// </summary>
        public List<Contour> Contours { get; } = new List<Contour>();

        /// <summary>
        /// Creates a new instance of <see cref="Shape"/>.
        /// </summary>
        public Shape() { }

        /// <summary>
        /// Creates a new instance of <see cref="Shape"/> from a list of linear points.
        /// </summary>
        /// <param name="points"></param>
        public Shape(List<Vector2F> points) : this(points, Vector2F.Zero, 1f) { }

        /// <summary>
        /// Creates a new instance of <see cref="Shape"/> from a list of linear points.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="offset"></param>
        /// <param name="scale"></param>
        public Shape(List<Vector2F> points, Vector2F offset, float scale = 1.0f)
        {
            Contour c = new Contour();
            Contours.Add(c);
            c.Add((Vector2D)points[0], (Vector2D)points[1]);
            for (int i = 2; i < points.Count; i++)
                c.Append((Vector2D)(offset + (points[i] * scale)), EdgeColor.White);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        public void Triangulate(List<Vector2F> output)
        {
            List<Triangle> triangles = new List<Triangle>();
            Triangulate(triangles);

            foreach (Triangle tri in triangles)
            {
                output.Add((Vector2F)tri.Points[0]);
                output.Add((Vector2F)tri.Points[2]);
                output.Add((Vector2F)tri.Points[1]);
            }
        }

        /// <summary>Triangulates the area inside the edge perimeter of the current <see cref="Shape"/>.</summary>
        /// <param name="output">A list to output the <see cref="Triangle"/> ojects which make up the interior mesh.</param>   
        internal void Triangulate(List<Triangle> output)
        {
            // Group contours
            List<(Contour c, List<TriPoint> edgeList)> holes =  new List<(Contour c, List<TriPoint> edgeList)> ();
            List<(Contour c, List<TriPoint> edgeList)> outlines = new List<(Contour c, List<TriPoint> edgeList)>();
            Sweep sweep = new Sweep();

            // Group contours into outlines and holes
            foreach (Contour c in Contours)
            {
                List<TriPoint> points = c.GetEdgePoints();

                // Check start/end points
                if (points.Count > 2)
                {
                    TriPoint p0 = points[0];
                    TriPoint p1 = points[points.Count - 1];
                    TriPoint p2 = points[points.Count - 2];
                    if (p1 == p2)
                        points.Remove(p2);

                    if (p1 == p0)
                        points.Remove(p1);
                }

                int winding = c.GetWinding();
                switch (winding)
                {
                    case 0: // Unknown
                        continue;

                    case -1: // Outline
                        outlines.Add((c, points));
                        break;

                    case 1: // Hole
                        points.Reverse();
                        holes.Add((c, points));
                        break;
                };
            }

            foreach((Contour c, List<TriPoint> edgePoints) in outlines)
            {
                sweep.Reset();
                SweepContext tcx = new SweepContext();
                tcx.AddPoints(edgePoints);

                // Add all holes to context
                foreach ((Contour h, List<TriPoint> holePoints) in holes)
                {
                    if (c.Contains(h) == ContainmentType.Contains)
                        tcx.AddHole(holePoints);
                }

                sweep.Triangulate(tcx);

                List<Triangle> r = tcx.GetTriangles();

                // Scale and offset triangles
                output.AddRange(r);
            }            
        }

        /// <summary>
        /// Performs basic checks to determine if the object represents a valid shape.
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            foreach (Contour contour in Contours)
            {
                if (contour.Edges.Count > 0)
                {
                    Vector2D corner = contour.Edges.Last().Point(1);
                    foreach (Edge edge in contour.Edges)
                    {
                        if (edge == null)
                            return false;
                        if (edge.Point(0) != corner)
                            return false;

                        corner = edge.Point(1);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the total number of edges in the current <see cref="Shape"/>.
        /// </summary>
        /// <returns></returns>
        public int GetEdgeCount()
        {
            int total = 0;
            foreach (Contour contour in Contours)
                total += contour.Edges.Count;

            return total;
        }

        public void Scale(float scale)
        {
            Scale(new Vector2F(scale));
        }

        public void Scale(Vector2F scale)
        {
            Vector2D dScale = (Vector2D)scale;

            foreach (Contour contour in Contours)
            {
                foreach (Edge e in contour.Edges)
                {
                    for (int i = 0; i < e.P.Length; i++)
                        e.P[i] *= dScale;
                }
            }
        }

        public void Offset(Vector2F offset)
        {
            Vector2D dOffset = (Vector2D)offset;

            foreach (Contour contour in Contours)
            {
                foreach (Edge e in contour.Edges)
                {
                    for (int i = 0; i < e.P.Length; i++)
                        e.P[i] += dOffset;
                }
            }
        }

        public void ScaleAndOffset(Vector2F offset, float scale)
        {
            ScaleAndOffset(offset, new Vector2F(scale));  
        }

        public void ScaleAndOffset(Vector2F offset, Vector2F scale)
        {
            Vector2D dOffset = (Vector2D)offset;
            Vector2D dScale = (Vector2D)scale;

            foreach (Contour contour in Contours)
            {
                foreach (Edge e in contour.Edges)
                {
                    for (int i = 0; i < e.P.Length; i++)
                    {
                        e.P[i] *= dScale;
                        e.P[i] += dOffset;
                    }
                }
            }
        }

        public bool Contains(Shape other)
        {
            foreach(Contour contour in Contours)
            {
                foreach(Contour otherContour in other.Contours)
                {
                    if (contour.Contains(otherContour) != ContainmentType.Contains)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Tests whether or not the current <see cref="Shape"/> contains the provided double-precision point.  
        /// </summary>
        /// <param name="point">The point to be tested.</param>
        /// <returns></returns>
        public bool Contains(Vector2D point)
        {
            // Check hole contours first.
            foreach (Contour c in Contours)
            {
                if (c.GetWinding() == -1)
                {
                    if (c.Contains(point))
                        return true;
                }
            }

            // Now check main contour(s)
            foreach (Contour c in Contours)
            {
                if (c.GetWinding() > -1)
                {
                    if (c.Contains(point))
                        return false;
                }
            }

            return false;
        }

        public bool Contains(Vector2F point)
        {
            Vector2D dPoint = (Vector2D)point;
            return Contains(dPoint);
        }
    }
}
