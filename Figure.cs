using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Formats.Asn1;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GKProj2
{
    public class Figure
    {
        private static int _idProvider = 1;
        public int Id { get; }
        private List<Vert> vertList;
        private LinkedList<Edge> edgeList;
        private List<NormalizedColor?> vertColors;

        public Figure(List<Vert> vertList)
        {
            if (vertList.Count < 3)
                throw new ArgumentException("too few verts were given while creating new figure");


            this.vertList = new List<Vert>(vertList);
            this.edgeList = new LinkedList<Edge>();
            this.vertColors = new List<NormalizedColor?>();

            for (int i = 1; i < vertList.Count; i++)
            {
                edgeList.AddLast(new Edge(vertList[i - 1], vertList[i]));
            }

            edgeList.AddLast(new Edge(vertList.Last(), vertList.First()));

            Id = _idProvider++;
        }
        
        
        // ================================= DRAWING FUNCTIONS ================================================
        public void DrawOutline(Bitmap canvas)
        {
            foreach (Edge e in edgeList)
            {
                e.Draw(canvas);
            }
        }
        public void FillTriangle(LockBitmap lockBitmapCanvas)
        {
            // calculating colors for interpolation
            if (!DrawingArgs.vecInterpolation)
                CalcVertColors();

            // get Y range for itarating
            var yDispRange = GetDispYRange();
            int yMin = (int)yDispRange.minY;
            int yMax = (int)yDispRange.MaxY;

            // init ET
            List<Edge>[] edgeTable = InitEdgeTable(yMin, yMax);
            List<Edge> activeEdges = new List<Edge>();

            for (int yIdx = 0; yIdx < yMax - yMin + 1; yIdx++)
            {
                if (edgeTable[yIdx].Count > 0)
                {
                    // adding new active edges
                    activeEdges.AddRange(edgeTable[yIdx]);
                }
                activeEdges = activeEdges.OrderBy(e => e.Slope == null ? double.MinValue : e.CurrentDrawingX).ToList(); // sort and put horizontal edges in the front of the list

                // remove all edges which are not horizontal (to avoid situation with two edges with same drawingX)
                activeEdges.RemoveAll(e => e.Slope != null && (int)e.YDispMax == yIdx + yMin);

                FillScanline(lockBitmapCanvas, activeEdges, yIdx + yMin);
                // remove all edges which will be unused in next iteration
                activeEdges.RemoveAll(e => e.Slope == null);
            }
        }
        private void FillScanline(LockBitmap lockBitmapCanvas, List<Edge> activeEdges, int y)
        {
            Color finalColor;
            int i = 0;
            // fill horizontal lines first
            while (i < activeEdges.Count && activeEdges[i].Slope == null)
            {
                for (int x = activeEdges[i].XDispMin; x < activeEdges[i].XDispMax; x++)
                {
                    double z = CalcZofDispPoint(x, y);
                    finalColor = GetFinalColor(activeEdges[i], new Point3D(x, y, z));
                    lockBitmapCanvas.SetPixel(x, y, finalColor);
                }
                i++;
            }
            // fill spaces between pairs of edges
            while (i < activeEdges.Count)
            {
                // set first pixel which is on activeEdges[i]
                int xBeg = (int)Math.Round(activeEdges[i].CurrentDrawingX);
                finalColor = GetFinalColor(activeEdges[i], new Point3D(xBeg, y, CalcZofDispPoint(xBeg, y)));
                lockBitmapCanvas.SetPixel(xBeg, y, finalColor);

                //set pixels inbetween edges
                xBeg++;
                int xEnd = (int)Math.Round(activeEdges[i + 1].CurrentDrawingX);
                for (int x = xBeg; x < xEnd; x++)
                {
                    finalColor = GetFinalColor(null, new Point3D(x, y, CalcZofDispPoint(x, y)));
                    lockBitmapCanvas.SetPixel(x, y, finalColor);
                }
                // set last pixel which is on activeEdges[i+1]
                finalColor = GetFinalColor(activeEdges[i + 1], new Point3D(xEnd, y, CalcZofDispPoint(xEnd, y)));
                lockBitmapCanvas.SetPixel(xEnd, y, finalColor);

                // update x-es for the edges
                activeEdges[i].CurrentDrawingX += activeEdges[i].Slope!.Value;
                activeEdges[i + 1].CurrentDrawingX += activeEdges[i + 1].Slope!.Value;

                i += 2;
            }
        }
        
        // ================================= CALCULATIONS FUNCTIONS ================================================
        private Vector InterpolateNormVector(Point3D point, bool r3)
        {
            TriangleInterpolator interpolator = new TriangleInterpolator(
                vertList[0].ToDispPoint3D(), 
                vertList[1].ToDispPoint3D(), 
                vertList[2].ToDispPoint3D(), 
                point, 
                r3);

            Vector finalNorm = new Vector(
                interpolator.Interpolate(vertList[0].NormVector.x, vertList[1].NormVector.x, vertList[2].NormVector.x),
                interpolator.Interpolate(vertList[0].NormVector.y, vertList[1].NormVector.y, vertList[2].NormVector.y),
                interpolator.Interpolate(vertList[0].NormVector.z, vertList[1].NormVector.z, vertList[2].NormVector.z)
                );

            finalNorm.Normalize();
            return finalNorm;
        }
        private NormalizedColor InterpolateColors(NormalizedColor c1, NormalizedColor c2, NormalizedColor c3, Point3D point, bool r3)
        {
            TriangleInterpolator interpolator = new TriangleInterpolator(
                vertList[0].ToDispPoint3D(),
                vertList[1].ToDispPoint3D(),
                vertList[2].ToDispPoint3D(),
                point,
                r3);

            return new NormalizedColor(
                interpolator.Interpolate(c1.r, c2.r, c3.r),
                interpolator.Interpolate(c1.g, c2.g, c3.g),
                interpolator.Interpolate(c1.b, c2.b, c3.b)
                );
        }
        public double CalcZofDispPoint(double px, double py)
        {
            double dx1 = px - vertList[0].DispX;
            double dy1 = py - vertList[0].DispY;
            double dx2 = vertList[1].DispX - vertList[0].DispX;
            double dy2 = vertList[1].DispY - vertList[0].DispY;
            double dx3 = vertList[2].DispX - vertList[0].DispX;
            double dy3 = vertList[2].DispY - vertList[0].DispY;

            return vertList[0].DispZ +
                ((dy1 * dx3 - dx1 * dy3) * (vertList[1].DispZ - vertList[0].DispZ) +
                (dx1 * dy2 - dy1 * dx2) * (vertList[2].DispZ - vertList[0].DispZ))
                / (dx3 * dy2 - dx2 * dy3);

            //TriangleInterpolator interpolator = new TriangleInterpolator(vertList[0].ToDispPoint3D(), vertList[1].ToDispPoint3D(), vertList[2].ToDispPoint3D(), new Point3D(px, py), false);
            //return interpolator.Interpolate(vertList[0].DispZ, vertList[1].DispZ, vertList[2].DispZ);
        }
        public (double minY,double MaxY) GetDispYRange()
        {
            double minY = vertList[0].DispY;
            double maxY = vertList[1].DispY;
            foreach(Vert v in vertList)
            {
                if (v.DispY < minY) minY = v.DispY;
                if (v.DispY > maxY) maxY = v.DispY;
            }
            return(minY,maxY);
        }
        public void CalcVertColors()
        {
            foreach(Vert v in vertList)
            {
                Vector lightVector = new Vector(v.ToDispPoint3D(), DrawingArgs.lightPosition!.ToDispPoint3D());
                lightVector.Normalize();

                var finalColor = MathFunctions.CalculateFinalColor(
                    DrawingArgs.useNormalMap? MathFunctions.ApplyNormalMap(v.NormVector,v.ToDispPoint3D()): v.NormVector,
                    lightVector,
                    new Vector(0, 0, 1),
                    DrawingArgs.m, DrawingArgs.kd, DrawingArgs.ks,
                    ColorConverter.RGBToStandarized(DrawingArgs.GetPixelColor(v.DispX,v.DispY)),
                    ColorConverter.RGBToStandarized(DrawingArgs.lightColor));
                v.Color = finalColor;
            }
        }
        public List<Edge>[] InitEdgeTable(int yMin, int yMax)
        {
            // init ET
            List<Edge>[] edgeTable = new List<Edge>[yMax - yMin + 1];
            for (int i = 0; i < edgeTable.Length; i++) edgeTable[i] = new List<Edge>();
            // add edges to ET
            foreach (Edge e in edgeList)
            {
                e.ResetCurrentDrawingX();
                edgeTable[e.YDispMin - yMin].Add(e);
            }
            return edgeTable;
        }
        public Color GetFinalColor(Edge? simplifier, Point3D point)
        {
            NormalizedColor finalColor;
            if (DrawingArgs.vecInterpolation)
            {
                var vNorm =
                    simplifier == null ?
                    InterpolateNormVector(point, DrawingArgs.r3) :
                    simplifier.InterpolateNormalVectorInPoint(point);

                if (DrawingArgs.useNormalMap)
                {
                    vNorm = MathFunctions.ApplyNormalMap(vNorm, point);
                }

                Vector lightVector = new Vector(point, DrawingArgs.lightPosition!.ToDispPoint3D());
                lightVector.Normalize();

                finalColor = MathFunctions.CalculateFinalColor(
                    vNorm,
                    lightVector,
                    new Vector(0, 0, 1),
                    DrawingArgs.m, DrawingArgs.kd, DrawingArgs.ks,
                    ColorConverter.RGBToStandarized(DrawingArgs.GetPixelColor((int)point.x,(int)point.y)),
                    ColorConverter.RGBToStandarized(DrawingArgs.lightColor));
            }
            else
            {
                finalColor = simplifier == null ?
                    InterpolateColors(vertList[0].Color!.Value, vertList[1].Color!.Value, vertList[2].Color!.Value, point, DrawingArgs.r3) :
                    simplifier.InterpolateColorInPoint(point);
            }

            return ColorConverter.StandarizedToRGB(finalColor);
        }
    }
}
