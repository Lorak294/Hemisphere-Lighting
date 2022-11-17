using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

namespace GKProj2
{
    public class Edge
    {
        //private Vert _v1, _v2;
        private static int _idProvider = 1;
        public int Id { get; }
        public Vert V1 { get; set; }
        public Vert V2 { get; set; }
        public double CurrentDrawingX { get; set; }

        private Pen pen;
        public double? Slope 
        { 
            get 
            { 
                if (V1.DispY == V2.DispY) 
                    return null;
                else 
                    return (double)(V2.DispX - V1.DispX) / (double)(V2.DispY - V1.DispY); 
            } 
        }
        public int YDispMax { get { return (int)(Math.Max(V1.DispY, V2.DispY)); } }
        public int YDispMin { get { return (int)(Math.Min(V1.DispY, V2.DispY)); } }
        public int XDispMax { get { return (int)(Math.Max(V1.DispX, V2.DispX)); } }
        public int XDispMin { get { return (int)(Math.Min(V1.DispX, V2.DispX)); } }

        public Edge(Vert v1, Vert v2)
        {
            Id = _idProvider++;
            V1 = v1;
            V2 = v2;
            pen = new Pen(Brushes.Black);
            ResetCurrentDrawingX();
        }

        public void Draw(Bitmap canvas)
        {
            Pen pen = new Pen(Brushes.Black);
            using(Graphics g = Graphics.FromImage(canvas))
            {
                g.DrawLine(pen,V1.DispX, V1.DispY,V2.DispX, V2.DispY);
            }
        }

        public void ResetCurrentDrawingX()
        {
            if (V1.Y < V2.Y)
                CurrentDrawingX = V1.DispX;
            else if (V2.Y < V1.Y)
                CurrentDrawingX = V2.DispX;
            else
                CurrentDrawingX = Math.Min(V1.DispX, V2.DispX);
        }
        
        public Vector InterpolateNormalVectorInPoint(Point3D p)
        {
            double length = MathFunctions.Distance(V1.ToDispPoint3D(), V2.ToDispPoint3D(),false);
            double alfa = MathFunctions.Distance(p, V1.ToDispPoint3D(), false)/length;

            Vector finalNormal  = new Vector(
                V1.NormVector.x * (1.0 - alfa) + V2.NormVector.x * alfa,
                V1.NormVector.y * (1.0 - alfa) + V2.NormVector.y * alfa,
                V1.NormVector.z * (1.0 - alfa) + V2.NormVector.z * alfa);

            finalNormal.Normalize();
            return finalNormal;
        }
        
        public NormalizedColor InterpolateColorInPoint(Point3D p)
        {
            double length = MathFunctions.Distance(V1.ToDispPoint3D(), V2.ToDispPoint3D(), false);
            double alfa = MathFunctions.Distance(p, V1.ToDispPoint3D(), false) / length;

            return new NormalizedColor(
                V1.Color!.Value.r * (1.0 - alfa) + V2.Color!.Value.r * alfa,
                V1.Color.Value.g * (1.0 - alfa) + V2.Color.Value.g * alfa,
                V1.Color.Value.b * (1.0 - alfa) + V2.Color.Value.b * alfa);
        }
    }
}
