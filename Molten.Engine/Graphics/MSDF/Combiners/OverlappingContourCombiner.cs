﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="ES">EdgeSelector type.</typeparam>
    /// <typeparam name="DT">Distance type. e.g. double, MultiDistance or MultiAndTrueDistance.</typeparam>
    /// <typeparam name="EC">Edge cache type.</typeparam>
    public class OverlappingContourCombiner<ES, DT> : ContourCombiner<ES, DT>
         where ES : EdgeSelector<DT>, new()
        where DT : unmanaged
    {
        Vector2D p;
        List<int> windings;
        List<ES> edgeSelectors;

        public OverlappingContourCombiner(MsdfShape shape)
        {
            windings = new List<int>(shape.Contours.Count);
            foreach (Contour contour in shape.Contours)
                windings.Add(contour.winding());

            edgeSelectors = new List<ES>(shape.Contours.Count);
            for (int i = 0; i < shape.Contours.Count; i++)
                edgeSelectors.Add(new ES());
        }

        public override void reset(ref Vector2D p)
        {
            this.p = p;
            foreach (EdgeSelector<DT> contourEdgeSelector in edgeSelectors)
                contourEdgeSelector.reset(ref p);
        }

        public override ES edgeSelector(int i)
        {
            return edgeSelectors[i];
        }

        public override DT distance()
        {
            int contourCount = edgeSelectors.Count;
            ES shapeEdgeSelector = new ES();
            ES innerEdgeSelector = new ES();
            ES outerEdgeSelector = new ES();
            shapeEdgeSelector.reset(ref p);
            innerEdgeSelector.reset(ref p);
            outerEdgeSelector.reset(ref p);
            for (int i = 0; i < contourCount; ++i)
            {
                DT edgeDistance = edgeSelectors[i].distance();
                shapeEdgeSelector.merge(edgeSelectors[i]);
                if (windings[i] > 0 && edgeSelectors[i].resolveDistance(edgeDistance) >= 0)
                    innerEdgeSelector.merge(edgeSelectors[i]);
                if (windings[i] < 0 && edgeSelectors[i].resolveDistance(edgeDistance) <= 0)
                    outerEdgeSelector.merge(edgeSelectors[i]);
            }

            DT shapeDistance = shapeEdgeSelector.distance();
            DT innerDistance = innerEdgeSelector.distance();
            DT outerDistance = outerEdgeSelector.distance();
            double innerScalarDistance = shapeEdgeSelector.resolveDistance(innerDistance);
            double outerScalarDistance = shapeEdgeSelector.resolveDistance(outerDistance);
            DT distance = new DT();
            shapeEdgeSelector.initDistance(ref distance);

            int winding = 0;
            if (innerScalarDistance >= 0 && Math.Abs(innerScalarDistance) <= Math.Abs(outerScalarDistance))
            {
                distance = innerDistance;
                winding = 1;
                for (int i = 0; i < contourCount; ++i)
                    if (windings[i] > 0)
                    {
                        DT contourDistance = edgeSelectors[i].distance();
                        if (Math.Abs(edgeSelectors[i].resolveDistance(contourDistance)) < Math.Abs(outerScalarDistance) && edgeSelectors[i].resolveDistance(contourDistance) > edgeSelectors[i].resolveDistance(distance))
                            distance = contourDistance;
                    }
            }
            else if (outerScalarDistance <= 0 && Math.Abs(outerScalarDistance) < Math.Abs(innerScalarDistance))
            {
                distance = outerDistance;
                winding = -1;
                for (int i = 0; i < contourCount; ++i)
                    if (windings[i] < 0)
                    {
                        DT contourDistance = edgeSelectors[i].distance();
                        if (Math.Abs(edgeSelectors[i].resolveDistance(contourDistance)) < Math.Abs(innerScalarDistance) && edgeSelectors[i].resolveDistance(contourDistance) < edgeSelectors[i].resolveDistance(distance))
                            distance = contourDistance;
                    }
            }
            else
                return shapeDistance;

            for (int i = 0; i < contourCount; ++i)
            {
                if (windings[i] != winding)
                {
                    DT contourDistance = edgeSelectors[i].distance();
                    if (edgeSelectors[i].resolveDistance(contourDistance) * edgeSelectors[i].resolveDistance(distance) >= 0 && Math.Abs(edgeSelectors[i].resolveDistance(contourDistance)) < Math.Abs(edgeSelectors[i].resolveDistance(distance)))
                        distance = contourDistance;
                }
            }
            if (shapeEdgeSelector.resolveDistance(distance) == shapeEdgeSelector.resolveDistance(shapeDistance))
                distance = shapeDistance;
            return distance;
        }
    }
}
